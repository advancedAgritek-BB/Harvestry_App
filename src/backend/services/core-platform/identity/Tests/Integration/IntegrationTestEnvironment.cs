using System;
using Microsoft.Extensions.Configuration;

namespace Harvestry.Identity.Tests.Integration;

internal static class IntegrationTestEnvironment
{
    public static bool TryGetConnectionString(IConfiguration? configuration, out string connectionString)
    {
        configuration ??= new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        connectionString = configuration.GetConnectionString("Identity")
            ?? configuration["Identity:ConnectionString"]
            ?? Environment.GetEnvironmentVariable("IDENTITY_DB_CONNECTION")
            ?? string.Empty;

        return !string.IsNullOrWhiteSpace(connectionString);
    }
}
