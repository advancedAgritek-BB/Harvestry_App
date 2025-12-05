using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Domain.Entities;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Harvestry.Genetics.Infrastructure.Persistence;

/// <summary>
/// Repository for BatchStageTransition operations
/// </summary>
public class BatchStageTransitionRepository : IBatchStageTransitionRepository
{
    private readonly GeneticsDbContext _dbContext;
    private readonly ILogger<BatchStageTransitionRepository> _logger;

    public BatchStageTransitionRepository(GeneticsDbContext dbContext, ILogger<BatchStageTransitionRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<BatchStageTransition> CreateAsync(BatchStageTransition transition, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO genetics.batch_stage_transitions (
                id, site_id, from_stage_id, to_stage_id, auto_advance, requires_approval, approval_role,
                created_at, created_by_user_id, updated_at, updated_by_user_id
            ) VALUES (
                @Id, @SiteId, @FromStageId, @ToStageId, @AutoAdvance, @RequiresApproval, @ApprovalRole,
                @CreatedAt, @CreatedByUserId, @UpdatedAt, @UpdatedByUserId
            )";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, transition.SiteId, cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Id", transition.Id);
        command.Parameters.AddWithValue("SiteId", transition.SiteId);
        command.Parameters.AddWithValue("FromStageId", transition.FromStageId);
        command.Parameters.AddWithValue("ToStageId", transition.ToStageId);
        command.Parameters.AddWithValue("AutoAdvance", transition.AutoAdvance);
        command.Parameters.AddWithValue("RequiresApproval", transition.RequiresApproval);
        command.Parameters.AddWithValue("ApprovalRole", (object?)transition.ApprovalRole ?? DBNull.Value);
        command.Parameters.AddWithValue("CreatedAt", transition.CreatedAt);
        command.Parameters.AddWithValue("CreatedByUserId", transition.CreatedByUserId);
        command.Parameters.AddWithValue("UpdatedAt", transition.UpdatedAt);
        command.Parameters.AddWithValue("UpdatedByUserId", transition.UpdatedByUserId);

        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogDebug("Created stage transition {TransitionId}", transition.Id);
        return transition;
    }

    public async Task<BatchStageTransition?> GetByIdAsync(Guid transitionId, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, from_stage_id, to_stage_id, auto_advance, requires_approval, approval_role,
                   created_at, created_by_user_id, updated_at, updated_by_user_id
            FROM genetics.batch_stage_transitions
            WHERE id = @TransitionId AND site_id = @SiteId";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("TransitionId", transitionId);
        command.Parameters.AddWithValue("SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapToBatchStageTransition(reader);
    }

    public async Task<IReadOnlyList<BatchStageTransition>> GetAllAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, from_stage_id, to_stage_id, auto_advance, requires_approval, approval_role,
                   created_at, created_by_user_id, updated_at, updated_by_user_id
            FROM genetics.batch_stage_transitions
            WHERE site_id = @SiteId
            ORDER BY from_stage_id, to_stage_id";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var transitions = new List<BatchStageTransition>();
        while (await reader.ReadAsync(cancellationToken))
        {
            transitions.Add(MapToBatchStageTransition(reader));
        }

        return transitions;
    }

