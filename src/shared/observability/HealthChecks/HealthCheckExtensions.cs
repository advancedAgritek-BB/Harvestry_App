using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Harvestry.Shared.Observability.HealthChecks;

/// <summary>
/// Extension methods for configuring standardized health checks across Harvestry services.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adds standard Harvestry health checks including database connectivity.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    /// <param name="serviceName">The name of the service for identification.</param>
    /// <returns>The health checks builder for further configuration.</returns>
    public static IHealthChecksBuilder AddHarvestryHealthChecks(
        this IServiceCollection services,
        string connectionString,
        string serviceName)
    {
        return services.AddHealthChecks()
            .AddNpgSql(
                connectionString: connectionString,
                name: "database",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "db", "ready", "critical" })
            .AddCheck(
                name: "startup",
                check: () => HealthCheckResult.Healthy($"{serviceName} is running"),
                tags: new[] { "live" });
    }

    /// <summary>
    /// Adds standard Harvestry health checks with custom database health check.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceName">The name of the service for identification.</param>
    /// <returns>The health checks builder for further configuration.</returns>
    public static IHealthChecksBuilder AddHarvestryHealthChecksCore(
        this IServiceCollection services,
        string serviceName)
    {
        return services.AddHealthChecks()
            .AddCheck(
                name: "startup",
                check: () => HealthCheckResult.Healthy($"{serviceName} is running"),
                tags: new[] { "live" });
    }
}

/// <summary>
/// Custom health check for TimescaleDB extension availability.
/// </summary>
public class TimescaleDbHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    public TimescaleDbHealthCheck(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT extversion FROM pg_extension WHERE extname = 'timescaledb'";
            
            var result = await command.ExecuteScalarAsync(cancellationToken);
            
            if (result != null)
            {
                return HealthCheckResult.Healthy($"TimescaleDB extension version: {result}");
            }
            
            return HealthCheckResult.Unhealthy("TimescaleDB extension not installed");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to check TimescaleDB", ex);
        }
    }
}

/// <summary>
/// Custom health check for external service reachability.
/// </summary>
public class ExternalServiceHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly string _serviceName;
    private readonly string _healthEndpoint;

    public ExternalServiceHealthCheck(
        HttpClient httpClient,
        string serviceName,
        string healthEndpoint)
    {
        _httpClient = httpClient;
        _serviceName = serviceName;
        _healthEndpoint = healthEndpoint;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(_healthEndpoint, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy($"{_serviceName} is reachable");
            }
            
            return HealthCheckResult.Degraded(
                $"{_serviceName} returned {response.StatusCode}");
        }
        catch (HttpRequestException ex)
        {
            return HealthCheckResult.Degraded(
                $"{_serviceName} is unreachable: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return HealthCheckResult.Degraded(
                $"{_serviceName} health check timed out");
        }
    }
}

