using System;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Harvestry.Identity.Infrastructure.Persistence;

/// <summary>
/// Factory helper for constructing <see cref="NpgsqlDataSource"/> instances used by
/// the identity service. Centralises connection pooling defaults so Program.cs
/// can register a single shared data source per service scope.
/// </summary>
public static class IdentityDataSourceFactory
{
    /// <summary>
    /// Creates a pooled <see cref="NpgsqlDataSource"/> configured for the identity service.
    /// </summary>
    /// <param name="connectionString">PostgreSQL connection string.</param>
    /// <param name="configure">Optional additional builder configuration.</param>
    public static NpgsqlDataSource Create(
        string connectionString,
        Action<NpgsqlDataSourceBuilder>? configure = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or whitespace", nameof(connectionString));
        }

        // Parse the original connection string to detect explicitly provided keys
        var originalBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        
        var builder = new NpgsqlDataSourceBuilder(connectionString);
        var connectionStringBuilder = builder.ConnectionStringBuilder;

        // Ensure sensible pooling defaults only if not explicitly set in the original connection string.
        // We detect explicit settings by checking if the property is present in the parsed builder.
        var hasExplicitPooling = originalBuilder.ContainsKey("Pooling");
        var hasExplicitMinPoolSize = originalBuilder.ContainsKey("MinPoolSize") || originalBuilder.ContainsKey("Minimum Pool Size");
        var hasExplicitMaxPoolSize = originalBuilder.ContainsKey("MaxPoolSize") || originalBuilder.ContainsKey("Maximum Pool Size");
        var hasExplicitTimeout = originalBuilder.ContainsKey("Timeout");
        var hasExplicitCommandTimeout = originalBuilder.ContainsKey("CommandTimeout") || originalBuilder.ContainsKey("Command Timeout");

        // Apply factory defaults only when keys are not explicitly provided
        if (!hasExplicitPooling)
        {
            connectionStringBuilder.Pooling = true;
        }
        if (!hasExplicitMinPoolSize)
        {
            connectionStringBuilder.MinPoolSize = 5;
        }
        if (!hasExplicitMaxPoolSize)
        {
            connectionStringBuilder.MaxPoolSize = 50;
        }
        if (!hasExplicitTimeout)
        {
            connectionStringBuilder.Timeout = 15;
        }
        if (!hasExplicitCommandTimeout)
        {
            connectionStringBuilder.CommandTimeout = 30;
        }

        configure?.Invoke(builder);

        return builder.Build();
    }

    /// <summary>
    /// Convenience overload that reads the connection string from configuration.
    /// </summary>
    public static NpgsqlDataSource CreateFromConfiguration(
        IConfiguration configuration,
        string connectionStringKey = "Identity:ConnectionString",
        Action<NpgsqlDataSourceBuilder>? configure = null)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var connectionString = configuration.GetConnectionString(connectionStringKey) ??
                                configuration[connectionStringKey];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"PostgreSQL connection string '{connectionStringKey}' was not found.");
        }

        return Create(connectionString, configure);
    }
}
