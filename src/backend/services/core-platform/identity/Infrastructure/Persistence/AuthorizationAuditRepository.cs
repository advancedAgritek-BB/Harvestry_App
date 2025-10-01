using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Application.Interfaces;
using Harvestry.Identity.Domain.Entities;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Harvestry.Identity.Infrastructure.Persistence;

public sealed class AuthorizationAuditRepository : IAuthorizationAuditRepository
{
    private readonly IdentityDbContext _dbContext;
    private readonly ILogger<AuthorizationAuditRepository> _logger;

    public AuthorizationAuditRepository(IdentityDbContext dbContext, ILogger<AuthorizationAuditRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task LogAsync(AuthorizationAuditEntry entry, CancellationToken cancellationToken = default)
    {
        if (entry is null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        var connection = await PrepareConnectionAsync(entry.SiteId, cancellationToken).ConfigureAwait(false);

        const string sql = @"
            INSERT INTO authorization_audit (
                user_id,
                site_id,
                action,
                resource_type,
                resource_id,
                granted,
                deny_reason,
                context,
                ip_address,
                user_agent,
                occurred_at)
            VALUES (
                @user_id,
                @site_id,
                @action,
                @resource_type,
                @resource_id,
                @granted,
                @deny_reason,
                @context,
                @ip_address,
                @user_agent,
                COALESCE(@occurred_at, NOW()));
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("user_id", NpgsqlDbType.Uuid).Value = entry.UserId;
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = entry.SiteId;
        command.Parameters.Add("action", NpgsqlDbType.Varchar).Value = entry.Action;
        command.Parameters.Add("resource_type", NpgsqlDbType.Varchar).Value = entry.ResourceType;
        command.Parameters.Add("resource_id", NpgsqlDbType.Uuid).Value = (object?)entry.ResourceId ?? DBNull.Value;
        command.Parameters.Add("granted", NpgsqlDbType.Boolean).Value = entry.Granted;
        command.Parameters.Add("deny_reason", NpgsqlDbType.Text).Value = (object?)entry.DenyReason ?? DBNull.Value;

        var contextPayload = SerializeContext(entry.Context);
        command.Parameters.Add("context", NpgsqlDbType.Jsonb).Value = contextPayload;

        command.Parameters.Add("ip_address", NpgsqlDbType.Inet).Value =
            !string.IsNullOrWhiteSpace(entry.IpAddress) ? entry.IpAddress! : DBNull.Value;
        command.Parameters.Add("user_agent", NpgsqlDbType.Text).Value =
            !string.IsNullOrWhiteSpace(entry.UserAgent) ? entry.UserAgent! : DBNull.Value;
        command.Parameters.Add("occurred_at", NpgsqlDbType.TimestampTz).Value =
            entry.OccurredAt.HasValue ? entry.OccurredAt.Value : DBNull.Value;

        var rows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        if (rows == 0)
        {
            _logger.LogWarning("Authorization audit insert returned zero rows for action {Action}", entry.Action);
        }
    }

    private async Task<NpgsqlConnection> PrepareConnectionAsync(Guid siteId, CancellationToken cancellationToken)
    {
        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(Guid.Empty, "service_account", siteId, cancellationToken).ConfigureAwait(false);
        return connection;
    }

    /// <summary>
    /// Serializes context dictionary to canonical JSON for audit logging.
    /// Uses canonical serialization to ensure deterministic hashing.
    /// </summary>
    private static string SerializeContext(IReadOnlyDictionary<string, object?>? context)
    {
        if (context is null || context.Count == 0)
        {
            return "{}";
        }

        var filtered = context
            .Where(kvp => kvp.Value is not null)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value!);

        if (filtered.Count == 0)
        {
            return "{}";
        }

        // Use canonical serialization for deterministic audit hashing
        return JsonUtilities.SerializeDictionaryCanonical(filtered);
    }
}
