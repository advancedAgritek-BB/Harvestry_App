using Harvestry.Compliance.Metrc.Application.Interfaces;
using Harvestry.Compliance.Metrc.Application.Services;
using Harvestry.Compliance.Metrc.Domain.JurisdictionRules;
using Harvestry.Compliance.Metrc.Infrastructure.External;
using Harvestry.Compliance.Metrc.Infrastructure.Persistence;
using Harvestry.Compliance.Metrc.Infrastructure.Repositories;
using Harvestry.Compliance.Metrc.Infrastructure.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace Harvestry.Compliance.Metrc.Infrastructure;

/// <summary>
/// Extension methods for registering METRC compliance services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds METRC compliance services to the service collection
    /// </summary>
    public static IServiceCollection AddMetrcCompliance(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuration
        services.Configure<MetrcApiConfiguration>(
            configuration.GetSection(MetrcApiConfiguration.SectionName));
        services.Configure<MetrcSyncWorkerOptions>(
            configuration.GetSection(MetrcSyncWorkerOptions.SectionName));

        // DbContext
        var connectionString = configuration.GetConnectionString("MetrcDb")
            ?? configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<MetrcDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.EnableRetryOnFailure(3);
            });
        });

        // Repositories
        services.AddScoped<IMetrcLicenseRepository, MetrcLicenseRepository>();
        services.AddScoped<IMetrcSyncJobRepository, MetrcSyncJobRepository>();
        services.AddScoped<IMetrcQueueItemRepository, MetrcQueueItemRepository>();
        services.AddScoped<IMetrcSyncCheckpointRepository, MetrcSyncCheckpointRepository>();

        // Application Services
        services.AddScoped<IMetrcSyncService, MetrcSyncService>();
        services.AddScoped<IMetrcLicenseService, MetrcLicenseService>();
        services.AddScoped<IMetrcQueueService, MetrcQueueService>();

        // Infrastructure
        services.AddSingleton<MetrcHttpClientFactory>();
        services.AddSingleton<JurisdictionRulesFactory>();

        // HTTP clients with Polly retry policies
        services.AddHttpClient("Metrc", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(
            3,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
        .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(
            5,
            TimeSpan.FromSeconds(30)));

        // Background worker
        var workerOptions = configuration
            .GetSection(MetrcSyncWorkerOptions.SectionName)
            .Get<MetrcSyncWorkerOptions>() ?? new MetrcSyncWorkerOptions();

        if (workerOptions.Enabled)
        {
            services.AddHostedService<MetrcSyncWorker>();
        }

        return services;
    }

    /// <summary>
    /// Adds METRC compliance services without the background worker (for testing)
    /// </summary>
    public static IServiceCollection AddMetrcComplianceWithoutWorker(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Same as above but skip the hosted service
        services.Configure<MetrcApiConfiguration>(
            configuration.GetSection(MetrcApiConfiguration.SectionName));
        services.Configure<MetrcSyncWorkerOptions>(
            configuration.GetSection(MetrcSyncWorkerOptions.SectionName));

        var connectionString = configuration.GetConnectionString("MetrcDb")
            ?? configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<MetrcDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.EnableRetryOnFailure(3);
            });
        });

        services.AddScoped<IMetrcLicenseRepository, MetrcLicenseRepository>();
        services.AddScoped<IMetrcSyncJobRepository, MetrcSyncJobRepository>();
        services.AddScoped<IMetrcQueueItemRepository, MetrcQueueItemRepository>();
        services.AddScoped<IMetrcSyncCheckpointRepository, MetrcSyncCheckpointRepository>();

        services.AddScoped<IMetrcSyncService, MetrcSyncService>();
        services.AddScoped<IMetrcLicenseService, MetrcLicenseService>();
        services.AddScoped<IMetrcQueueService, MetrcQueueService>();

        services.AddSingleton<MetrcHttpClientFactory>();
        services.AddSingleton<JurisdictionRulesFactory>();

        return services;
    }
}
