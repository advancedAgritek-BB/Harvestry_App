using System.Text.Json;
using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Domain.Entities;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Harvestry.Telemetry.Infrastructure.Repositories;

/// <summary>
/// Npgsql-backed repository for ingestion session persistence.
/// </summary>
public sealed class IngestionSessionRepository : IIngestionSessionRepository
{
    private readonly ITelemetryConnectionFactory _connectionFactory;
    private readonly ILogger<IngestionSessionRepository> _logger;

    public IngestionSessionRepository(
        ITelemetryConnectionFactory connectionFactory,
        ILogger<IngestionSessionRepository> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task CreateAsync(IngestionSession session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        const string insertSql = @"
            INSERT INTO ingestion_sessions (
                id, site_id, equipment_id, protocol,
                started_at, last_heartbeat_at, ended_at,
                message_count, error_count, metadata)
            VALUES (
                @Id, @SiteId, @EquipmentId, @Protocol,
                @StartedAt, @LastHeartbeatAt, NULL,
                @MessageCount, @ErrorCount, @Metadata)
        ";

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = new NpgsqlCommand(insertSql, connection);

        command.Parameters.AddWithValue("@Id", session.Id);
        command.Parameters.AddWithValue("@SiteId", session.SiteId);
        command.Parameters.AddWithValue("@EquipmentId", session.EquipmentId);
        command.Parameters.AddWithValue("@Protocol", session.Protocol.ToString());
        command.Parameters.AddWithValue("@StartedAt", session.StartedAt.UtcDateTime);
        command.Parameters.AddWithValue("@LastHeartbeatAt", session.LastHeartbeatAt.UtcDateTime);
        command.Parameters.AddWithValue("@MessageCount", session.MessageCount);
        command.Parameters.AddWithValue("@ErrorCount", session.ErrorCount);

        if (session.Metadata is { Count: > 0 })
        {
            command.Parameters.AddWithValue("@Metadata", NpgsqlTypes.NpgsqlDbType.Jsonb, JsonSerializer.Serialize(session.Metadata));
        }
        else
        {
            command.Parameters.AddWithValue("@Metadata", NpgsqlTypes.NpgsqlDbType.Jsonb, DBNull.Value);
        }

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateHeartbeatAsync(Guid sessionId, DateTimeOffset heartbeatAt, CancellationToken cancellationToken = default)
    {
        const string updateSql = @"
            UPDATE ingestion_sessions
            SET last_heartbeat_at = @HeartbeatAt
            WHERE id = @SessionId";

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = new NpgsqlCommand(updateSql, connection);
        command.Parameters.AddWithValue("@SessionId", sessionId);
        command.Parameters.AddWithValue("@HeartbeatAt", heartbeatAt.UtcDateTime);

        var rows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        if (rows == 0)
        {
            _logger.LogWarning("Attempted to update heartbeat for missing session {SessionId}", sessionId);
        }
    }

    public async Task EndAsync(Guid sessionId, DateTimeOffset endedAt, CancellationToken cancellationToken = default)
    {
        const string updateSql = @"
            UPDATE ingestion_sessions
            SET ended_at = @EndedAt
            WHERE id = @SessionId";

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = new NpgsqlCommand(updateSql, connection);
        command.Parameters.AddWithValue("@SessionId", sessionId);
        command.Parameters.AddWithValue("@EndedAt", endedAt.UtcDateTime);

        var rows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        if (rows == 0)
        {
            _logger.LogWarning("Attempted to end missing session {SessionId}", sessionId);
        }
    }

    public async Task<int> EndStaleSessionsAsync(TimeSpan staleThreshold, DateTimeOffset endedAt, CancellationToken cancellationToken = default)
    {
        const string updateSql = @"
            UPDATE ingestion_sessions
            SET ended_at = @EndedAt
            WHERE ended_at IS NULL
              AND last_heartbeat_at < @Cutoff";

        var cutoff = endedAt - staleThreshold;

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = new NpgsqlCommand(updateSql, connection);
        command.Parameters.AddWithValue("@EndedAt", endedAt.UtcDateTime);
        command.Parameters.AddWithValue("@Cutoff", cutoff.UtcDateTime);

        var rows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        if (rows > 0)
        {
            _logger.LogInformation("Ended {Count} stale ingestion sessions", rows);
        }

        return rows;
    }
}
