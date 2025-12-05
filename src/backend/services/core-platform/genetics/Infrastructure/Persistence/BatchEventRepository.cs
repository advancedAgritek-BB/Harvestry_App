using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Domain.Entities;
using Harvestry.Genetics.Domain.Enums;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Text.Json;

namespace Harvestry.Genetics.Infrastructure.Persistence;

/// <summary>
/// Repository for BatchEvent operations
/// </summary>
public class BatchEventRepository : IBatchEventRepository
{
    private readonly GeneticsDbContext _dbContext;
    private readonly ILogger<BatchEventRepository> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public BatchEventRepository(GeneticsDbContext dbContext, ILogger<BatchEventRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<BatchEvent> CreateAsync(BatchEvent batchEvent, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO genetics.batch_events (
                id, site_id, batch_id, event_type, event_data, performed_by_user_id, performed_at, notes, created_at
            ) VALUES (
                @Id, @SiteId, @BatchId, @EventType, @EventData::jsonb, @PerformedByUserId, @PerformedAt, @Notes, @CreatedAt
            )";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, batchEvent.SiteId, cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Id", batchEvent.Id);
        command.Parameters.AddWithValue("SiteId", batchEvent.SiteId);
        command.Parameters.AddWithValue("BatchId", batchEvent.BatchId);
        command.Parameters.AddWithValue("EventType", batchEvent.EventType.ToString());
        command.Parameters.AddWithValue("EventData", JsonSerializer.Serialize(batchEvent.EventData, JsonOptions));
        command.Parameters.AddWithValue("PerformedByUserId", batchEvent.PerformedByUserId);
        command.Parameters.AddWithValue("PerformedAt", batchEvent.PerformedAt);
        command.Parameters.AddWithValue("Notes", (object?)batchEvent.Notes ?? DBNull.Value);
        command.Parameters.AddWithValue("CreatedAt", batchEvent.CreatedAt);

        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogDebug("Created batch event {EventId} for batch {BatchId}", batchEvent.Id, batchEvent.BatchId);
        return batchEvent;
    }

    public async Task<IReadOnlyList<BatchEvent>> GetByBatchIdAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, batch_id, event_type, event_data, performed_by_user_id, performed_at, notes, created_at
            FROM genetics.batch_events
            WHERE batch_id = @BatchId AND site_id = @SiteId
            ORDER BY performed_at DESC";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("BatchId", batchId);
        command.Parameters.AddWithValue("SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var events = new List<BatchEvent>();
        while (await reader.ReadAsync(cancellationToken))
        {
            events.Add(MapToBatchEvent(reader));
        }

        return events;
    }

    public async Task<IReadOnlyList<BatchEvent>> GetByEventTypeAsync(Guid batchId, EventType eventType, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, batch_id, event_type, event_data, performed_by_user_id, performed_at, notes, created_at
            FROM genetics.batch_events
            WHERE batch_id = @BatchId AND event_type = @EventType AND site_id = @SiteId
            ORDER BY performed_at DESC";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("BatchId", batchId);
        command.Parameters.AddWithValue("EventType", eventType.ToString());
        command.Parameters.AddWithValue("SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var events = new List<BatchEvent>();
        while (await reader.ReadAsync(cancellationToken))
        {
            events.Add(MapToBatchEvent(reader));
        }

        return events;
    }

    public async Task<IReadOnlyList<BatchEvent>> GetRecentEventsAsync(Guid siteId, int limit = 100, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, batch_id, event_type, event_data, performed_by_user_id, performed_at, notes, created_at
            FROM genetics.batch_events
            WHERE site_id = @SiteId
            ORDER BY performed_at DESC
            LIMIT @Limit";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("SiteId", siteId);
        command.Parameters.AddWithValue("Limit", limit);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var events = new List<BatchEvent>();
        while (await reader.ReadAsync(cancellationToken))
        {
            events.Add(MapToBatchEvent(reader));
        }

        return events;
    }

    private static BatchEvent MapToBatchEvent(NpgsqlDataReader reader)
    {
        var eventDataJson = reader.GetString(reader.GetOrdinal("event_data"));
        var eventData = JsonSerializer.Deserialize<Dictionary<string, object>>(eventDataJson, JsonOptions)
                        ?? new Dictionary<string, object>();

        return BatchEvent.FromPersistence(
            id: reader.GetGuid(reader.GetOrdinal("id")),
            siteId: reader.GetGuid(reader.GetOrdinal("site_id")),
            batchId: reader.GetGuid(reader.GetOrdinal("batch_id")),
            eventType: Enum.Parse<EventType>(reader.GetString(reader.GetOrdinal("event_type"))),
            eventData: eventData,
            performedByUserId: reader.GetGuid(reader.GetOrdinal("performed_by_user_id")),
            performedAt: reader.GetDateTime(reader.GetOrdinal("performed_at")),
            notes: reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes")),
            createdAt: reader.GetDateTime(reader.GetOrdinal("created_at"))
        );
    }
}
