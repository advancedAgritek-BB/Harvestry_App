using System;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Harvestry.Tasks.Infrastructure.Authorization;

/// <summary>
/// Provides authorization checks for site access by querying the identity database.
/// </summary>
public sealed class SiteAuthorizationService : ISiteAuthorizationService
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<SiteAuthorizationService> _logger;

    public SiteAuthorizationService(NpgsqlDataSource dataSource, ILogger<SiteAuthorizationService> logger)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> HasSiteAccessAsync(Guid userId, Guid siteId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            return false;
        }

        if (siteId == Guid.Empty)
        {
            return false;
        }

        const string sql = @"
            SELECT 1
            FROM user_sites
            WHERE user_id = @userId
              AND site_id = @siteId
              AND revoked_at IS NULL
            LIMIT 1;
        ";

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        var userParameter = command.Parameters.Add("userId", NpgsqlDbType.Uuid);
        userParameter.Value = userId;

        var siteParameter = command.Parameters.Add("siteId", NpgsqlDbType.Uuid);
        siteParameter.Value = siteId;

        try
        {
            var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            var hasAccess = result != null;

            _logger.LogInformation("Site access check for user {UserId} to site {SiteId}: {HasAccess}",
                userId, siteId, hasAccess);

            return hasAccess;
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "Failed to check site access for user {UserId} to site {SiteId}.", userId, siteId);
            throw;
        }
    }

    public async Task<bool> IsSiteAdminAsync(Guid userId, Guid siteId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            return false;
        }

        if (siteId == Guid.Empty)
        {
            return false;
        }

        const string sql = @"
            SELECT 1
            FROM user_sites us
            INNER JOIN roles r ON us.role_id = r.role_id
            WHERE us.user_id = @userId
              AND us.site_id = @siteId
              AND us.revoked_at IS NULL
              AND r.name = 'admin'
            LIMIT 1;
        ";

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        var userParameter = command.Parameters.Add("userId", NpgsqlDbType.Uuid);
        userParameter.Value = userId;

        var siteParameter = command.Parameters.Add("siteId", NpgsqlDbType.Uuid);
        siteParameter.Value = siteId;

        try
        {
            var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            var isAdmin = result != null;

            _logger.LogInformation("Site admin check for user {UserId} to site {SiteId}: {IsAdmin}",
                userId, siteId, isAdmin);

            return isAdmin;
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "Failed to check site admin status for user {UserId} to site {SiteId}.", userId, siteId);
            throw;
        }
    }

    public async Task<CrossSiteAssignmentResult> ValidateCrossSiteAssignmentAsync(
        Guid assignerUserId,
        Guid assigneeUserId,
        Guid targetSiteId,
        CancellationToken cancellationToken = default)
    {
        if (assignerUserId == Guid.Empty || assigneeUserId == Guid.Empty || targetSiteId == Guid.Empty)
        {
            return CrossSiteAssignmentResult.Denied("Invalid user or site identifier.");
        }

        // Check if assignee has access to the target site
        var assigneeHasAccess = await HasSiteAccessAsync(assigneeUserId, targetSiteId, cancellationToken)
            .ConfigureAwait(false);

        if (!assigneeHasAccess)
        {
            return CrossSiteAssignmentResult.Denied("Assignee does not have access to the target site.");
        }

        // Check if assigner has access to target site (same site assignment)
        var assignerHasDirectAccess = await HasSiteAccessAsync(assignerUserId, targetSiteId, cancellationToken)
            .ConfigureAwait(false);

        if (assignerHasDirectAccess)
        {
            // Same site assignment - allowed
            return CrossSiteAssignmentResult.Allowed(isCrossSite: false);
        }

        // Cross-site assignment - check if assigner has cross-site management permission
        var orgId = await GetOrgIdForSiteAsync(targetSiteId, cancellationToken).ConfigureAwait(false);
        if (!orgId.HasValue)
        {
            return CrossSiteAssignmentResult.Denied("Could not determine organization for site.");
        }

        var hasCrossSitePermission = await HasCrossSiteManagementPermissionAsync(assignerUserId, orgId.Value, cancellationToken)
            .ConfigureAwait(false);

        if (!hasCrossSitePermission)
        {
            return CrossSiteAssignmentResult.Denied("Assigner does not have permission to assign tasks across sites.");
        }

        _logger.LogInformation(
            "Cross-site task assignment validated: Assigner={AssignerId}, Assignee={AssigneeId}, Site={SiteId}",
            assignerUserId, assigneeUserId, targetSiteId);

        return CrossSiteAssignmentResult.Allowed(isCrossSite: true);
    }

    public async Task<bool> HasCrossSiteManagementPermissionAsync(Guid userId, Guid orgId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty || orgId == Guid.Empty)
        {
            return false;
        }

        // Check if user has an org-level admin role or specific cross-site permission
        const string sql = @"
            SELECT 1
            FROM user_sites us
            INNER JOIN sites s ON us.site_id = s.site_id
            INNER JOIN roles r ON us.role_id = r.role_id
            WHERE us.user_id = @userId
              AND s.org_id = @orgId
              AND us.revoked_at IS NULL
              AND (r.name = 'org_admin' OR r.name = 'admin' OR r.permissions @> '[""cross_site_task_management""]')
            LIMIT 1;
        ";

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        command.Parameters.Add("userId", NpgsqlDbType.Uuid).Value = userId;
        command.Parameters.Add("orgId", NpgsqlDbType.Uuid).Value = orgId;

        try
        {
            var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return result != null;
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "Failed to check cross-site permission for user {UserId} in org {OrgId}.", userId, orgId);
            throw;
        }
    }

    public async Task<Guid?> GetOrgIdForSiteAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        if (siteId == Guid.Empty)
        {
            return null;
        }

        const string sql = "SELECT org_id FROM sites WHERE site_id = @siteId LIMIT 1;";

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("siteId", NpgsqlDbType.Uuid).Value = siteId;

        try
        {
            var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return result is Guid orgId ? orgId : null;
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "Failed to get org ID for site {SiteId}.", siteId);
            throw;
        }
    }

    public async Task<bool> AreSitesInSameOrgAsync(Guid siteId1, Guid siteId2, CancellationToken cancellationToken = default)
    {
        if (siteId1 == Guid.Empty || siteId2 == Guid.Empty)
        {
            return false;
        }

        if (siteId1 == siteId2)
        {
            return true;
        }

        const string sql = @"
            SELECT 1
            FROM sites s1
            INNER JOIN sites s2 ON s1.org_id = s2.org_id
            WHERE s1.site_id = @siteId1
              AND s2.site_id = @siteId2
            LIMIT 1;
        ";

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        command.Parameters.Add("siteId1", NpgsqlDbType.Uuid).Value = siteId1;
        command.Parameters.Add("siteId2", NpgsqlDbType.Uuid).Value = siteId2;

        try
        {
            var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return result != null;
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "Failed to check if sites {SiteId1} and {SiteId2} are in same org.", siteId1, siteId2);
            throw;
        }
    }
}
