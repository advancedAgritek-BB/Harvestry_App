using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Application.Interfaces;
using Harvestry.Identity.Domain.Entities;
using Harvestry.Identity.Domain.Enums;
using Harvestry.Identity.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Harvestry.Identity.Infrastructure.Persistence;

public sealed class BadgeRepository : IBadgeRepository
{
    private readonly IdentityDbContext _dbContext;
    private readonly IRlsContextAccessor _rlsContextAccessor;
    private readonly ILogger<BadgeRepository> _logger;

    public BadgeRepository(
        IdentityDbContext dbContext,
        IRlsContextAccessor rlsContextAccessor,
        ILogger<BadgeRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Badge?> GetByIdAsync(Guid badgeId, CancellationToken cancellationToken = default)
    {
        await using var connection = await PrepareConnectionAsync(null, cancellationToken).ConfigureAwait(false);

        const string sql = @"
            SELECT
                badge_id,
                user_id,
                site_id,
                badge_code,
                badge_type,
                status,
                issued_at,
                expires_at,
                last_used_at,
                revoked_at,
                revoked_by,
                revoke_reason,
                metadata,
                created_at,
                updated_at
            FROM badges
            WHERE badge_id = @badge_id
            LIMIT 1;
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("badge_id", NpgsqlDbType.Uuid).Value = badgeId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return MapBadge(reader);
    }

    public async Task<Badge?> GetByCodeAsync(BadgeCode badgeCode, CancellationToken cancellationToken = default)
    {
        if (badgeCode == null) throw new ArgumentNullException(nameof(badgeCode));

        var connection = await PrepareConnectionAsync(null, cancellationToken).ConfigureAwait(false);

        const string sql = @"
            SELECT
                badge_id,
                user_id,
                site_id,
                badge_code,
                badge_type,
                status,
                issued_at,
                expires_at,
                last_used_at,
                revoked_at,
                revoked_by,
                revoke_reason,
                metadata,
                created_at,
                updated_at
            FROM badges
            WHERE badge_code = @badge_code
            LIMIT 1;
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("badge_code", NpgsqlDbType.Varchar).Value = (string)badgeCode;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return MapBadge(reader);
    }

    public async Task<IEnumerable<Badge>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(null, cancellationToken).ConfigureAwait(false);

        const string sql = @"
            SELECT
                badge_id,
                user_id,
                site_id,
                badge_code,
                badge_type,
                status,
                issued_at,
                expires_at,
                last_used_at,
                revoked_at,
                revoked_by,
                revoke_reason,
                metadata,
                created_at,
                updated_at
            FROM badges
            WHERE user_id = @user_id
            ORDER BY issued_at DESC;
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("user_id", NpgsqlDbType.Uuid).Value = userId;

        var results = new List<Badge>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(MapBadge(reader));
        }

