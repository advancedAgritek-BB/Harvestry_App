using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Domain.Entities;
using Harvestry.Telemetry.Domain.Enums;
using Harvestry.Telemetry.Infrastructure.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Npgsql.Replication;
using Npgsql.Replication.PgOutput;
using Npgsql.Replication.PgOutput.Messages;
using NpgsqlTypes;

namespace Harvestry.Telemetry.Infrastructure.Workers;

/// <summary>
/// Listens to PostgreSQL logical replication and pushes telemetry changes to SignalR subscribers.
/// </summary>
public sealed class WalFanoutWorker : BackgroundService
{
    private readonly ITelemetryRealtimeDispatcher _dispatcher;
    private readonly TelemetryWalReplicationOptions _options;
    private readonly ILogger<WalFanoutWorker> _logger;

    public WalFanoutWorker(
        ITelemetryRealtimeDispatcher dispatcher,
        IOptions<TelemetryWalReplicationOptions> options,
        ILogger<WalFanoutWorker> logger)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("WAL fan-out worker disabled via configuration.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            _logger.LogWarning("WAL fan-out worker cannot start because no replication connection string was provided.");
            return;
        }

        var slot = new PgOutputReplicationSlot(_options.SlotName);
        var walOptions = new PgOutputReplicationOptions(
            _options.PublicationName,
            protocolVersion: 1,
            binary: false,
            streaming: false,
            messages: false,
            twoPhase: false);

        var statusInterval = TimeSpan.FromSeconds(Math.Clamp(_options.StatusIntervalSeconds, 1, 60));
        var maxRetryDelaySeconds = Math.Max(1, _options.MaxRetryDelaySeconds);
        var baseRetryDelay = TimeSpan.FromSeconds(Math.Clamp(_options.RetryDelaySeconds, 1, maxRetryDelaySeconds));
        var retryDelay = baseRetryDelay;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var connection = new LogicalReplicationConnection(_options.ConnectionString!);
                await connection.Open(stoppingToken).ConfigureAwait(false);
                retryDelay = baseRetryDelay;

                _logger.LogInformation(
                    "Logical replication connected using slot '{Slot}' and publication '{Publication}'.",
                    _options.SlotName,
                    _options.PublicationName);

                var lastStatusSentAt = DateTime.UtcNow;
                var lastReceived = connection.LastReceivedLsn;
                var lastApplied = connection.LastAppliedLsn;
                var lastFlushed = connection.LastFlushedLsn;

