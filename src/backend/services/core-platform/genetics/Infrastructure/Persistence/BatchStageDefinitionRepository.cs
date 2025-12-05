using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Domain.Entities;
using Harvestry.Genetics.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Harvestry.Genetics.Infrastructure.Persistence;

/// <summary>
/// Repository for BatchStageDefinition operations
/// </summary>
public class BatchStageDefinitionRepository : IBatchStageDefinitionRepository
{
    private readonly GeneticsDbContext _dbContext;
    private readonly ILogger<BatchStageDefinitionRepository> _logger;

    public BatchStageDefinitionRepository(GeneticsDbContext dbContext, ILogger<BatchStageDefinitionRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<BatchStageDefinition> CreateAsync(BatchStageDefinition stage, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO genetics.batch_stage_definitions (
                id, site_id, stage_key, display_name, description, sequence_order,
                is_terminal, requires_harvest_metrics,
                created_at, created_by_user_id, updated_at, updated_by_user_id
            ) VALUES (
                @Id, @SiteId, @StageKey, @DisplayName, @Description, @SequenceOrder,
                @IsTerminal, @RequiresHarvestMetrics,
                @CreatedAt, @CreatedByUserId, @UpdatedAt, @UpdatedByUserId
            )";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, stage.SiteId, cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Id", stage.Id);
        command.Parameters.AddWithValue("SiteId", stage.SiteId);
        command.Parameters.AddWithValue("StageKey", stage.StageKey.Value);
        command.Parameters.AddWithValue("DisplayName", stage.DisplayName);
        command.Parameters.AddWithValue("Description", (object?)stage.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("SequenceOrder", stage.SequenceOrder);
        command.Parameters.AddWithValue("IsTerminal", stage.IsTerminal);
        command.Parameters.AddWithValue("RequiresHarvestMetrics", stage.RequiresHarvestMetrics);
        command.Parameters.AddWithValue("CreatedAt", stage.CreatedAt);
        command.Parameters.AddWithValue("CreatedByUserId", stage.CreatedByUserId);
        command.Parameters.AddWithValue("UpdatedAt", stage.UpdatedAt);
        command.Parameters.AddWithValue("UpdatedByUserId", stage.UpdatedByUserId);

        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogDebug("Created batch stage {StageId} with key {StageKey}", stage.Id, stage.StageKey.Value);
        return stage;
    }

    public async Task<BatchStageDefinition?> GetByIdAsync(Guid stageId, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, stage_key, display_name, description, sequence_order,
                   is_terminal, requires_harvest_metrics,
                   created_at, created_by_user_id, updated_at, updated_by_user_id
            FROM genetics.batch_stage_definitions
            WHERE id = @StageId AND site_id = @SiteId";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("StageId", stageId);
        command.Parameters.AddWithValue("SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapToBatchStageDefinition(reader);
    }

    public async Task<IReadOnlyList<BatchStageDefinition>> GetAllAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, stage_key, display_name, description, sequence_order,
                   is_terminal, requires_harvest_metrics,
                   created_at, created_by_user_id, updated_at, updated_by_user_id
            FROM genetics.batch_stage_definitions
            WHERE site_id = @SiteId
            ORDER BY sequence_order";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var stages = new List<BatchStageDefinition>();
        while (await reader.ReadAsync(cancellationToken))
        {
            stages.Add(MapToBatchStageDefinition(reader));
        }

        return stages;
    }

    public async Task<IReadOnlyList<BatchStageDefinition>> GetActiveAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        // Note: The entity doesn't have an IsActive flag, so we return all stages
        // If IsActive is added later, update this query to filter by it
        return await GetAllAsync(siteId, cancellationToken);
    }

    public async Task<BatchStageDefinition> UpdateAsync(BatchStageDefinition stage, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE genetics.batch_stage_definitions SET
                display_name = @DisplayName,
                description = @Description,
                sequence_order = @SequenceOrder,
                is_terminal = @IsTerminal,
                requires_harvest_metrics = @RequiresHarvestMetrics,
                updated_at = @UpdatedAt,
                updated_by_user_id = @UpdatedByUserId
            WHERE id = @Id AND site_id = @SiteId";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, stage.SiteId, cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Id", stage.Id);
        command.Parameters.AddWithValue("SiteId", stage.SiteId);
        command.Parameters.AddWithValue("DisplayName", stage.DisplayName);
        command.Parameters.AddWithValue("Description", (object?)stage.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("SequenceOrder", stage.SequenceOrder);
        command.Parameters.AddWithValue("IsTerminal", stage.IsTerminal);
        command.Parameters.AddWithValue("RequiresHarvestMetrics", stage.RequiresHarvestMetrics);
        command.Parameters.AddWithValue("UpdatedAt", stage.UpdatedAt);
        command.Parameters.AddWithValue("UpdatedByUserId", stage.UpdatedByUserId);

        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogDebug("Updated batch stage {StageId}", stage.Id);
        return stage;
    }

    public async Task DeleteAsync(Guid stageId, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM genetics.batch_stage_definitions WHERE id = @StageId AND site_id = @SiteId";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("StageId", stageId);
        command.Parameters.AddWithValue("SiteId", siteId);

        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogDebug("Deleted batch stage {StageId}", stageId);
    }

    public async Task<bool> ExistsAsync(Guid stageId, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT COUNT(1) FROM genetics.batch_stage_definitions WHERE id = @StageId AND site_id = @SiteId";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("StageId", stageId);
        command.Parameters.AddWithValue("SiteId", siteId);

        var count = (long)(await command.ExecuteScalarAsync(cancellationToken) ?? 0L);
        return count > 0;
    }

    private static BatchStageDefinition MapToBatchStageDefinition(NpgsqlDataReader reader)
    {
        return BatchStageDefinition.FromPersistence(
            id: reader.GetGuid(reader.GetOrdinal("id")),
            siteId: reader.GetGuid(reader.GetOrdinal("site_id")),
            stageKey: StageKey.Create(reader.GetString(reader.GetOrdinal("stage_key"))),
            displayName: reader.GetString(reader.GetOrdinal("display_name")),
            description: reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
            sequenceOrder: reader.GetInt32(reader.GetOrdinal("sequence_order")),
            isTerminal: reader.GetBoolean(reader.GetOrdinal("is_terminal")),
            requiresHarvestMetrics: reader.GetBoolean(reader.GetOrdinal("requires_harvest_metrics")),
            createdAt: reader.GetDateTime(reader.GetOrdinal("created_at")),
            createdByUserId: reader.GetGuid(reader.GetOrdinal("created_by_user_id")),
            updatedAt: reader.GetDateTime(reader.GetOrdinal("updated_at")),
            updatedByUserId: reader.GetGuid(reader.GetOrdinal("updated_by_user_id"))
        );
    }
}
