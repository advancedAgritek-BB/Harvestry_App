using System;
using Microsoft.Extensions.Configuration;

namespace Harvestry.Spatial.Tests.Integration;

internal static class IntegrationTestEnvironment
{
    public static bool TryGetConnectionString(IConfiguration? configuration, out string connectionString)
    {
        configuration ??= new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        connectionString = configuration.GetConnectionString("Spatial")
            ?? configuration["Spatial:ConnectionString"]
            ?? Environment.GetEnvironmentVariable("SPATIAL_DB_CONNECTION")
            ?? string.Empty;

        return !string.IsNullOrWhiteSpace(connectionString) &&
               !string.Equals(connectionString, "REPLACE_ME", StringComparison.OrdinalIgnoreCase);
    }
}