                await foreach (var message in connection.StartReplication(slot, walOptions, cancellationToken: stoppingToken))
                {
                    switch (message)
                    {
                        case InsertMessage insert when string.Equals(insert.Relation.RelationName, "sensor_readings", StringComparison.Ordinal):
                            lastReceived = insert.WalEnd;
                            await DispatchReplicationTupleAsync(insert.Relation, insert.NewRow, stoppingToken).ConfigureAwait(false);
                            break;

                        case UpdateMessage update when string.Equals(update.Relation.RelationName, "sensor_readings", StringComparison.Ordinal):
                            lastReceived = update.WalEnd;
                            await DispatchReplicationTupleAsync(update.Relation, update.NewRow, stoppingToken).ConfigureAwait(false);
                            break;

                        case CommitMessage commit:
                            lastApplied = commit.WalEnd;
                            lastFlushed = commit.WalEnd;
                            await SendStatusUpdateAsync(connection, lastReceived, lastApplied, lastFlushed, stoppingToken).ConfigureAwait(false);
                            lastStatusSentAt = DateTime.UtcNow;
                            break;

                    }

                    var now = DateTime.UtcNow;
                    if (now - lastStatusSentAt >= statusInterval)
                    {
                        await SendStatusUpdateAsync(connection, lastReceived, lastApplied, lastFlushed, stoppingToken).ConfigureAwait(false);
                        lastStatusSentAt = now;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logical replication stream disconnected. Retrying in {DelaySeconds} seconds...", retryDelay.TotalSeconds);

                try
                {
                    await Task.Delay(retryDelay, stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                retryDelay = TimeSpan.FromSeconds(Math.Min(retryDelay.TotalSeconds * 2, maxRetryDelaySeconds));
            }
        }

        _logger.LogInformation("WAL fan-out worker stopping.");
    }

    private async Task DispatchReplicationTupleAsync(
        RelationMessage relation,
        ReplicationTuple tuple,
        CancellationToken cancellationToken)
    {
        var reading = await TryMapSensorReadingAsync(relation, tuple, cancellationToken).ConfigureAwait(false);
        if (reading != null)
        {
            await _dispatcher.PublishWalEventAsync(reading, cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task<SensorReading?> TryMapSensorReadingAsync(
        RelationMessage relation,
        ReplicationTuple tuple,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(relation.RelationName, "sensor_readings", StringComparison.Ordinal))
        {
            return null;
        }

        DateTimeOffset time = DateTimeOffset.MinValue;
        Guid streamId = Guid.Empty;
        double value = 0;
        short qualityCode = 0;
        DateTimeOffset? sourceTimestamp = null;
        DateTimeOffset ingestionTimestamp = DateTimeOffset.UtcNow;
        string? messageId = null;
        Dictionary<string, object>? metadata = null;

        var index = 0;

        await foreach (var token in tuple.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (index >= relation.Columns.Count)
            {
                break;
            }

            var column = relation.Columns[index++];

            if (token.IsDBNull)
            {
                continue;
            }

            switch (column.ColumnName)
            {
                case "time":
                    time = await token.Get<DateTimeOffset>(cancellationToken).ConfigureAwait(false);
                    break;
                case "stream_id":
                    streamId = await token.Get<Guid>(cancellationToken).ConfigureAwait(false);
                    break;
                case "value":
                    value = await token.Get<double>(cancellationToken).ConfigureAwait(false);
                    break;
                case "quality_code":
                    qualityCode = await token.Get<short>(cancellationToken).ConfigureAwait(false);
                    break;
                case "source_timestamp":
                    sourceTimestamp = await token.Get<DateTimeOffset>(cancellationToken).ConfigureAwait(false);
                    break;
                case "ingestion_timestamp":
                    ingestionTimestamp = await token.Get<DateTimeOffset>(cancellationToken).ConfigureAwait(false);
                    break;
                case "message_id":
                    messageId = await token.Get<string>(cancellationToken).ConfigureAwait(false);
                    break;
                case "metadata":
                    var json = await token.Get<string>(cancellationToken).ConfigureAwait(false);
                    var deserialized = DeserializeMetadata(json);
                    if (deserialized is not null)
                    {
                        metadata = deserialized
                            .Where(kvp => kvp.Value is not null)
                            .ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value!);
                    }
                    break;
            }
        }

        if (streamId == Guid.Empty)
        {
            return null;
        }

        return SensorReading.FromPersistence(
            time,
            streamId,
            value,
            (QualityCode)qualityCode,
            sourceTimestamp,
            ingestionTimestamp,
            messageId,
            metadata);
    }

    private static async Task SendStatusUpdateAsync(
        LogicalReplicationConnection connection,
        NpgsqlLogSequenceNumber lastReceived,
        NpgsqlLogSequenceNumber lastApplied,
        NpgsqlLogSequenceNumber lastFlushed,
        CancellationToken cancellationToken)
    {
        connection.SetReplicationStatus(lastReceived);
        connection.LastAppliedLsn = lastApplied;
        connection.LastFlushedLsn = lastFlushed;
        await connection.SendStatusUpdate(cancellationToken).ConfigureAwait(false);
    }

    private static Dictionary<string, object>? DeserializeMetadata(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in document.RootElement.EnumerateObject())
            {
                dictionary[property.Name] = ConvertJsonElement(property.Value)!;
            }

            return dictionary;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static object? ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.False => false,
            JsonValueKind.True => true,
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Array => element.EnumerateArray().Select(ConvertJsonElement).ToList(),
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(p => p.Name, p => ConvertJsonElement(p.Value)),
            _ => element.ToString()
        };
    }
}
