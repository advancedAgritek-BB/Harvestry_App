using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Domain.Entities;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Harvestry.Genetics.Infrastructure.Persistence;

/// <summary>
/// Repository for BatchStageHistory operations
/// </summary>
public class BatchStageHistoryRepository : IBatchStageHistoryRepository
{
    private readonly GeneticsDbContext _dbContext;
    private readonly ILogger<BatchStageHistoryRepository> _logger;

    public BatchStageHistoryRepository(GeneticsDbContext dbContext, ILogger<BatchStageHistoryRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<BatchStageHistory> CreateAsync(BatchStageHistory history, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO genetics.batch_stage_history (
                id, batch_id, from_stage_id, to_stage_id, changed_by_user_id, changed_at, notes
            ) VALUES (
                @Id, @BatchId, @FromStageId, @ToStageId, @ChangedByUserId, @ChangedAt, @Notes
            )";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, null, cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Id", history.Id);
        command.Parameters.AddWithValue("BatchId", history.BatchId);
        command.Parameters.AddWithValue("FromStageId", (object?)history.FromStageId ?? DBNull.Value);
        command.Parameters.AddWithValue("ToStageId", history.ToStageId);
        command.Parameters.AddWithValue("ChangedByUserId", history.ChangedByUserId);
        command.Parameters.AddWithValue("ChangedAt", history.ChangedAt);
        command.Parameters.AddWithValue("Notes", (object?)history.Notes ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogDebug("Created stage history {HistoryId} for batch {BatchId}", history.Id, history.BatchId);
        return history;
    }

    public async Task<IReadOnlyList<BatchStageHistory>> GetByBatchIdAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT h.id, h.batch_id, h.from_stage_id, h.to_stage_id, h.changed_by_user_id, h.changed_at, h.notes
            FROM genetics.batch_stage_history h
            INNER JOIN genetics.batches b ON h.batch_id = b.id
            WHERE h.batch_id = @BatchId AND b.site_id = @SiteId
            ORDER BY h.changed_at DESC";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("BatchId", batchId);
        command.Parameters.AddWithValue("SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var historyEntries = new List<BatchStageHistory>();
        while (await reader.ReadAsync(cancellationToken))
        {
            historyEntries.Add(MapToBatchStageHistory(reader));
        }

        return historyEntries;
    }

    public async Task<BatchStageHistory?> GetMostRecentAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT h.id, h.batch_id, h.from_stage_id, h.to_stage_id, h.changed_by_user_id, h.changed_at, h.notes
            FROM genetics.batch_stage_history h
            INNER JOIN genetics.batches b ON h.batch_id = b.id
            WHERE h.batch_id = @BatchId AND b.site_id = @SiteId
            ORDER BY h.changed_at DESC
            LIMIT 1";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("BatchId", batchId);
        command.Parameters.AddWithValue("SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapToBatchStageHistory(reader);
    }

    private static BatchStageHistory MapToBatchStageHistory(NpgsqlDataReader reader)
    {
        return BatchStageHistory.FromPersistence(
            id: reader.GetGuid(reader.GetOrdinal("id")),
            batchId: reader.GetGuid(reader.GetOrdinal("batch_id")),
            fromStageId: reader.IsDBNull(reader.GetOrdinal("from_stage_id")) ? null : reader.GetGuid(reader.GetOrdinal("from_stage_id")),
            toStageId: reader.GetGuid(reader.GetOrdinal("to_stage_id")),
            changedByUserId: reader.GetGuid(reader.GetOrdinal("changed_by_user_id")),
            changedAt: reader.GetDateTime(reader.GetOrdinal("changed_at")),
            notes: reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes"))
        );
    }
}
