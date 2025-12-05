using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Domain.Entities;
using Harvestry.Telemetry.Domain.Enums;
using Harvestry.Telemetry.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Harvestry.Telemetry.Infrastructure.Repositories;

/// <summary>
/// Npgsql-based telemetry query repository with TimescaleDB awareness.
/// </summary>
public sealed class TelemetryQueryRepository : ITelemetryQueryRepository
{
    private readonly ITelemetryConnectionFactory _connectionFactory;
    private readonly ILogger<TelemetryQueryRepository> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public TelemetryQueryRepository(
        ITelemetryConnectionFactory connectionFactory,
        ILogger<TelemetryQueryRepository> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<SensorReading>> GetReadingsAsync(
        Guid streamId,
        DateTimeOffset start,
        DateTimeOffset end,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        const string baseSql = @"
            SELECT time, stream_id, value, quality_code,
                   source_timestamp, ingestion_timestamp, message_id, metadata
            FROM sensor_readings
            WHERE stream_id = @StreamId
              AND time >= @Start
              AND time <= @End
            ORDER BY time";

        var sql = limit.HasValue ? baseSql + " LIMIT @Limit" : baseSql;

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@StreamId", streamId);
        command.Parameters.AddWithValue("@Start", start.UtcDateTime);
        command.Parameters.AddWithValue("@End", end.UtcDateTime);
        if (limit.HasValue)
        {
            command.Parameters.AddWithValue("@Limit", limit.Value);
        }

        var readings = new List<SensorReading>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            readings.Add(MapToSensorReading(reader));
        }

        return readings;
    }

    public async Task<SensorReading?> GetLatestReadingAsync(Guid streamId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT time, stream_id, value, quality_code,
                   source_timestamp, ingestion_timestamp, message_id, metadata
            FROM sensor_readings
            WHERE stream_id = @StreamId
            ORDER BY time DESC
            LIMIT 1";

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@StreamId", streamId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return MapToSensorReading(reader);
        }

        return null;
    }

    public async Task<IReadOnlyList<RollupData>> GetRollupsAsync(
        Guid streamId,
        DateTimeOffset start,
        DateTimeOffset end,
        RollupInterval interval,
        CancellationToken cancellationToken = default)
    {
        var viewName = interval switch
        {
            RollupInterval.OneMinute => "sensor_readings_1min",
            RollupInterval.FiveMinutes => "sensor_readings_5min",
            RollupInterval.OneHour => "sensor_readings_1hour",
            RollupInterval.Raw => throw new ArgumentException("Use GetReadingsAsync for raw data", nameof(interval)),
            _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, "Unsupported rollup interval")
        };

        var sql = $@"
            SELECT bucket, reading_count, avg_value, min_value, max_value,
                   stddev_value
            FROM {viewName}
            WHERE stream_id = @StreamId
              AND bucket >= @Start
              AND bucket <= @End
            ORDER BY bucket";

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@StreamId", streamId);
        command.Parameters.AddWithValue("@Start", start.UtcDateTime);
        command.Parameters.AddWithValue("@End", end.UtcDateTime);

        var rollups = new List<RollupData>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            rollups.Add(new RollupData
            {
                Bucket = reader.GetFieldValue<DateTimeOffset>(0),
                Interval = interval,
                SampleCount = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                AvgValue = reader.IsDBNull(2) ? double.NaN : reader.GetDouble(2),
                MinValue = reader.IsDBNull(3) ? double.NaN : reader.GetDouble(3),
                MaxValue = reader.IsDBNull(4) ? double.NaN : reader.GetDouble(4),
                StdDevValue = reader.IsDBNull(5) ? null : reader.GetDouble(5)
            });
        }

        return rollups;
    }

    public async Task<IReadOnlyList<(Guid SiteId, SensorReading Reading)>> GetReadingsSinceAsync(
        DateTimeOffset since,
        int limit,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT sr.time, sr.stream_id, sr.value, sr.quality_code,
                   sr.source_timestamp, sr.ingestion_timestamp, sr.message_id, sr.metadata,
                   ss.site_id
            FROM sensor_readings sr
            INNER JOIN sensor_streams ss ON ss.id = sr.stream_id
            WHERE sr.ingestion_timestamp > @Since
            ORDER BY sr.ingestion_timestamp ASC
            LIMIT @Limit";

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Since", since.UtcDateTime);
        command.Parameters.AddWithValue("@Limit", limit);

        var readings = new List<(Guid, SensorReading)>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var reading = MapToSensorReading(reader);
            var siteId = reader.GetGuid(8);
            readings.Add((siteId, reading));
        }

        return readings;
    }

    private static SensorReading MapToSensorReading(NpgsqlDataReader reader)
    {
        Dictionary<string, object>? metadata = null;
        if (!reader.IsDBNull(7))
        {
            try
            {
                metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(reader.GetFieldValue<string>(7), JsonOptions);
            }
            catch (JsonException)
            {
                // Ignore malformed metadata; return as null to keep ingest resilient.
            }
        }

        return SensorReading.FromPersistence(
            time: reader.GetFieldValue<DateTimeOffset>(0),
            streamId: reader.GetGuid(1),
            value: reader.GetDouble(2),
            qualityCode: (QualityCode)reader.GetInt16(3),
            sourceTimestamp: reader.IsDBNull(4) ? null : reader.GetFieldValue<DateTimeOffset>(4),
            ingestionTimestamp: reader.GetFieldValue<DateTimeOffset>(5),
            messageId: reader.IsDBNull(6) ? null : reader.GetString(6),
            metadata: metadata);
    }
}
