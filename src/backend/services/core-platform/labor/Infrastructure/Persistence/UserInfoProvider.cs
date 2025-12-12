using Harvestry.Labor.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Harvestry.Labor.Infrastructure.Persistence;

/// <summary>
/// Provides user information by querying the users and user_sites tables
/// </summary>
public class UserInfoProvider : IUserInfoProvider
{
    private readonly LaborDbContext _context;
    private readonly ILogger<UserInfoProvider> _logger;

    public UserInfoProvider(LaborDbContext context, ILogger<UserInfoProvider> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UserInfo?> GetUserInfoAsync(Guid userId, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
        {
            return null;
        }

        var sql = @"
            SELECT 
                u.user_id,
                u.first_name,
                u.last_name,
                u.profile_photo_url,
                r.role_name
            FROM users u
            LEFT JOIN user_sites us ON u.user_id = us.user_id AND us.revoked_at IS NULL
            LEFT JOIN roles r ON us.role_id = r.role_id
            WHERE u.user_id = @userId
            LIMIT 1";

        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync(ct);

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.Add(new NpgsqlParameter("@userId", userId));

            using var reader = await command.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                return new UserInfo(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.IsDBNull(3) ? null : reader.GetString(3),
                    reader.IsDBNull(4) ? null : reader.GetString(4));
            }

            return null;
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "Failed to get user info for user {UserId}.", userId);
            throw;
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    public async Task<IReadOnlyDictionary<Guid, UserInfo>> GetUserInfoBatchAsync(
        IEnumerable<Guid> userIds, 
        CancellationToken ct = default)
    {
        var userIdList = userIds.Where(id => id != Guid.Empty).ToList();
        if (userIdList.Count == 0)
        {
            return new Dictionary<Guid, UserInfo>();
        }

        var sql = @"
            SELECT DISTINCT ON (u.user_id)
                u.user_id,
                u.first_name,
                u.last_name,
                u.profile_photo_url,
                r.role_name
            FROM users u
            LEFT JOIN user_sites us ON u.user_id = us.user_id AND us.revoked_at IS NULL
            LEFT JOIN roles r ON us.role_id = r.role_id
            WHERE u.user_id = ANY(@userIds)
            ORDER BY u.user_id, us.is_primary_site DESC NULLS LAST";

        var result = new Dictionary<Guid, UserInfo>();
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync(ct);

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.Add(new NpgsqlParameter("@userIds", userIdList.ToArray()));

            using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var userId = reader.GetGuid(0);
                result[userId] = new UserInfo(
                    userId,
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.IsDBNull(3) ? null : reader.GetString(3),
                    reader.IsDBNull(4) ? null : reader.GetString(4));
            }

            return result;
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "Failed to get batch user info for {Count} users.", userIdList.Count);
            throw;
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    public async Task<IReadOnlyList<UserInfo>> GetUsersBySiteAsync(Guid siteId, CancellationToken ct = default)
    {
        if (siteId == Guid.Empty)
        {
            return Array.Empty<UserInfo>();
        }

        var sql = @"
            SELECT 
                u.user_id,
                u.first_name,
                u.last_name,
                u.profile_photo_url,
                r.role_name
            FROM users u
            JOIN user_sites us ON u.user_id = us.user_id
            LEFT JOIN roles r ON us.role_id = r.role_id
            WHERE us.site_id = @siteId
              AND us.revoked_at IS NULL
              AND u.status = 'active'
            ORDER BY u.last_name, u.first_name";

        var result = new List<UserInfo>();
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync(ct);

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.Add(new NpgsqlParameter("@siteId", siteId));

            using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                result.Add(new UserInfo(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.IsDBNull(3) ? null : reader.GetString(3),
                    reader.IsDBNull(4) ? null : reader.GetString(4)));
            }

            return result;
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "Failed to get users for site {SiteId}.", siteId);
            throw;
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    public async Task<bool> IsManagerOrSupervisorAsync(Guid userId, Guid siteId, CancellationToken ct = default)
    {
        // Fail fast for invalid inputs - security-critical method
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("IsManagerOrSupervisorAsync called with empty userId.");
            return false;
        }

        if (siteId == Guid.Empty)
        {
            _logger.LogWarning("IsManagerOrSupervisorAsync called with empty siteId.");
            return false;
        }

        // Use SELECT 1 ... LIMIT 1 pattern for consistent null-check handling
        const string sql = @"
            SELECT 1
            FROM user_sites us
            JOIN roles r ON us.role_id = r.role_id
            WHERE us.user_id = @userId
              AND us.site_id = @siteId
              AND us.revoked_at IS NULL
              AND r.role_name IN ('admin', 'manager', 'supervisor')
            LIMIT 1";

        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync(ct);

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.Add(new NpgsqlParameter("@userId", userId));
            command.Parameters.Add(new NpgsqlParameter("@siteId", siteId));

            var result = await command.ExecuteScalarAsync(ct);
            var isManagerOrSupervisor = result != null;

            _logger.LogDebug(
                "Manager/Supervisor check for user {UserId} at site {SiteId}: {Result}",
                userId, siteId, isManagerOrSupervisor);

            return isManagerOrSupervisor;
        }
        catch (PostgresException ex)
        {
            _logger.LogError(
                ex,
                "Database error checking manager/supervisor status for user {UserId} at site {SiteId}.",
                userId, siteId);
            throw;
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}
