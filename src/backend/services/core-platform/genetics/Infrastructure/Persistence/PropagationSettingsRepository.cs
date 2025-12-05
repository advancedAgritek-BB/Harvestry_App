using System.Text.Json;
using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Domain.Entities;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Harvestry.Genetics.Infrastructure.Persistence;

/// <summary>
/// Repository for propagation settings.
/// </summary>
public sealed class PropagationSettingsRepository : IPropagationSettingsRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly GeneticsDbContext _dbContext;
    private readonly ILogger<PropagationSettingsRepository> _logger;

    public PropagationSettingsRepository(GeneticsDbContext dbContext, ILogger<PropagationSettingsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<PropagationSettings?> GetBySiteAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, daily_limit, weekly_limit, mother_propagation_limit,
                   requires_override_approval, approver_role, approver_policy,
                   created_at, created_by_user_id, updated_at, updated_by_user_id
            FROM genetics.propagation_settings
            WHERE site_id = @SiteId";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var policyJson = reader.GetString(reader.GetOrdinal("approver_policy"));
        var policy = JsonSerializer.Deserialize<Dictionary<string, object>>(policyJson, SerializerOptions) ?? new Dictionary<string, object>();

        return PropagationSettings.FromPersistence(
            reader.GetGuid(reader.GetOrdinal("id")),
            reader.GetGuid(reader.GetOrdinal("site_id")),
            reader.IsDBNull(reader.GetOrdinal("daily_limit")) ? null : reader.GetInt32(reader.GetOrdinal("daily_limit")),
            reader.IsDBNull(reader.GetOrdinal("weekly_limit")) ? null : reader.GetInt32(reader.GetOrdinal("weekly_limit")),
            reader.IsDBNull(reader.GetOrdinal("mother_propagation_limit")) ? null : reader.GetInt32(reader.GetOrdinal("mother_propagation_limit")),
            reader.GetBoolean(reader.GetOrdinal("requires_override_approval")),
            reader.IsDBNull(reader.GetOrdinal("approver_role")) ? null : reader.GetString(reader.GetOrdinal("approver_role")),
            policy,
            reader.GetDateTime(reader.GetOrdinal("created_at")),
            reader.GetGuid(reader.GetOrdinal("created_by_user_id")),
            reader.GetDateTime(reader.GetOrdinal("updated_at")),
            reader.GetGuid(reader.GetOrdinal("updated_by_user_id"))
        );
    }

    public async Task<PropagationSettings> UpsertAsync(PropagationSettings settings, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO genetics.propagation_settings (
                id, site_id, daily_limit, weekly_limit, mother_propagation_limit,
                requires_override_approval, approver_role, approver_policy,
                created_at, created_by_user_id, updated_at, updated_by_user_id
            ) VALUES (
                @Id, @SiteId, @DailyLimit, @WeeklyLimit, @MotherLimit,
                @RequiresOverride, @ApproverRole, @ApproverPolicy::jsonb,
                @CreatedAt, @CreatedByUserId, @UpdatedAt, @UpdatedByUserId
            )
            ON CONFLICT (site_id) DO UPDATE SET
                daily_limit = @DailyLimit,
                weekly_limit = @WeeklyLimit,
                mother_propagation_limit = @MotherLimit,
                requires_override_approval = @RequiresOverride,
                approver_role = @ApproverRole,
                approver_policy = @ApproverPolicy::jsonb,
                updated_at = @UpdatedAt,
                updated_by_user_id = @UpdatedByUserId
            RETURNING id";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(connection, settings.SiteId, cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("Id", settings.Id);
        command.Parameters.AddWithValue("SiteId", settings.SiteId);
        command.Parameters.AddWithValue("DailyLimit", settings.DailyLimit.HasValue ? settings.DailyLimit.Value : DBNull.Value);
        command.Parameters.AddWithValue("WeeklyLimit", settings.WeeklyLimit.HasValue ? settings.WeeklyLimit.Value : DBNull.Value);
        command.Parameters.AddWithValue("MotherLimit", settings.MotherPropagationLimit.HasValue ? settings.MotherPropagationLimit.Value : DBNull.Value);
        command.Parameters.AddWithValue("RequiresOverride", settings.RequiresOverrideApproval);
        command.Parameters.AddWithValue("ApproverRole", settings.ApproverRole is null ? DBNull.Value : settings.ApproverRole);
        command.Parameters.AddWithValue("ApproverPolicy", JsonSerializer.Serialize(settings.ApproverPolicy, SerializerOptions));
        command.Parameters.AddWithValue("CreatedAt", settings.CreatedAt);
        command.Parameters.AddWithValue("CreatedByUserId", settings.CreatedByUserId);
        command.Parameters.AddWithValue("UpdatedAt", settings.UpdatedAt);
        command.Parameters.AddWithValue("UpdatedByUserId", settings.UpdatedByUserId);

        await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Upserted propagation settings for site {SiteId}", settings.SiteId);
        return settings;
    }
}
