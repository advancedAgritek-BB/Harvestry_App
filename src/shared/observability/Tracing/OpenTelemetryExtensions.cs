using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Harvestry.Shared.Observability.Tracing;

/// <summary>
/// Extension methods for configuring OpenTelemetry instrumentation in Harvestry services.
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Adds OpenTelemetry tracing and metrics to the service collection with Harvestry defaults.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="serviceName">The name of the service (e.g., "Harvestry.Identity").</param>
    /// <param name="serviceVersion">The version of the service.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHarvestryOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        string serviceVersion = "1.0.0")
    {
        var otelConfig = configuration.GetSection("OpenTelemetry");
        var endpoint = otelConfig["Endpoint"] ?? "http://localhost:4317";
        var environment = configuration["Environment"] ?? "development";
        var enableConsoleExporter = otelConfig.GetValue<bool>("EnableConsoleExporter", false);

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion,
                    serviceInstanceId: Environment.MachineName)
                .AddAttributes(new[]
                {
                    new KeyValuePair<string, object>("deployment.environment", environment),
                    new KeyValuePair<string, object>("host.name", Environment.MachineName),
                    new KeyValuePair<string, object>("os.type", Environment.OSVersion.Platform.ToString())
                }))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.Filter = httpContext =>
                        {
                            // Exclude health check endpoints from tracing
                            var path = httpContext.Request.Path.Value ?? "";
                            return !path.Contains("/health", StringComparison.OrdinalIgnoreCase) &&
                                   !path.Contains("/healthz", StringComparison.OrdinalIgnoreCase) &&
                                   !path.Contains("/ready", StringComparison.OrdinalIgnoreCase) &&
                                   !path.Contains("/live", StringComparison.OrdinalIgnoreCase);
                        };
                        options.EnrichWithHttpRequest = (activity, request) =>
                        {
                            activity.SetTag("http.request.header.x-request-id", 
                                request.Headers["X-Request-Id"].FirstOrDefault());
                        };
                        options.EnrichWithHttpResponse = (activity, response) =>
                        {
                            activity.SetTag("http.response.status_code", response.StatusCode);
                        };
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.FilterHttpRequestMessage = request =>
                        {
                            // Exclude internal health checks
                            var uri = request.RequestUri?.ToString() ?? "";
                            return !uri.Contains("/health", StringComparison.OrdinalIgnoreCase);
                        };
                    })
                    .AddSource("Npgsql")
                    .AddSource("Harvestry.*")
                    .AddSource(ActivitySources.IdentityServiceName)
                    .AddSource(ActivitySources.TelemetryServiceName)
                    .AddSource(ActivitySources.GeneticsServiceName)
                    .AddSource(ActivitySources.SpatialServiceName)
                    .AddSource(ActivitySources.TasksServiceName);

                // Add OTLP exporter
                tracing.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(endpoint);
                });

                // Optionally add console exporter for development
                if (enableConsoleExporter)
                {
                    tracing.AddConsoleExporter();
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter("Harvestry.*")
                    .AddMeter(HarvestryMetrics.MeterName);

                // Add OTLP exporter
                metrics.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(endpoint);
                });

                // Optionally add console exporter for development
                if (enableConsoleExporter)
                {
                    metrics.AddConsoleExporter();
                }
            });

        // Register metrics service
        services.AddSingleton<HarvestryMetrics>();

        return services;
    }

    /// <summary>
    /// Adds OpenTelemetry with minimal configuration for testing.
    /// </summary>
    public static IServiceCollection AddHarvestryOpenTelemetryForTesting(
        this IServiceCollection services,
        string serviceName)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddSource("Harvestry.*"))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddMeter("Harvestry.*"));

        services.AddSingleton<HarvestryMetrics>();

        return services;
    }
}