    public async Task<IReadOnlyList<BatchStageTransition>> GetFromStageAsync(Guid fromStageId, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, from_stage_id, to_stage_id, auto_advance, requires_approval, approval_role,
                   created_at, created_by_user_id, updated_at, updated_by_user_id
            FROM genetics.batch_stage_transitions
            WHERE from_stage_id = @FromStageId AND site_id = @SiteId
            ORDER BY to_stage_id";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("FromStageId", fromStageId);
        command.Parameters.AddWithValue("SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var transitions = new List<BatchStageTransition>();
        while (await reader.ReadAsync(cancellationToken))
        {
            transitions.Add(MapToBatchStageTransition(reader));
        }

        return transitions;
    }

    public async Task<IReadOnlyList<BatchStageTransition>> GetToStageAsync(Guid toStageId, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, from_stage_id, to_stage_id, auto_advance, requires_approval, approval_role,
                   created_at, created_by_user_id, updated_at, updated_by_user_id
            FROM genetics.batch_stage_transitions
            WHERE to_stage_id = @ToStageId AND site_id = @SiteId
            ORDER BY from_stage_id";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("ToStageId", toStageId);
        command.Parameters.AddWithValue("SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var transitions = new List<BatchStageTransition>();
        while (await reader.ReadAsync(cancellationToken))
        {
            transitions.Add(MapToBatchStageTransition(reader));
        }

        return transitions;
    }

    public async Task<BatchStageTransition?> GetTransitionAsync(Guid fromStageId, Guid toStageId, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, from_stage_id, to_stage_id, auto_advance, requires_approval, approval_role,
                   created_at, created_by_user_id, updated_at, updated_by_user_id
            FROM genetics.batch_stage_transitions
            WHERE from_stage_id = @FromStageId AND to_stage_id = @ToStageId AND site_id = @SiteId";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("FromStageId", fromStageId);
        command.Parameters.AddWithValue("ToStageId", toStageId);
        command.Parameters.AddWithValue("SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapToBatchStageTransition(reader);
    }

    public async Task<BatchStageTransition> UpdateAsync(BatchStageTransition transition, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE genetics.batch_stage_transitions SET
                auto_advance = @AutoAdvance,
                requires_approval = @RequiresApproval,
                approval_role = @ApprovalRole,
                updated_at = @UpdatedAt,
                updated_by_user_id = @UpdatedByUserId
            WHERE id = @Id AND site_id = @SiteId";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, transition.SiteId, cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Id", transition.Id);
        command.Parameters.AddWithValue("SiteId", transition.SiteId);
        command.Parameters.AddWithValue("AutoAdvance", transition.AutoAdvance);
        command.Parameters.AddWithValue("RequiresApproval", transition.RequiresApproval);
        command.Parameters.AddWithValue("ApprovalRole", (object?)transition.ApprovalRole ?? DBNull.Value);
        command.Parameters.AddWithValue("UpdatedAt", transition.UpdatedAt);
        command.Parameters.AddWithValue("UpdatedByUserId", transition.UpdatedByUserId);

        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogDebug("Updated stage transition {TransitionId}", transition.Id);
        return transition;
    }

    public async Task DeleteAsync(Guid transitionId, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM genetics.batch_stage_transitions WHERE id = @TransitionId AND site_id = @SiteId";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("TransitionId", transitionId);
        command.Parameters.AddWithValue("SiteId", siteId);

        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogDebug("Deleted stage transition {TransitionId}", transitionId);
    }

    public async Task<bool> TransitionExistsAsync(Guid fromStageId, Guid toStageId, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT COUNT(1) FROM genetics.batch_stage_transitions
            WHERE from_stage_id = @FromStageId AND to_stage_id = @ToStageId AND site_id = @SiteId";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("FromStageId", fromStageId);
        command.Parameters.AddWithValue("ToStageId", toStageId);
        command.Parameters.AddWithValue("SiteId", siteId);

        var count = (long)(await command.ExecuteScalarAsync(cancellationToken) ?? 0L);
        return count > 0;
    }

    private static BatchStageTransition MapToBatchStageTransition(NpgsqlDataReader reader)
    {
        return BatchStageTransition.FromPersistence(
            id: reader.GetGuid(reader.GetOrdinal("id")),
            siteId: reader.GetGuid(reader.GetOrdinal("site_id")),
            fromStageId: reader.GetGuid(reader.GetOrdinal("from_stage_id")),
            toStageId: reader.GetGuid(reader.GetOrdinal("to_stage_id")),
            autoAdvance: reader.GetBoolean(reader.GetOrdinal("auto_advance")),
            requiresApproval: reader.GetBoolean(reader.GetOrdinal("requires_approval")),
            approvalRole: reader.IsDBNull(reader.GetOrdinal("approval_role")) ? null : reader.GetString(reader.GetOrdinal("approval_role")),
            createdAt: reader.GetDateTime(reader.GetOrdinal("created_at")),
            createdByUserId: reader.GetGuid(reader.GetOrdinal("created_by_user_id")),
            updatedAt: reader.GetDateTime(reader.GetOrdinal("updated_at")),
            updatedByUserId: reader.GetGuid(reader.GetOrdinal("updated_by_user_id"))
        );
    }
}
