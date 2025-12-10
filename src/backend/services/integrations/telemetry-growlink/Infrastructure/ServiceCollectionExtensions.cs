using Harvestry.Integration.Growlink.Application.Interfaces;
using Harvestry.Integration.Growlink.Application.Services;
using Harvestry.Integration.Growlink.Infrastructure.External;
using Harvestry.Integration.Growlink.Infrastructure.Persistence;
using Harvestry.Integration.Growlink.Infrastructure.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace Harvestry.Integration.Growlink.Infrastructure;

/// <summary>
/// Extension methods for registering Growlink integration services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Growlink integration services to the service collection.
    /// </summary>
    public static IServiceCollection AddGrowlinkIntegration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuration
        services.Configure<GrowlinkApiConfiguration>(
            configuration.GetSection(GrowlinkApiConfiguration.SectionName));

        // HTTP Client with retry and circuit breaker policies
        services.AddHttpClient<IGrowlinkApiClient, GrowlinkHttpClient>(client =>
        {
            var config = configuration
                .GetSection(GrowlinkApiConfiguration.SectionName)
                .Get<GrowlinkApiConfiguration>() ?? new GrowlinkApiConfiguration();

            client.BaseAddress = new Uri(config.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        // Repositories
        services.AddScoped<IGrowlinkCredentialRepository, GrowlinkCredentialRepository>();
        services.AddScoped<IGrowlinkStreamMappingRepository, GrowlinkStreamMappingRepository>();

        // Application Services
        services.AddScoped<IGrowlinkStreamMapper, GrowlinkStreamMapper>();
        services.AddScoped<IGrowlinkSyncService, GrowlinkSyncService>();

        // Background Worker
        services.AddHostedService<GrowlinkSyncWorker>();

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }
}




