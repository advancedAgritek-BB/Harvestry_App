using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Harvestry.Genetics.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Harvestry.Genetics.Tests.Integration;

/// <summary>
/// Factory for E2E tests using real PostgreSQL database with Testcontainers.
/// </summary>
public sealed class GeneticsE2EFactory : WebApplicationFactory<Program>, IAsyncDisposable
{
    private bool _initialized;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private PostgreSqlContainer? _postgresContainer;
    private string? _connectionString;

    private const string SeedSiteId = "550e8400-e29b-41d4-a716-446655440000";
    private const string SeedUserId = "550e8400-e29b-41d4-a716-446655440000";
    private const string PropagationStageId = "550e8400-e29b-41d4-a716-446655440001";
    private const string VegetativeStageId = "550e8400-e29b-41d4-a716-446655440002";
    private const string GeneticsId = "660e8400-e29b-41d4-a716-446655440001";
    private const string StrainId = "770e8400-e29b-41d4-a716-446655440001";
    private const string MotherPlantId = "880e8400-e29b-41d4-a716-446655440001";

    public string ConnectionString
    {
        get
        {
            if (_connectionString is null)
            {
                throw new InvalidOperationException("Connection string requested before Testcontainer initialization. Ensure InitializeAsync was awaited or CreateClient was called.");
            }

            return _connectionString;
        }
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        await _initializationLock.WaitAsync();
        try
        {
            if (_initialized)
            {
                return;
            }

            _postgresContainer ??= new PostgreSqlBuilder()
                .WithImage("postgres:15-alpine")
                .WithDatabase("harvestry_genetics_test")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .WithEnvironment("POSTGRES_HOST_AUTH_METHOD", "trust")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
                .Build();

            await _postgresContainer.StartAsync().ConfigureAwait(false);

            var builder = new NpgsqlConnectionStringBuilder(_postgresContainer.GetConnectionString());
            _connectionString = builder.ConnectionString;

            await InitializeDatabaseAsync().ConfigureAwait(false);
            _initialized = true;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public new async ValueTask DisposeAsync()
    {
        _initialized = false;
        _connectionString = null;

        if (_postgresContainer is not null)
        {
            await _postgresContainer.DisposeAsync().ConfigureAwait(false);
            _postgresContainer = null;
        }
        
        await base.DisposeAsync().ConfigureAwait(false);
    }

    private async Task InitializeDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        // Minimal identity tables required for foreign keys / RLS helpers
        const string bootstrapSql = @"
            DO $$
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'harvestry_app') THEN
                    CREATE ROLE harvestry_app;
                END IF;

                EXECUTE 'ALTER ROLE harvestry_app SET search_path = genetics, public';
            END$$;

            CREATE EXTENSION IF NOT EXISTS ""uuid-ossp"";

            CREATE TABLE IF NOT EXISTS sites (
                site_id UUID PRIMARY KEY,
                org_id UUID,
                site_name TEXT,
                site_code TEXT UNIQUE
            );

            CREATE TABLE IF NOT EXISTS user_sites (
                user_site_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                user_id UUID NOT NULL,
                site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
                revoked_at TIMESTAMPTZ
            );
        ";

        await ExecuteScriptAsync(connection, bootstrapSql).ConfigureAwait(false);
        await ExecuteScriptAsync(connection, await ReadMigrationScriptAsync("20251002_01_CreateGeneticsSchema.sql")).ConfigureAwait(false);

        await GrantGeneticsPrivilegesAsync(connection).ConfigureAwait(false);

        await SeedInitialDataAsync(connection).ConfigureAwait(false);

        await ExecuteScriptAsync(connection, await ReadMigrationScriptAsync("20251002_02_EnableGeneticsRls.sql")).ConfigureAwait(false);
        await ExecuteScriptAsync(connection, await ReadMigrationScriptAsync("20251002_03_AddUniqueConstraints.sql")).ConfigureAwait(false);
    }

    private static async Task GrantGeneticsPrivilegesAsync(NpgsqlConnection connection)
    {
        const string sql = @"
            GRANT USAGE ON SCHEMA genetics TO harvestry_app;
            GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA genetics TO harvestry_app;
            GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA genetics TO harvestry_app;

            ALTER DEFAULT PRIVILEGES IN SCHEMA genetics
                GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO harvestry_app;

            ALTER DEFAULT PRIVILEGES IN SCHEMA genetics
                GRANT USAGE, SELECT ON SEQUENCES TO harvestry_app;
        ";

        await ExecuteScriptAsync(connection, sql).ConfigureAwait(false);
    }

