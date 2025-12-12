using Harvestry.AiModels.Application.Interfaces;
using Harvestry.AiModels.Application.Services;
using Harvestry.AiModels.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Harvestry.AiModels.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for registering Graph ML services with dependency injection.
/// </summary>
public static class GraphMlServiceRegistration
{
    /// <summary>
    /// Add Graph ML services to the service collection
    /// </summary>
    public static IServiceCollection AddGraphMlServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<GraphDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("GraphDb") 
                ?? configuration.GetConnectionString("DefaultConnection");

            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "ml");
            });
        });

        // Register repositories
        services.AddScoped<IGraphRepository, GraphRepository>();

        // Register graph builders
        services.AddScoped<IPackageGraphBuilder, PackageGraphBuilder>();
        services.AddScoped<ITaskGraphBuilder, TaskGraphBuilder>();
        services.AddScoped<ITelemetryGraphBuilder, TelemetryGraphBuilder>();
        services.AddScoped<IGeneticsGraphBuilder, GeneticsGraphBuilder>();
        services.AddScoped<IGraphSnapshotBuilder, GraphSnapshotBuilder>();

        // Register anomaly detection services
        services.AddScoped<IMovementAnomalyDetector, MovementAnomalyDetector>();
        services.AddScoped<IIrrigationAnomalyDetector, IrrigationAnomalyDetector>();
        services.AddScoped<IAnomalyDetectionService, AnomalyDetectionService>();

        // Register task prediction service
        services.AddScoped<ITaskPredictionService, TaskPredictionService>();

        // Configure snapshot options
        services.Configure<GraphSnapshotOptions>(
            configuration.GetSection(GraphSnapshotOptions.SectionName));

        return services;
    }

    /// <summary>
    /// Add Graph ML background services
    /// </summary>
    public static IServiceCollection AddGraphMlBackgroundServices(
        this IServiceCollection services)
    {
        services.AddHostedService<GraphSnapshotBackgroundService>();
        return services;
    }
}
