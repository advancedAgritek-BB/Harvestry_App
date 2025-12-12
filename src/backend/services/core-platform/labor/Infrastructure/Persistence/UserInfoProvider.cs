using Harvestry.Labor.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Harvestry.Labor.Infrastructure.Persistence;

/// <summary>
/// Provides user information by querying the users and user_sites tables
/// </summary>
public class UserInfoProvider : IUserInfoProvider
{
    private readonly LaborDbContext _context;

    public UserInfoProvider(LaborDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<UserInfo?> GetUserInfoAsync(Guid userId, CancellationToken ct = default)
    {
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
        finally
        {
            await connection.CloseAsync();
        }
    }

    public async Task<IReadOnlyDictionary<Guid, UserInfo>> GetUserInfoBatchAsync(IEnumerable<Guid> userIds, CancellationToken ct = default)
    {
        var userIdList = userIds.ToList();
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
        finally
        {
            await connection.CloseAsync();
        }
    }

    public async Task<IReadOnlyList<UserInfo>> GetUsersBySiteAsync(Guid siteId, CancellationToken ct = default)
    {
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
        finally
        {
            await connection.CloseAsync();
        }
    }

    public async Task<bool> IsManagerOrSupervisorAsync(Guid userId, Guid siteId, CancellationToken ct = default)
    {
        var sql = @"
            SELECT EXISTS (
                SELECT 1
                FROM user_sites us
                JOIN roles r ON us.role_id = r.role_id
                WHERE us.user_id = @userId
                  AND us.site_id = @siteId
                  AND us.revoked_at IS NULL
                  AND r.role_name IN ('admin', 'manager', 'supervisor')
            )";

        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync(ct);

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.Add(new NpgsqlParameter("@userId", userId));
            command.Parameters.Add(new NpgsqlParameter("@siteId", siteId));

            var result = await command.ExecuteScalarAsync(ct);
            return result is bool boolResult && boolResult;
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}
