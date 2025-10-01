using System;
using System.Collections.Concurrent;
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

public sealed class RoleRepository : IRoleRepository, IDisposable
{
    private readonly IdentityDbContext _dbContext;
    private readonly IRlsContextAccessor _rlsContextAccessor;
    private readonly ILogger<RoleRepository> _logger;
    private readonly ConcurrentDictionary<Guid, Role> _roleCache = new();
    private readonly SemaphoreSlim _cacheSemaphore = new(1, 1);
    private int _disposed;

    public RoleRepository(
        IdentityDbContext dbContext,
        IRlsContextAccessor rlsContextAccessor,
        ILogger<RoleRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Role?> GetByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        if (_roleCache.TryGetValue(roleId, out var cached))
        {
            return cached;
        }

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        const string sql = RoleSelect + " WHERE role_id = @role_id LIMIT 1;";
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("role_id", NpgsqlDbType.Uuid).Value = roleId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var role = MapRole(reader);
        _roleCache[role.Id] = role;
        return role;
    }

    public async Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            throw new ArgumentException("Role name cannot be null or whitespace", nameof(roleName));
        }

        var cached = _roleCache.Values.FirstOrDefault(r => string.Equals(r.RoleName, roleName, StringComparison.OrdinalIgnoreCase));
        if (cached is not null)
        {
            return cached;
        }

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        const string sql = RoleSelect + " WHERE role_name = @role_name LIMIT 1;";
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("role_name", NpgsqlDbType.Varchar).Value = roleName;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var role = MapRole(reader);
        _roleCache[role.Id] = role;
        return role;
    }

    public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await _cacheSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_roleCache.Count > 0)
            {
                return _roleCache.Values.ToList();
            }

            var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

            const string sql = RoleSelect + " ORDER BY display_name;";
            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var role = MapRole(reader);
                _roleCache[role.Id] = role;
            }

            return _roleCache.Values.ToList();
        }
        finally
        {
            _cacheSemaphore.Release();
        }
    }

    public async Task<Role> AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
            INSERT INTO roles (
                role_id,
                role_name,
                display_name,
                description,
                permissions,
                is_system_role,
                created_at,
                updated_at)
            VALUES (
                @role_id,
                @role_name,
                @display_name,
                @description,
                @permissions,
                @is_system_role,
                @created_at,
                @updated_at);
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        PopulateParameters(command, role);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _roleCache[role.Id] = role;
        return role;
    }

    public async Task UpdateAsync(Role role, CancellationToken cancellationToken = default)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
            UPDATE roles SET
                role_name = @role_name,
                display_name = @display_name,
                description = @description,
                permissions = @permissions,
                is_system_role = @is_system_role,
                updated_at = @updated_at
            WHERE role_id = @role_id;
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        PopulateParameters(command, role);

        var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        if (rowsAffected == 0)
        {
            throw new InvalidOperationException($"Role {role.Id} not found or update was blocked");
        }
        
        _roleCache[role.Id] = role;
    }

    private async Task<NpgsqlConnection> PrepareConnectionAsync(CancellationToken cancellationToken)
    {
        var context = _rlsContextAccessor.Current;
        var userId = context.UserId == Guid.Empty ? Guid.Empty : context.UserId;
        var role = string.IsNullOrWhiteSpace(context.Role) ? "service_account" : context.Role;

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(userId, role, Guid.Empty, cancellationToken).ConfigureAwait(false);
        return connection;
    }

    private static void PopulateParameters(NpgsqlCommand command, Role role)
    {
        command.Parameters.Add("role_id", NpgsqlDbType.Uuid).Value = role.Id;
        command.Parameters.Add("role_name", NpgsqlDbType.Varchar).Value = role.RoleName;
        command.Parameters.Add("display_name", NpgsqlDbType.Varchar).Value = role.DisplayName;
        command.Parameters.Add("description", NpgsqlDbType.Text).Value = (object?)role.Description ?? DBNull.Value;
        command.Parameters.Add("permissions", NpgsqlDbType.Jsonb).Value = JsonUtilities.SerializeStringCollection(role.Permissions);
        command.Parameters.Add("is_system_role", NpgsqlDbType.Boolean).Value = role.IsSystemRole;
        command.Parameters.Add("created_at", NpgsqlDbType.TimestampTz).Value = role.CreatedAt;
        command.Parameters.Add("updated_at", NpgsqlDbType.TimestampTz).Value = role.UpdatedAt;
    }

    private static Role MapRole(NpgsqlDataReader reader)
    {
        var permissionsValue = reader["permissions"];

        return Role.Restore(
            reader.GetGuid(reader.GetOrdinal("role_id")),
            reader.GetString(reader.GetOrdinal("role_name")),
            reader.GetString(reader.GetOrdinal("display_name")),
            reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
            JsonUtilities.ToStringList(permissionsValue),
            reader.GetBoolean(reader.GetOrdinal("is_system_role")),
            reader.GetDateTime(reader.GetOrdinal("created_at")),
            reader.GetDateTime(reader.GetOrdinal("updated_at")));
    }

    private const string RoleSelect = @"
        SELECT
            role_id,
            role_name,
            display_name,
            description,
            permissions,
            is_system_role,
            created_at,
            updated_at
        FROM roles";

    public void Dispose()
    {
        // Thread-safe atomic disposal
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
            return;

        _cacheSemaphore?.Dispose();
        // Note: ConcurrentDictionary doesn't need explicit clearing on dispose
        GC.SuppressFinalize(this);
    }
}
