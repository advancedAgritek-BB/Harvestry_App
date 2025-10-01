using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Harvestry.Identity.Tests.Integration;

internal static class TestDataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, NpgsqlTransaction transaction, CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        
        // Switch to postgres role to bypass RLS for seeding operations
        await using (var roleCommand = transaction.Connection!.CreateCommand())
        {
            roleCommand.Transaction = transaction;
            roleCommand.CommandText = "SET LOCAL ROLE postgres;";
            await roleCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("TestSeeder");

        await TruncateTablesAsync(transaction, cancellationToken).ConfigureAwait(false);
        var roleLookup = await EnsureRolesAsync(transaction, cancellationToken).ConfigureAwait(false);
        await UpsertOrganizationsAndSitesAsync(transaction, cancellationToken).ConfigureAwait(false);
        await UpsertUsersAsync(transaction, cancellationToken).ConfigureAwait(false);
        await UpsertUserSitesAsync(transaction, roleLookup, cancellationToken).ConfigureAwait(false);
        await UpsertBadgesAsync(transaction, cancellationToken).ConfigureAwait(false);
        await UpsertAbacPermissionsAsync(transaction, cancellationToken).ConfigureAwait(false);

        // Reset to app role after seeding
        await using (var resetRoleCommand = transaction.Connection!.CreateCommand())
        {
            resetRoleCommand.Transaction = transaction;
            resetRoleCommand.CommandText = "RESET ROLE;";
            await resetRoleCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        logger.LogInformation("Integration test data seeded");
    }

    private static async Task TruncateTablesAsync(NpgsqlTransaction transaction, CancellationToken cancellationToken)
    {
        const string sql = @"
            DELETE FROM sessions;
            DELETE FROM badges;
            DELETE FROM user_sites;
            DELETE FROM two_person_approvals;
            DELETE FROM authorization_audit;
            DELETE FROM abac_permissions;
            DELETE FROM users;
        ";

        await ExecuteNonQueryAsync(transaction, sql, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<(Guid OperatorRoleId, Guid ManagerRoleId, Guid AdminRoleId)> EnsureRolesAsync(NpgsqlTransaction transaction, CancellationToken cancellationToken)
    {
        var roles = new Dictionary<string, string[]>
        {
            ["operator"] = new[] { "users:read" },
            ["manager"] = new[] { "users:read", "users:update", "badges:create", "badges:revoke" },
            ["admin"] = new[] { "*:*" }
        };

        foreach (var (roleName, permissions) in roles)
        {
            await UpsertRoleAsync(transaction, roleName, char.ToUpper(roleName[0]) + roleName[1..], permissions, cancellationToken).ConfigureAwait(false);
        }

        const string lookupSql = @"
            SELECT role_name, role_id
            FROM roles
            WHERE role_name IN ('operator', 'manager', 'admin');
        ";

        Guid? operatorId = null;
        Guid? managerId = null;
        Guid? adminId = null;

        await using var command = transaction.Connection!.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = lookupSql;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var name = reader.GetString(0);
            var id = reader.GetGuid(1);
            switch (name)
            {
                case "operator":
                    operatorId = id;
                    break;
                case "manager":
                    managerId = id;
                    break;
                case "admin":
                    adminId = id;
                    break;
            }
        }

        if (operatorId is null || managerId is null || adminId is null)
        {
            throw new InvalidOperationException("Required roles (operator, manager, admin) were not found after seeding.");
        }

        return (operatorId.Value, managerId.Value, adminId.Value);
    }

    private static async Task UpsertRoleAsync(NpgsqlTransaction transaction, string roleName, string displayName, IEnumerable<string> permissions, CancellationToken cancellationToken)
    {
        const string sql = @"
            INSERT INTO roles (role_name, display_name, permissions, is_system_role)
            VALUES (@role_name, @display_name, @permissions, TRUE)
            ON CONFLICT (role_name) DO UPDATE
            SET display_name = EXCLUDED.display_name,
                permissions = EXCLUDED.permissions,
                updated_at = NOW();
        ";

        await using var command = transaction.Connection!.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.Parameters.AddWithValue("@role_name", roleName);
        command.Parameters.AddWithValue("@display_name", displayName);
        command.Parameters.Add("@permissions", NpgsqlDbType.Jsonb).Value = JsonSerializer.Serialize(permissions);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task UpsertOrganizationsAndSitesAsync(NpgsqlTransaction transaction, CancellationToken cancellationToken)
    {
        const string organizationsSql = @"
            INSERT INTO organizations (organization_id, name, slug, status)
            VALUES
                ('00000000-0000-0000-0000-00000000aaaa', 'Delta Growers Cooperative', 'delta-growers', 'active'),
                ('00000000-0000-0000-0000-00000000bbbb', 'Mountain Flower Group', 'mountain-flower', 'active')
            ON CONFLICT (organization_id) DO UPDATE
                SET name = EXCLUDED.name,
                    slug = EXCLUDED.slug,
                    status = EXCLUDED.status,
                    updated_at = NOW();
        ";

        await ExecuteNonQueryAsync(transaction, organizationsSql, cancellationToken).ConfigureAwait(false);

        const string sitesSql = @"
            INSERT INTO sites (site_id, org_id, site_name, site_code, status)
            VALUES
                ('00000000-0000-0000-0000-000000000a01', '00000000-0000-0000-0000-00000000aaaa', 'Denver Main', 'DEN-001', 'active'),
                ('00000000-0000-0000-0000-000000000a02', '00000000-0000-0000-0000-00000000bbbb', 'Boulder Annex', 'BOL-001', 'active')
            ON CONFLICT (site_id) DO UPDATE
                SET org_id = EXCLUDED.org_id,
                    site_name = EXCLUDED.site_name,
                    site_code = EXCLUDED.site_code,
                    status = EXCLUDED.status,
                    updated_at = NOW();
        ";

        await ExecuteNonQueryAsync(transaction, sitesSql, cancellationToken).ConfigureAwait(false);
    }

    private static async Task UpsertUsersAsync(NpgsqlTransaction transaction, CancellationToken cancellationToken)
    {
        const string sql = @"
            INSERT INTO users (user_id, email, first_name, last_name, display_name, status)
            VALUES
                ('00000000-0000-0000-0000-000000000101', 'operator@denver.example', 'Olivia', 'Operator', 'Olivia Operator', 'active'),
                ('00000000-0000-0000-0000-000000000102', 'manager@denver.example', 'Mason', 'Manager', 'Mason Manager', 'active'),
                ('00000000-0000-0000-0000-000000000103', 'admin@example.com', 'Ada', 'Admin', 'Ada Admin', 'active'),
                ('00000000-0000-0000-0000-000000000104', 'operator@boulder.example', 'Bea', 'Operator', 'Bea Operator', 'active')
            ON CONFLICT (user_id) DO UPDATE
                SET email = EXCLUDED.email,
                    first_name = EXCLUDED.first_name,
                    last_name = EXCLUDED.last_name,
                    display_name = EXCLUDED.display_name,
                    status = EXCLUDED.status,
                    updated_at = NOW();
        ";

        await ExecuteNonQueryAsync(transaction, sql, cancellationToken).ConfigureAwait(false);
    }
    private static async Task UpsertUserSitesAsync(NpgsqlTransaction transaction, (Guid OperatorRoleId, Guid ManagerRoleId, Guid AdminRoleId) roles, CancellationToken cancellationToken)
    {
        const string sql = @"
            INSERT INTO user_sites (user_site_id, user_id, site_id, role_id, is_primary_site, assigned_at, assigned_by)
            VALUES
                ('00000000-0000-0000-0000-000000001001', '00000000-0000-0000-0000-000000000101', '00000000-0000-0000-0000-000000000a01', @operatorRoleId, true, NOW(), '00000000-0000-0000-0000-000000000103'),
                ('00000000-0000-0000-0000-000000001002', '00000000-0000-0000-0000-000000000102', '00000000-0000-0000-0000-000000000a01', @managerRoleId, true, NOW(), '00000000-0000-0000-0000-000000000103'),
                ('00000000-0000-0000-0000-000000001003', '00000000-0000-0000-0000-000000000103', '00000000-0000-0000-0000-000000000a01', @adminRoleId, true, NOW(), '00000000-0000-0000-0000-000000000103'),
                ('00000000-0000-0000-0000-000000001004', '00000000-0000-0000-0000-000000000104', '00000000-0000-0000-0000-000000000a02', @operatorRoleId, true, NOW(), '00000000-0000-0000-0000-000000000103')
            ON CONFLICT (user_site_id) DO UPDATE
                SET role_id = EXCLUDED.role_id,
                    is_primary_site = EXCLUDED.is_primary_site,
                    assigned_at = EXCLUDED.assigned_at,
                    assigned_by = EXCLUDED.assigned_by,
                    updated_at = NOW();
        ";

        await using var command = transaction.Connection!.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.Parameters.AddWithValue("@operatorRoleId", roles.OperatorRoleId);
        command.Parameters.AddWithValue("@managerRoleId", roles.ManagerRoleId);
        command.Parameters.AddWithValue("@adminRoleId", roles.AdminRoleId);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task UpsertBadgesAsync(NpgsqlTransaction transaction, CancellationToken cancellationToken)
    {
        const string sql = @"
            INSERT INTO badges (badge_id, user_id, site_id, badge_code, status, issued_at)
            VALUES
                ('00000000-0000-0000-0000-00000000b001', '00000000-0000-0000-0000-000000000101', '00000000-0000-0000-0000-000000000a01', 'DEN-OP-001', 'active', NOW()),
                ('00000000-0000-0000-0000-00000000b002', '00000000-0000-0000-0000-000000000102', '00000000-0000-0000-0000-000000000a01', 'DEN-MN-001', 'active', NOW()),
                ('00000000-0000-0000-0000-00000000b003', '00000000-0000-0000-0000-000000000104', '00000000-0000-0000-0000-000000000a02', 'BOL-OP-001', 'active', NOW())
            ON CONFLICT (badge_id) DO UPDATE
                SET user_id = EXCLUDED.user_id,
                    site_id = EXCLUDED.site_id,
                    badge_code = EXCLUDED.badge_code,
                    status = EXCLUDED.status,
                    issued_at = EXCLUDED.issued_at,
                    updated_at = NOW();
        ";

        await ExecuteNonQueryAsync(transaction, sql, cancellationToken).ConfigureAwait(false);
    }
    private static async Task UpsertAbacPermissionsAsync(NpgsqlTransaction transaction, CancellationToken cancellationToken)
    {
        const string sql = @"
            INSERT INTO abac_permissions (role_name, action, resource_type, requires_two_person, requires_reason)
            VALUES
                ('manager', 'badges:create', 'badge', FALSE, FALSE),
                ('manager', 'badges:revoke', 'badge', FALSE, FALSE),
                ('manager', 'inventory:destroy', 'lot', TRUE, TRUE),
                ('admin', 'badges:create', 'badge', FALSE, FALSE),
                ('admin', 'badges:revoke', 'badge', FALSE, FALSE),
                ('admin', 'inventory:destroy', 'lot', FALSE, FALSE)
            ON CONFLICT (role_name, action, resource_type) DO UPDATE
                SET requires_two_person = EXCLUDED.requires_two_person,
                    requires_reason = EXCLUDED.requires_reason;
        ";

        await ExecuteNonQueryAsync(transaction, sql, cancellationToken).ConfigureAwait(false);
    }
    private static async Task ExecuteNonQueryAsync(NpgsqlTransaction transaction, string sql, CancellationToken cancellationToken)
    {
        await using var command = transaction.Connection!.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
