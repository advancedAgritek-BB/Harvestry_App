using System;
using System.IO;
using AutoMapper;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Harvestry.Telemetry.Application.DeviceAdapters;
using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Application.Mappers;
using Harvestry.Telemetry.Application.Services;
using Harvestry.Telemetry.Infrastructure.Persistence;
using Harvestry.Telemetry.Infrastructure.Repositories;
using Harvestry.Telemetry.Domain.Entities;
using Harvestry.Telemetry.Infrastructure.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Xunit;

namespace Harvestry.Telemetry.IntegrationTests;

public abstract partial class IntegrationTestBase : IAsyncLifetime
{
    protected IServiceProvider Services { get; private set; } = default!;
    private string? _tempDbName;
    private string? _serverConnectionString;

    protected string ConnectionString =>
        Environment.GetEnvironmentVariable("TELEMETRY_DB_CONNECTION")
        ?? Environment.GetEnvironmentVariable("DATABASE_URL")
        ?? "postgresql://postgres:postgres@localhost:55432/harvestry_dev";

    public async Task InitializeAsync()
    {
        var sc = new ServiceCollection();

        // Logging
        sc.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Npgsql data source
        var cs = ConnectionString;
        if (cs.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) || cs.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(cs);
            var userInfo = uri.UserInfo.Split(':');
            var user = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "postgres";
            var pass = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "postgres";
            var host = uri.Host;
            var port = uri.IsDefaultPort ? 5432 : uri.Port;
            var database = uri.AbsolutePath.TrimStart('/');
            cs = $"Host={host};Port={port};Database={database};Username={user};Password={pass}";
        }

        // Optional: create a fresh temporary database per test run
        var createTemp = Environment.GetEnvironmentVariable("TELEMETRY_TEST_CREATE_TEMP_DB");
        if (string.Equals(createTemp, "true", StringComparison.OrdinalIgnoreCase))
        {
            var (serverCs, dbName) = PrepareTempDatabase(cs);
            _serverConnectionString = serverCs;
            _tempDbName = dbName;
            cs = ReplaceDatabase(serverCs, dbName);
        }
        var dsb = new NpgsqlDataSourceBuilder(cs);
        dsb.EnableDynamicJson();
        var dataSource = dsb.Build();
        sc.AddSingleton(dataSource);

        // RLS accessor + connection factory
        sc.AddSingleton<ITelemetryRlsContextAccessor, AsyncLocalTelemetryRlsContextAccessor>();
        sc.AddScoped<ITelemetryConnectionFactory, TelemetryConnectionFactory>();

        // DbContext
        sc.AddDbContext<TelemetryDbContext>((sp, options) =>
        {
            var npg = sp.GetRequiredService<NpgsqlDataSource>();
            options.UseNpgsql(npg);
        });

        // AutoMapper (manual registration to avoid test-time DI extension dependency)
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<TelemetryMappingProfile>());
        var mapper = mapperConfig.CreateMapper();
        sc.AddSingleton(mapper);

        // Repositories
        sc.AddScoped<ISensorStreamRepository, SensorStreamRepository>();
        sc.AddScoped<ISensorReadingRepository, SensorReadingRepository>();
        sc.AddScoped<IAlertRuleRepository, AlertRuleRepository>();
        sc.AddScoped<IAlertInstanceRepository, AlertInstanceRepository>();
        sc.AddScoped<ITelemetryQueryRepository, TelemetryQueryRepository>();
        sc.AddScoped<IIngestionSessionRepository, IngestionSessionRepository>();
        sc.AddScoped<IIngestionErrorRepository, IngestionErrorRepository>();

        // Services
        sc.AddScoped<IIdempotencyService, IdempotencyService>();
        sc.AddScoped<INormalizationService, NormalizationService>();
        sc.AddScoped<ITelemetryIngestService, TelemetryIngestService>();
        sc.AddScoped<IAlertEvaluationService, AlertEvaluationService>();
        sc.AddScoped<ITelemetryRealtimeDispatcher, NoopRealtimeDispatcher>();

        Services = sc.BuildServiceProvider();

        // Smoke DB connectivity and ensure schema is present
        await using var conn = await dataSource.OpenConnectionAsync();
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT 1";
            var r = await cmd.ExecuteScalarAsync();
            r.Should().NotBeNull();
        }

        await EnsureTelemetrySchemaAsync(conn);
    }

    public Task DisposeAsync()
    {
        try
        {
            if (Services is IDisposable d)
            {
                d.Dispose();
            }
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(_serverConnectionString) && !string.IsNullOrWhiteSpace(_tempDbName))
            {
                try
                {
                    using var admin = new Npgsql.NpgsqlConnection(_serverConnectionString);
                    admin.Open();
                    using var drop = admin.CreateCommand();
                    drop.CommandText = $"DROP DATABASE IF EXISTS \"{_tempDbName}\" WITH (FORCE);";
                    drop.ExecuteNonQuery();
                }
                catch
                {
                    // best-effort cleanup only
                }
            }
        }
        return Task.CompletedTask;
    }
}

