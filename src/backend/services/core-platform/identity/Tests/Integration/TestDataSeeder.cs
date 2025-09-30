using System;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Domain.Entities;
using Harvestry.Identity.Domain.Enums;
using Harvestry.Identity.Domain.ValueObjects;
using Harvestry.Identity.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Harvestry.Identity.Tests.Integration;

internal static class TestDataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var connection = await dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await dbContext.ResetRlsContextAsync(cancellationToken).ConfigureAwait(false);

        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("TestSeeder");

        // Simple clean-up to avoid duplicate inserts; in real tests consider using transactions/containers.
        var truncateSql = @"
            TRUNCATE TABLE sessions, badges, user_sites, users, sites, roles CASCADE;
        ";

        await using (var truncateCommand = connection.CreateCommand())
        {
            truncateCommand.CommandText = truncateSql;
            await truncateCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        const string seedSql = @"
            INSERT INTO roles (role_id, role_name, display_name, permissions, is_system_role)
            VALUES
                ('00000000-0000-0000-0000-000000000901', 'operator', 'Operator', '["users:read"]', true),
                ('00000000-0000-0000-0000-000000000902', 'manager', 'Manager', '["users:read", "users:update", "badges:create", "badges:revoke"]', true),
                ('00000000-0000-0000-0000-000000000903', 'admin', 'Admin', '["*:*"]', true)
            ON CONFLICT (role_name) DO NOTHING;

            INSERT INTO sites (site_id, org_id, site_name, site_code, status)
            VALUES
                ('00000000-0000-0000-0000-000000000a01', '00000000-0000-0000-0000-00000000aaaa', 'Denver Main', 'DEN-001', 'active'),
                ('00000000-0000-0000-0000-000000000a02', '00000000-0000-0000-0000-00000000bbbb', 'Boulder Annex', 'BOL-001', 'active')
            ON CONFLICT (site_code) DO NOTHING;

            INSERT INTO users (user_id, email, first_name, last_name, status)
            VALUES
                ('00000000-0000-0000-0000-000000000101', 'operator@denver.example', 'Olivia', 'Operator', 'active'),
                ('00000000-0000-0000-0000-000000000102', 'manager@denver.example', 'Mason', 'Manager', 'active'),
                ('00000000-0000-0000-0000-000000000103', 'admin@example', 'Ada', 'Admin', 'active'),
                ('00000000-0000-0000-0000-000000000104', 'operator@boulder.example', 'Bea', 'Operator', 'active')
            ON CONFLICT (user_id) DO NOTHING;

            INSERT INTO user_sites (user_site_id, user_id, site_id, role_id, is_primary_site)
            VALUES
                ('00000000-0000-0000-0000-000000001001', '00000000-0000-0000-0000-000000000101', '00000000-0000-0000-0000-000000000a01', '00000000-0000-0000-0000-000000000901', true),
                ('00000000-0000-0000-0000-000000001002', '00000000-0000-0000-0000-000000000102', '00000000-0000-0000-0000-000000000a01', '00000000-0000-0000-0000-000000000902', true),
                ('00000000-0000-0000-0000-000000001003', '00000000-0000-0000-0000-000000000103', '00000000-0000-0000-0000-000000000a01', '00000000-0000-0000-0000-000000000903', true),
                ('00000000-0000-0000-0000-000000001004', '00000000-0000-0000-0000-000000000104', '00000000-0000-0000-0000-000000000a02', '00000000-0000-0000-0000-000000000901', true)
            ON CONFLICT (user_site_id) DO NOTHING;

            INSERT INTO badges (badge_id, user_id, site_id, badge_code, status)
            VALUES
                ('00000000-0000-0000-0000-00000000b001', '00000000-0000-0000-0000-000000000101', '00000000-0000-0000-0000-000000000a01', 'DEN-OP-001', 'active'),
                ('00000000-0000-0000-0000-00000000b002', '00000000-0000-0000-0000-000000000102', '00000000-0000-0000-0000-000000000a01', 'DEN-MN-001', 'active'),
                ('00000000-0000-0000-0000-00000000b003', '00000000-0000-0000-0000-000000000104', '00000000-0000-0000-0000-000000000a02', 'BOL-OP-001', 'active')
            ON CONFLICT (badge_id) DO NOTHING;

            INSERT INTO abac_permissions (role_name, action, resource_type, requires_two_person)
            VALUES
                ('manager', 'badges:create', 'badge', FALSE),
                ('manager', 'badges:revoke', 'badge', FALSE),
                ('manager', 'inventory:destroy', 'lot', TRUE),
                ('admin', 'badges:create', 'badge', FALSE),
                ('admin', 'badges:revoke', 'badge', FALSE),
                ('admin', 'inventory:destroy', 'lot', FALSE)
            ON CONFLICT (role_name, action, resource_type) DO NOTHING;
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = seedSql;
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Integration test data seeded");
    }
}
