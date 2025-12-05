using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Domain.Entities;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Text.Json;

namespace Harvestry.Genetics.Infrastructure.Persistence;

/// <summary>
/// Repository for BatchCodeRule operations
/// </summary>
public class BatchCodeRuleRepository : IBatchCodeRuleRepository
{
    private readonly GeneticsDbContext _dbContext;
    private readonly ILogger<BatchCodeRuleRepository> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public BatchCodeRuleRepository(GeneticsDbContext dbContext, ILogger<BatchCodeRuleRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<BatchCodeRule> CreateAsync(BatchCodeRule rule, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO genetics.batch_code_rules (
                id, site_id, name, rule_definition, reset_policy, is_active,
                created_at, created_by_user_id, updated_at, updated_by_user_id
            ) VALUES (
                @Id, @SiteId, @Name, @RuleDefinition::jsonb, @ResetPolicy, @IsActive,
                @CreatedAt, @CreatedByUserId, @UpdatedAt, @UpdatedByUserId
            )";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, rule.SiteId, cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Id", rule.Id);
        command.Parameters.AddWithValue("SiteId", rule.SiteId);
        command.Parameters.AddWithValue("Name", rule.Name);
        command.Parameters.AddWithValue("RuleDefinition", JsonSerializer.Serialize(rule.RuleDefinition, JsonOptions));
        command.Parameters.AddWithValue("ResetPolicy", rule.ResetPolicy);
        command.Parameters.AddWithValue("IsActive", rule.IsActive);
        command.Parameters.AddWithValue("CreatedAt", rule.CreatedAt);
        command.Parameters.AddWithValue("CreatedByUserId", rule.CreatedByUserId);
        command.Parameters.AddWithValue("UpdatedAt", rule.UpdatedAt);
        command.Parameters.AddWithValue("UpdatedByUserId", rule.UpdatedByUserId);

        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogDebug("Created batch code rule {RuleId} with name {RuleName}", rule.Id, rule.Name);
        return rule;
    }

    public async Task<BatchCodeRule?> GetByIdAsync(Guid ruleId, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, name, rule_definition, reset_policy, is_active,
                   created_at, created_by_user_id, updated_at, updated_by_user_id
            FROM genetics.batch_code_rules
            WHERE id = @RuleId AND site_id = @SiteId";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("RuleId", ruleId);
        command.Parameters.AddWithValue("SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapToBatchCodeRule(reader);
    }

    public async Task<IReadOnlyList<BatchCodeRule>> GetAllAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, name, rule_definition, reset_policy, is_active,
                   created_at, created_by_user_id, updated_at, updated_by_user_id
            FROM genetics.batch_code_rules
            WHERE site_id = @SiteId
            ORDER BY name";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var rules = new List<BatchCodeRule>();
        while (await reader.ReadAsync(cancellationToken))
        {
            rules.Add(MapToBatchCodeRule(reader));
        }

        return rules;
    }

    public async Task<IReadOnlyList<BatchCodeRule>> GetActiveAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, name, rule_definition, reset_policy, is_active,
                   created_at, created_by_user_id, updated_at, updated_by_user_id
            FROM genetics.batch_code_rules
            WHERE site_id = @SiteId AND is_active = true
            ORDER BY name";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var rules = new List<BatchCodeRule>();
        while (await reader.ReadAsync(cancellationToken))
        {
            rules.Add(MapToBatchCodeRule(reader));
        }

        return rules;
    }

    public async Task<IReadOnlyList<BatchCodeRule>> GetOrderedByPriorityAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        // Note: Priority field doesn't exist yet on BatchCodeRule entity
        // For now, return active rules ordered by creation date (oldest = highest priority)
        const string sql = @"
            SELECT id, site_id, name, rule_definition, reset_policy, is_active,
                   created_at, created_by_user_id, updated_at, updated_by_user_id
            FROM genetics.batch_code_rules
            WHERE site_id = @SiteId AND is_active = true
            ORDER BY created_at ASC";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var rules = new List<BatchCodeRule>();
        while (await reader.ReadAsync(cancellationToken))
        {
            rules.Add(MapToBatchCodeRule(reader));
        }

        return rules;
    }

    public async Task<BatchCodeRule> UpdateAsync(BatchCodeRule rule, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE genetics.batch_code_rules SET
                name = @Name,
                rule_definition = @RuleDefinition::jsonb,
                reset_policy = @ResetPolicy,
                is_active = @IsActive,
                updated_at = @UpdatedAt,
                updated_by_user_id = @UpdatedByUserId
            WHERE id = @Id AND site_id = @SiteId";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, rule.SiteId, cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Id", rule.Id);
        command.Parameters.AddWithValue("SiteId", rule.SiteId);
        command.Parameters.AddWithValue("Name", rule.Name);
        command.Parameters.AddWithValue("RuleDefinition", JsonSerializer.Serialize(rule.RuleDefinition, JsonOptions));
        command.Parameters.AddWithValue("ResetPolicy", rule.ResetPolicy);
        command.Parameters.AddWithValue("IsActive", rule.IsActive);
        command.Parameters.AddWithValue("UpdatedAt", rule.UpdatedAt);
        command.Parameters.AddWithValue("UpdatedByUserId", rule.UpdatedByUserId);

        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogDebug("Updated batch code rule {RuleId}", rule.Id);
        return rule;
    }

    public async Task DeleteAsync(Guid ruleId, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM genetics.batch_code_rules WHERE id = @RuleId AND site_id = @SiteId";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("RuleId", ruleId);
        command.Parameters.AddWithValue("SiteId", siteId);

        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogDebug("Deleted batch code rule {RuleId}", ruleId);
    }

    public async Task<bool> RuleNameExistsAsync(string ruleName, Guid siteId, Guid? excludeRuleId = null, CancellationToken cancellationToken = default)
    {
        var sql = "SELECT COUNT(1) FROM genetics.batch_code_rules WHERE name = @RuleName AND site_id = @SiteId";
        if (excludeRuleId.HasValue)
        {
            sql += " AND id != @ExcludeRuleId";
        }

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("RuleName", ruleName);
        command.Parameters.AddWithValue("SiteId", siteId);

        if (excludeRuleId.HasValue)
        {
            command.Parameters.AddWithValue("ExcludeRuleId", excludeRuleId.Value);
        }

        var count = (long)(await command.ExecuteScalarAsync(cancellationToken) ?? 0L);
        return count > 0;
    }

    private static BatchCodeRule MapToBatchCodeRule(NpgsqlDataReader reader)
    {
        var ruleDefinitionJson = reader.GetString(reader.GetOrdinal("rule_definition"));
        var ruleDefinition = JsonSerializer.Deserialize<Dictionary<string, object>>(ruleDefinitionJson, JsonOptions)
                             ?? new Dictionary<string, object>();

        return BatchCodeRule.FromPersistence(
            id: reader.GetGuid(reader.GetOrdinal("id")),
            siteId: reader.GetGuid(reader.GetOrdinal("site_id")),
            name: reader.GetString(reader.GetOrdinal("name")),
            ruleDefinition: ruleDefinition,
            resetPolicy: reader.GetString(reader.GetOrdinal("reset_policy")),
            isActive: reader.GetBoolean(reader.GetOrdinal("is_active")),
            createdAt: reader.GetDateTime(reader.GetOrdinal("created_at")),
            createdByUserId: reader.GetGuid(reader.GetOrdinal("created_by_user_id")),
            updatedAt: reader.GetDateTime(reader.GetOrdinal("updated_at")),
            updatedByUserId: reader.GetGuid(reader.GetOrdinal("updated_by_user_id"))
        );
    }
}