/// <summary>
/// No-op realtime dispatcher for integration tests.
/// </summary>
internal sealed class NoopRealtimeDispatcher : ITelemetryRealtimeDispatcher
{
    public Task PublishAsync(IEnumerable<SensorReading> readings, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task PublishWalEventAsync(SensorReading reading, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

public abstract partial class IntegrationTestBase
{
    private static async Task EnsureTelemetrySchemaAsync(Npgsql.NpgsqlConnection connection)
    {
        // Check for core tables
        var startRoot = new DirectoryInfo(AppContext.BaseDirectory);
        await using var check = connection.CreateCommand();
        check.CommandText = @"SELECT COUNT(*) FROM information_schema.tables WHERE table_name IN ('sensor_streams','sensor_readings');";
        var count = (long)(await check.ExecuteScalarAsync() ?? 0L);
        if (count < 2)
        {
            // Run base schema and setup
            await RunMigrationFilesAsync(connection, await FindMigrationsFolderAsync(startRoot));
}

// Helpers for temporary database management
public abstract partial class IntegrationTestBase
{
    private static (string serverCs, string tempDbName) PrepareTempDatabase(string cs)
    {
        var serverCs = ReplaceDatabase(cs, "postgres");
        var tempDbName = $"telemetry_it_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{Guid.NewGuid().ToString("N").Substring(0, 6)}";

        using var admin = new Npgsql.NpgsqlConnection(serverCs);
        admin.Open();
        using (var cmd = admin.CreateCommand())
        {
            cmd.CommandText = $"CREATE DATABASE \"{tempDbName}\"";
            cmd.ExecuteNonQuery();
        }
        return (serverCs, tempDbName);
    }

    private static string ReplaceDatabase(string connString, string database)
    {
        var parts = connString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        var found = false;
        for (int i = 0; i < parts.Count; i++)
        {
            if (parts[i].StartsWith("Database=", StringComparison.OrdinalIgnoreCase) || parts[i].StartsWith("Db=", StringComparison.OrdinalIgnoreCase))
            {
                parts[i] = $"Database={database}";
                found = true;
                break;
            }
        }
        if (!found)
        {
            parts.Add($"Database={database}");
        }
        var joined = string.Join(';', parts);
        if (!joined.EndsWith(';')) joined += ';';
        return joined;
    }
}
        // Do not enforce continuous aggregates in test environment; rollups validated in staging.
        return;
    }

    private static async Task<string> FindMigrationsFolderAsync(DirectoryInfo start)
    {
        DirectoryInfo? root = start;
        for (int i = 0; i < 10 && root != null; i++)
        {
            var candidate = Path.Combine(root.FullName, "src", "database", "migrations", "telemetry");
            if (File.Exists(Path.Combine(candidate, "001_initial_schema.sql")))
            {
                return candidate;
            }
            root = root.Parent;
        }
        throw new InvalidOperationException("Telemetry migration files not found for integration tests.");
    }
    private static async Task RunMigrationFilesAsync(Npgsql.NpgsqlConnection connection, string folder)
    {
        var files = new[]
        {
            "001_initial_schema.sql",
            "002_timescaledb_setup.sql",
            "003_additional_indexes.sql",
            // Skipping RLS policies in test environment to avoid role dependencies
            // "004_rls_policies.sql",
            "005_seed_data.sql"
        };

        foreach (var file in files)
        {
            var path = Path.Combine(folder, file);
            if (!File.Exists(path)) continue;
            var sql = await File.ReadAllTextAsync(path);
            // Remove GRANT statements that depend on non-existent roles in local test DB
            var lines = sql.Split('\n');
            lines = lines.Where(l => !l.Contains(" GRANT ", StringComparison.OrdinalIgnoreCase) && !l.TrimStart().StartsWith("GRANT ", StringComparison.OrdinalIgnoreCase)).ToArray();
            sql = string.Join('\n', lines);

            if (string.Equals(Path.GetFileName(path), "002_timescaledb_setup.sql", StringComparison.OrdinalIgnoreCase))
            {
                sql = sql.Replace("if_not_exists => TRUE", "if_not_exists => TRUE, migrate_data => TRUE");
                // Remove unique index on (stream_id, message_id) which is incompatible with hypertable partitioning constraints in tests
                var filtered = new List<string>();
                foreach (var line in lines)
                {
                    var trimmed = line.TrimStart();
                    if (trimmed.StartsWith("CREATE UNIQUE INDEX IF NOT EXISTS idx_sensor_readings_stream_message", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    if (trimmed.StartsWith("ON sensor_readings (stream_id, message_id)", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    if (trimmed.StartsWith("WHERE message_id IS NOT NULL;", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    filtered.Add(line);
                }
                sql = string.Join('\n', filtered);
            }
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
