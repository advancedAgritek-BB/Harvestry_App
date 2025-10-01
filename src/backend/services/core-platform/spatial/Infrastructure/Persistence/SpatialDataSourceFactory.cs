using System;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Harvestry.Spatial.Infrastructure.Persistence;

/// <summary>
/// Provides consistent configuration for pooled Npgsql data sources used by the spatial service.
/// </summary>
public static class SpatialDataSourceFactory
{
    public static NpgsqlDataSource Create(
        string connectionString,
        Action<NpgsqlDataSourceBuilder>? configure = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or whitespace", nameof(connectionString));
        }

        var originalBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        var builder = new NpgsqlDataSourceBuilder(connectionString);
        var csBuilder = builder.ConnectionStringBuilder;

        if (!originalBuilder.ContainsKey("Pooling"))
        {
            csBuilder.Pooling = true;
        }

        if (!originalBuilder.ContainsKey("MinPoolSize") && !originalBuilder.ContainsKey("Minimum Pool Size"))
        {
            csBuilder.MinPoolSize = 5;
        }

        if (!originalBuilder.ContainsKey("MaxPoolSize") && !originalBuilder.ContainsKey("Maximum Pool Size"))
        {
            csBuilder.MaxPoolSize = 50;
        }

        if (!originalBuilder.ContainsKey("Timeout"))
        {
            csBuilder.Timeout = 15;
        }

        if (!originalBuilder.ContainsKey("CommandTimeout") && !originalBuilder.ContainsKey("Command Timeout"))
        {
            csBuilder.CommandTimeout = 30;
        }

        configure?.Invoke(builder);
        return builder.Build();
    }

    public static NpgsqlDataSource CreateFromConfiguration(
        IConfiguration configuration,
        string connectionStringKey = "Spatial",
        Action<NpgsqlDataSourceBuilder>? configure = null)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var connectionString = ResolveConnectionString(configuration, connectionStringKey);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{connectionStringKey}' was not found.");
        }

        return Create(connectionString, configure);
    }

    private static string? ResolveConnectionString(IConfiguration configuration, string key)
    {
        var envConnection = configuration["SPATIAL_DB_CONNECTION"] ?? Environment.GetEnvironmentVariable("SPATIAL_DB_CONNECTION");
        if (!string.IsNullOrWhiteSpace(envConnection))
        {
            return envConnection;
        }

        var connection = configuration.GetConnectionString(key);
        if (!string.IsNullOrWhiteSpace(connection))
        {
            return connection;
        }

        if (key.Contains(':', StringComparison.Ordinal))
        {
            connection = configuration[key];
            if (!string.IsNullOrWhiteSpace(connection))
            {
                return connection;
            }
        }
        else
        {
            connection = configuration[$"{key}:ConnectionString"];
            if (!string.IsNullOrWhiteSpace(connection))
            {
                return connection;
            }

            connection = configuration[$"ConnectionStrings:{key}"];
            if (!string.IsNullOrWhiteSpace(connection))
            {
                return connection;
            }
        }

        return null;
    }
}