        return results;
    }

    public async Task<IEnumerable<Badge>> GetActiveBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(siteId, cancellationToken).ConfigureAwait(false);

        const string sql = @"
            SELECT
                badge_id,
                user_id,
                site_id,
                badge_code,
                badge_type,
                status,
                issued_at,
                expires_at,
                last_used_at,
                revoked_at,
                revoked_by,
                revoke_reason,
                metadata,
                created_at,
                updated_at
            FROM badges
            WHERE site_id = @site_id AND status = 'active';
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = siteId;

        var results = new List<Badge>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(MapBadge(reader));
        }

        return results;
    }

    public async Task<Badge> AddAsync(Badge badge, CancellationToken cancellationToken = default)
    {
        if (badge == null) throw new ArgumentNullException(nameof(badge));

        var connection = await PrepareConnectionAsync(badge.SiteId, cancellationToken).ConfigureAwait(false);

        const string sql = @"
            INSERT INTO badges (
                badge_id,
                user_id,
                site_id,
                badge_code,
                badge_type,
                status,
                issued_at,
                expires_at,
                last_used_at,
                revoked_at,
                revoked_by,
                revoke_reason,
                metadata,
                created_at,
                updated_at)
            VALUES (
                @badge_id,
                @user_id,
                @site_id,
                @badge_code,
                @badge_type,
                @status,
                @issued_at,
                @expires_at,
                @last_used_at,
                @revoked_at,
                @revoked_by,
                @revoke_reason,
                @metadata,
                @created_at,
                @updated_at);
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        PopulateParameters(command, badge);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return badge;
    }

    public async Task UpdateAsync(Badge badge, CancellationToken cancellationToken = default)
    {
        if (badge == null) throw new ArgumentNullException(nameof(badge));

        var connection = await PrepareConnectionAsync(badge.SiteId, cancellationToken).ConfigureAwait(false);

        const string sql = @"
            UPDATE badges SET
                user_id = @user_id,
                site_id = @site_id,
                badge_code = @badge_code,
                badge_type = @badge_type,
                status = @status,
                issued_at = @issued_at,
                expires_at = @expires_at,
                last_used_at = @last_used_at,
                revoked_at = @revoked_at,
                revoked_by = @revoked_by,
                revoke_reason = @revoke_reason,
                metadata = @metadata,
                updated_at = @updated_at
            WHERE badge_id = @badge_id;
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        PopulateParameters(command, badge);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        if (affected == 0)
        {
            throw new InvalidOperationException($"Badge {badge.Id} update failed: row not found or blocked by RLS");
        }
    }

    private async Task<NpgsqlConnection> PrepareConnectionAsync(Guid? siteId, CancellationToken cancellationToken)
    {
        var context = _rlsContextAccessor.Current;
        var role = string.IsNullOrWhiteSpace(context.Role) ? "service_account" : context.Role.Trim();
        var isServiceAccount = string.Equals(role, "service_account", StringComparison.OrdinalIgnoreCase);

        var userId = context.UserId;
        if (!isServiceAccount && userId == Guid.Empty)
        {
            throw new InvalidOperationException("RLS context UserId is required for non-service accounts.");
        }

        var effectiveSite = siteId ?? context.SiteId;
        if (!isServiceAccount && effectiveSite is null)
        {
            throw new InvalidOperationException("RLS context SiteId is required for non-service accounts.");
        }

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(isServiceAccount ? Guid.Empty : userId, role, effectiveSite, cancellationToken).ConfigureAwait(false);
        return connection;
    }

    private static void PopulateParameters(NpgsqlCommand command, Badge badge)
    {
        command.Parameters.Add("badge_id", NpgsqlDbType.Uuid).Value = badge.Id;
        command.Parameters.Add("user_id", NpgsqlDbType.Uuid).Value = badge.UserId;
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = badge.SiteId;
        command.Parameters.Add("badge_code", NpgsqlDbType.Varchar).Value = (string)badge.BadgeCode;
        command.Parameters.Add("badge_type", NpgsqlDbType.Varchar).Value = badge.BadgeType.ToString().ToLowerInvariant();
        command.Parameters.Add("status", NpgsqlDbType.Varchar).Value = badge.Status.ToString().ToLowerInvariant();
        command.Parameters.Add("issued_at", NpgsqlDbType.TimestampTz).Value = badge.IssuedAt;
        command.Parameters.Add("expires_at", NpgsqlDbType.TimestampTz).Value = (object?)badge.ExpiresAt ?? DBNull.Value;
        command.Parameters.Add("last_used_at", NpgsqlDbType.TimestampTz).Value = (object?)badge.LastUsedAt ?? DBNull.Value;
        command.Parameters.Add("revoked_at", NpgsqlDbType.TimestampTz).Value = (object?)badge.RevokedAt ?? DBNull.Value;
        command.Parameters.Add("revoked_by", NpgsqlDbType.Uuid).Value = (object?)badge.RevokedBy ?? DBNull.Value;
        command.Parameters.Add("revoke_reason", NpgsqlDbType.Text).Value = (object?)badge.RevokeReason ?? DBNull.Value;
        var metadataPayload = badge.Metadata?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            ?? new Dictionary<string, object>();
        command.Parameters.Add("metadata", NpgsqlDbType.Jsonb).Value = JsonUtilities.SerializeDictionary(metadataPayload);
        command.Parameters.Add("created_at", NpgsqlDbType.TimestampTz).Value = badge.CreatedAt;
        command.Parameters.Add("updated_at", NpgsqlDbType.TimestampTz).Value = badge.UpdatedAt;
    }

    private static Badge MapBadge(NpgsqlDataReader reader)
    {
        var metadataValue = reader["metadata"];

        var badgeCode = BadgeCode.Create(reader.GetString(reader.GetOrdinal("badge_code")));
        var badgeTypeValue = reader.GetString(reader.GetOrdinal("badge_type"));
        if (!Enum.TryParse(badgeTypeValue, true, out BadgeType badgeType))
        {
            badgeType = BadgeType.Physical;
        }

        var statusValue = reader.GetString(reader.GetOrdinal("status"));
        if (!Enum.TryParse(statusValue, true, out BadgeStatus status))
        {
            status = BadgeStatus.Active;
        }

        return Badge.Restore(
            reader.GetGuid(reader.GetOrdinal("badge_id")),
            reader.GetGuid(reader.GetOrdinal("user_id")),
            reader.GetGuid(reader.GetOrdinal("site_id")),
            badgeCode,
            badgeType,
            status,
            reader.GetDateTime(reader.GetOrdinal("issued_at")),
            reader.IsDBNull(reader.GetOrdinal("expires_at")) ? null : reader.GetDateTime(reader.GetOrdinal("expires_at")),
            reader.IsDBNull(reader.GetOrdinal("last_used_at")) ? null : reader.GetDateTime(reader.GetOrdinal("last_used_at")),
            reader.IsDBNull(reader.GetOrdinal("revoked_at")) ? null : reader.GetDateTime(reader.GetOrdinal("revoked_at")),
            reader.IsDBNull(reader.GetOrdinal("revoked_by")) ? null : reader.GetGuid(reader.GetOrdinal("revoked_by")),
            reader.IsDBNull(reader.GetOrdinal("revoke_reason")) ? null : reader.GetString(reader.GetOrdinal("revoke_reason")),
            JsonUtilities.ToDictionary(metadataValue),
            reader.GetDateTime(reader.GetOrdinal("created_at")),
            reader.GetDateTime(reader.GetOrdinal("updated_at")));
    }
}
