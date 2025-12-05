using System;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Spatial.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Harvestry.Spatial.Tests.Integration;

internal static class SpatialTestDataSeeder
{
    public static readonly Guid OrganizationId = Guid.Parse("00000000-0000-0000-0000-0000000000A1");
    public static readonly Guid SiteId = Guid.Parse("00000000-0000-0000-0000-0000000000B1");
    public static readonly Guid ManagerUserId = Guid.Parse("00000000-0000-0000-0000-0000000000C1");
    public static readonly Guid ManagerUserSiteId = Guid.Parse("00000000-0000-0000-0000-0000000000D1");

    public static async Task SeedAsync(IServiceProvider services, NpgsqlTransaction transaction, CancellationToken cancellationToken)
    {
        var dbContext = services.GetRequiredService<SpatialDbContext>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("SpatialTestDataSeeder");

        await dbContext.SetRlsContextAsync(Guid.Empty, "admin", null, cancellationToken);

        var connection = transaction.Connection ?? throw new InvalidOperationException("Transaction connection not available.");
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
            INSERT INTO organizations (organization_id, name, slug, status, metadata, created_at, updated_at)
            VALUES (@org_id, 'Spatial Test Org', 'spatial-test-org', 'active', '{}'::jsonb, NOW(), NOW())
            ON CONFLICT (organization_id) DO NOTHING;

            INSERT INTO sites (site_id, org_id, site_name, site_code, status, timezone, site_policies, metadata, created_at, updated_at)
            VALUES (@site_id, @org_id, 'Spatial Integration Site', 'SPATIAL-INT-01', 'active', 'UTC', '{}'::jsonb, '{}'::jsonb, NOW(), NOW())
            ON CONFLICT (site_id) DO NOTHING;

            INSERT INTO users (user_id, email, email_verified, first_name, last_name, status, created_at, updated_at)
            VALUES (@user_id, 'spatial.integration@harvestry.local', TRUE, 'Spatial', 'Manager', 'active', NOW(), NOW())
            ON CONFLICT (user_id) DO NOTHING;

            INSERT INTO user_sites (user_site_id, user_id, site_id, role_id, is_primary_site, assigned_at)
            SELECT @user_site_id, @user_id, @site_id, role_id, TRUE, NOW()
            FROM roles
            WHERE role_name = 'manager'
            ON CONFLICT (user_id, site_id) DO NOTHING;
        ";

        command.Parameters.AddWithValue("org_id", OrganizationId);
        command.Parameters.AddWithValue("site_id", SiteId);
        command.Parameters.AddWithValue("user_id", ManagerUserId);
        command.Parameters.AddWithValue("user_site_id", ManagerUserSiteId);

        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (PostgresException ex)
        {
            logger.LogError(ex, "Failed to seed spatial integration test data.");
            throw;
        }
        finally
        {
            await dbContext.ResetRlsContextAsync(cancellationToken);
        }
    }
}
