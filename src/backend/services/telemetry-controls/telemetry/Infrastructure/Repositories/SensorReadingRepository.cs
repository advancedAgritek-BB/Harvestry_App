using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Domain.Entities;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Harvestry.Telemetry.Infrastructure.Repositories;

/// <summary>
/// TimescaleDB-backed repository for persisting sensor readings in bulk.
/// </summary>
public sealed class SensorReadingRepository : ISensorReadingRepository
{
    private readonly ITelemetryConnectionFactory _connectionFactory;
    private readonly ILogger<SensorReadingRepository> _logger;

    public SensorReadingRepository(
        ITelemetryConnectionFactory connectionFactory,
        ILogger<SensorReadingRepository> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task BulkInsertAsync(IEnumerable<SensorReading> readings, CancellationToken cancellationToken = default)
    {
        if (readings == null)
        {
            throw new ArgumentNullException(nameof(readings));
        }

        var readingList = readings as IList<SensorReading> ?? readings.ToList();
        if (readingList.Count == 0)
        {
            return;
        }

        const string copyCommand = @"
            COPY sensor_readings (
                time, stream_id, value, quality_code,
                source_timestamp, ingestion_timestamp, message_id, metadata
            ) FROM STDIN (FORMAT BINARY)";

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var writer = await connection.BeginBinaryImportAsync(copyCommand, cancellationToken).ConfigureAwait(false);

        foreach (var reading in readingList)
        {
            await writer.StartRowAsync(cancellationToken).ConfigureAwait(false);
            await writer.WriteAsync(reading.Time.UtcDateTime, NpgsqlDbType.TimestampTz, cancellationToken).ConfigureAwait(false);
            await writer.WriteAsync(reading.StreamId, NpgsqlDbType.Uuid, cancellationToken).ConfigureAwait(false);
            await writer.WriteAsync(reading.Value, NpgsqlDbType.Double, cancellationToken).ConfigureAwait(false);
            await writer.WriteAsync((short)reading.QualityCode, NpgsqlDbType.Smallint, cancellationToken).ConfigureAwait(false);

            if (reading.SourceTimestamp.HasValue)
            {
                await writer.WriteAsync(reading.SourceTimestamp.Value.UtcDateTime, NpgsqlDbType.TimestampTz, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await writer.WriteNullAsync(cancellationToken).ConfigureAwait(false);
            }

            await writer.WriteAsync(reading.IngestionTimestamp.UtcDateTime, NpgsqlDbType.TimestampTz, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(reading.MessageId))
            {
                await writer.WriteAsync(reading.MessageId, NpgsqlDbType.Varchar, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await writer.WriteNullAsync(cancellationToken).ConfigureAwait(false);
            }

            if (reading.Metadata is { Count: > 0 })
            {
                var metadataJson = JsonSerializer.Serialize(reading.Metadata);
                await writer.WriteAsync(metadataJson, NpgsqlDbType.Jsonb, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await writer.WriteNullAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        await writer.CompleteAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogDebug("Bulk inserted {Count} sensor readings via COPY", readingList.Count);
    }
}
