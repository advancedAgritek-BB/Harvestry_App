using System.Linq;
using System.Text.Json;
using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Domain.Entities;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Harvestry.Telemetry.Infrastructure.Repositories;

/// <summary>
/// Persists telemetry ingestion errors for diagnostics.
/// </summary>
public sealed class IngestionErrorRepository : IIngestionErrorRepository
{
    private readonly ITelemetryConnectionFactory _connectionFactory;
    private readonly ILogger<IngestionErrorRepository> _logger;

    public IngestionErrorRepository(
        ITelemetryConnectionFactory connectionFactory,
        ILogger<IngestionErrorRepository> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task LogAsync(IEnumerable<IngestionError> errors, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(errors);

        var errorList = errors.ToList();
        if (errorList.Count == 0)
        {
            return;
        }

        const string insertSql = @"
            INSERT INTO ingestion_errors (
                id, site_id, session_id, equipment_id,
                protocol, error_type, error_message, raw_payload, occurred_at)
            VALUES (
                @Id, @SiteId, @SessionId, @EquipmentId,
                @Protocol, @ErrorType, @ErrorMessage, @RawPayload, @OccurredAt)
        ";

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);

        foreach (var error in errorList)
        {
            await using var command = new NpgsqlCommand(insertSql, connection);
            command.Parameters.AddWithValue("@Id", error.Id);
            command.Parameters.AddWithValue("@SiteId", error.SiteId);
            command.Parameters.AddWithValue("@SessionId", (object?)error.SessionId ?? DBNull.Value);
            command.Parameters.AddWithValue("@EquipmentId", (object?)error.EquipmentId ?? DBNull.Value);
            command.Parameters.AddWithValue("@Protocol", error.Protocol.ToString());
            command.Parameters.AddWithValue("@ErrorType", error.ErrorType.ToString());
            command.Parameters.AddWithValue("@ErrorMessage", error.ErrorMessage);

            if (error.RawPayload is { Count: > 0 })
            {
                command.Parameters.AddWithValue("@RawPayload", NpgsqlTypes.NpgsqlDbType.Jsonb, JsonSerializer.Serialize(error.RawPayload));
            }
            else
            {
                command.Parameters.AddWithValue("@RawPayload", NpgsqlTypes.NpgsqlDbType.Jsonb, DBNull.Value);
            }

            command.Parameters.AddWithValue("@OccurredAt", error.OccurredAt.UtcDateTime);

            try
            {
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (PostgresException ex)
            {
                _logger.LogError(ex, "Failed to persist ingestion error {ErrorId}", error.Id);
                throw;
            }
        }
    }
}