    private async Task SeedInitialDataAsync(NpgsqlConnection connection)
    {
        const string sqlTemplate = @"
            INSERT INTO sites (site_id, site_name, site_code)
            VALUES ('{SITE_ID}', 'Test Cultivation', 'TEST-CULT-01')
            ON CONFLICT (site_id) DO NOTHING;

            INSERT INTO user_sites (user_site_id, user_id, site_id, revoked_at)
            VALUES (gen_random_uuid(), '{USER_ID}', '{SITE_ID}', NULL)
            ON CONFLICT DO NOTHING;

            INSERT INTO genetics.batch_stage_definitions (
                id, site_id, stage_key, display_name, description, sequence_order,
                is_terminal, requires_harvest_metrics, created_at, created_by_user_id, updated_at, updated_by_user_id)
            VALUES
                ('{PROP_STAGE_ID}', '{SITE_ID}', 'propagation', 'Propagation', 'Initial propagation', 1, FALSE, FALSE, NOW(), '{USER_ID}', NOW(), '{USER_ID}'),
                ('{VEG_STAGE_ID}', '{SITE_ID}', 'vegetative', 'Vegetative', 'Vegetative growth', 2, FALSE, FALSE, NOW(), '{USER_ID}', NOW(), '{USER_ID}')
            ON CONFLICT (id) DO NOTHING;

            INSERT INTO genetics.genetics (
                id, site_id, name, description, genetic_type, thc_min_percentage, thc_max_percentage,
                cbd_min_percentage, cbd_max_percentage, flowering_time_days, yield_potential,
                growth_characteristics, terpene_profile, breeding_notes,
                created_at, created_by_user_id, updated_at, updated_by_user_id)
            VALUES (
                '{GENETICS_ID}', '{SITE_ID}', 'Northern Lights', 'Classic indica dominant strain', 'Indica',
                16.0, 21.0, 0.1, 0.5, 45, 'High', '{}'::jsonb, '{}'::jsonb, NULL,
                NOW(), '{USER_ID}', NOW(), '{USER_ID}')
            ON CONFLICT (id) DO NOTHING;

            INSERT INTO genetics.strains (
                id, site_id, genetics_id, phenotype_id, name, breeder, seed_bank, description,
                cultivation_notes, expected_harvest_window_days, target_environment, compliance_requirements,
                created_at, created_by_user_id, updated_at, updated_by_user_id)
            VALUES (
                '{STRAIN_ID}', '{SITE_ID}', '{GENETICS_ID}', NULL, 'Northern Lights #1', 'Harvestry', 'Seed Bank',
                'Primary production strain', 'Thrives in controlled env', 60, '{}'::jsonb, '{""requiresMetrcTagging"": true}'::jsonb,
                NOW(), '{USER_ID}', NOW(), '{USER_ID}')
            ON CONFLICT (id) DO NOTHING;

            INSERT INTO genetics.batches (
                id, site_id, strain_id, batch_code, batch_name, batch_type, source_type,
                parent_batch_id, generation, plant_count, target_plant_count, current_stage_id,
                stage_started_at, expected_harvest_date, actual_harvest_date, location_id, room_id, zone_id,
                status, notes, metadata, created_at, created_by_user_id, updated_at, updated_by_user_id)
            VALUES (
                gen_random_uuid(), '{SITE_ID}', '{STRAIN_ID}', 'BATCH-001', 'Baseline Batch', 'MotherPlant', 'Propagation',
                NULL, 1, 100, 100, '{VEG_STAGE_ID}', NOW(), NULL, NULL, NULL, NULL, NULL,
                'Active', NULL, '{}'::jsonb, NOW(), '{USER_ID}', NOW(), '{USER_ID}')
            ON CONFLICT DO NOTHING;

            INSERT INTO genetics.mother_plants (
                id, site_id, batch_id, strain_id, plant_tag, status, location_id, room_id,
                date_established, last_propagation_date, propagation_count, max_propagation_count,
                notes, metadata, created_at, created_by_user_id, updated_at, updated_by_user_id)
            VALUES (
                '{MOTHER_ID}', '{SITE_ID}', (
                    SELECT id FROM genetics.batches WHERE batch_code = 'BATCH-001' LIMIT 1
                ), '{STRAIN_ID}', 'MP-001', 'Active', NULL, NULL,
                CURRENT_DATE - INTERVAL '60 days', NULL, 0, 50,
                NULL, '{}'::jsonb, NOW(), '{USER_ID}', NOW(), '{USER_ID}')
            ON CONFLICT (id) DO NOTHING;
        ";

        var sql = sqlTemplate
            .Replace("{SITE_ID}", SeedSiteId)
            .Replace("{USER_ID}", SeedUserId)
            .Replace("{PROP_STAGE_ID}", PropagationStageId)
            .Replace("{VEG_STAGE_ID}", VegetativeStageId)
            .Replace("{GENETICS_ID}", GeneticsId)
            .Replace("{STRAIN_ID}", StrainId)
            .Replace("{MOTHER_ID}", MotherPlantId);

        await ExecuteScriptAsync(connection, sql).ConfigureAwait(false);
    }
    private static async Task ExecuteScriptAsync(NpgsqlConnection connection, string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return;
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            await command.ExecuteNonQueryAsync();
        }
        catch (PostgresException ex)
        {
            throw new InvalidOperationException("Failed to execute SQL script", ex);
        }
    }

    private static async Task<string> ReadMigrationScriptAsync(string fileName)
    {
        var repoRoot = GetRepositoryRoot();
        var fullPath = Path.Combine(repoRoot, "src", "database", "migrations", "frp03", fileName);
        return await File.ReadAllTextAsync(fullPath);
    }

    private static string GetRepositoryRoot()
    {
        var current = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        for (var i = 0; i < 9; i++)
        {
            current = Directory.GetParent(current)!.FullName;
        }

        return current;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // NOTE: Blocking on async initialization here can cause deadlocks in some contexts.
        // Callers should ensure InitializeAsync() is called and awaited before using CreateClient().
        // This synchronous blocking is required by the WebApplicationFactory base class design.
        if (!_initialized)
        {
            InitializeAsync().GetAwaiter().GetResult();
        }

        builder.ConfigureAppConfiguration((context, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:GeneticsDb"] = ConnectionString,
                ["Database:DisablePasswordProvider"] = bool.TrueString
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(NpgsqlDataSource));

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(ConnectionString);
            dataSourceBuilder.EnableDynamicJson();
            services.AddSingleton(dataSourceBuilder.Build());
        });
    }
}
