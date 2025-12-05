using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Domain.Entities;
using Npgsql;
using NpgsqlTypes;

namespace Harvestry.Telemetry.Application.Services;

/// <summary>
/// Service for enforcing message idempotency (deduplication).
/// Prevents duplicate sensor readings from being stored using message IDs.
/// </summary>
public class IdempotencyService : IIdempotencyService
{
    private readonly ITelemetryConnectionFactory _connectionFactory;

    public IdempotencyService(ITelemetryConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<bool> IsDuplicateAsync(
        Guid streamId,
        string messageId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(messageId))
        {
            return false;
        }

        const string query = @"
            SELECT EXISTS(
                SELECT 1
                FROM sensor_readings
                WHERE stream_id = @StreamId
                  AND message_id = @MessageId
                LIMIT 1
            )";

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@StreamId", streamId);
        command.Parameters.AddWithValue("@MessageId", messageId.Trim());

        var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return result is bool exists && exists;
    }

    public async Task<int> DeduplicateBatchAsync(
        ICollection<SensorReading> readings,
        CancellationToken cancellationToken = default)
    {
        if (readings == null || readings.Count == 0)
        {
            return 0;
        }

        var messageIdsByStream = readings
            .Where(r => !string.IsNullOrWhiteSpace(r.MessageId))
            .GroupBy(r => r.StreamId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(r => r.MessageId!.Trim())
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct(StringComparer.Ordinal)
                    .ToArray());

        if (messageIdsByStream.Count == 0)
        {
            return 0;
        }

        var duplicates = await GetDuplicatesByStreamAsync(messageIdsByStream, cancellationToken).ConfigureAwait(false);

        var duplicateCount = 0;
        foreach (var reading in readings.ToList())
        {
            if (string.IsNullOrWhiteSpace(reading.MessageId))
            {
                continue;
            }

            var key = (reading.StreamId, reading.MessageId!.Trim());
            if (!duplicates.Contains(key))
            {
                continue;
            }

            readings.Remove(reading);
            duplicateCount++;
        }

        return duplicateCount;
    }

    public async Task<HashSet<string>> GetDuplicatesAsync(
        Guid streamId,
        IEnumerable<string> messageIds,
        CancellationToken cancellationToken = default)
    {
        var ids = messageIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (ids.Length == 0)
        {
            return new HashSet<string>();
        }

        var result = await GetDuplicatesByStreamAsync(
            new Dictionary<Guid, string[]> { { streamId, ids } },
            cancellationToken).ConfigureAwait(false);

        return result
            .Where(tuple => tuple.StreamId == streamId)
            .Select(tuple => tuple.MessageId)
            .ToHashSet(StringComparer.Ordinal);
    }

    public async Task<HashSet<(Guid StreamId, string MessageId)>> GetDuplicatesByStreamAsync(
        IReadOnlyDictionary<Guid, string[]> messageIdsByStream,
        CancellationToken cancellationToken = default)
    {
        if (messageIdsByStream == null || messageIdsByStream.Count == 0)
        {
            return new HashSet<(Guid, string)>();
        }

        var pairs = messageIdsByStream
            .SelectMany(kvp => kvp.Value
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => (StreamId: kvp.Key, MessageId: v.Trim())))
            .ToArray();

        if (pairs.Length == 0)
        {
            return new HashSet<(Guid, string)>();
        }

        var streamIds = pairs.Select(p => p.StreamId).ToArray();
        var messageIds = pairs.Select(p => p.MessageId).ToArray();

        const string query = @"
            SELECT sr.stream_id, sr.message_id
            FROM sensor_readings sr
            JOIN unnest(@StreamIds::uuid[], @MessageIds::text[]) AS input(stream_id, message_id)
              ON sr.stream_id = input.stream_id AND sr.message_id = input.message_id";

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.Add("@StreamIds", NpgsqlDbType.Array | NpgsqlDbType.Uuid).Value = streamIds;
        command.Parameters.Add("@MessageIds", NpgsqlDbType.Array | NpgsqlDbType.Text).Value = messageIds;

        var duplicates = new HashSet<(Guid, string)>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            duplicates.Add((reader.GetGuid(0), reader.GetString(1)));
        }

        return duplicates;
    }
}

