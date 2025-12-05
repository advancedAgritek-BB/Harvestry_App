using System;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Application.Services;
using Harvestry.Tasks.Infrastructure.Authorization;
using Harvestry.Tasks.Infrastructure.External.Slack;
using Harvestry.Tasks.Infrastructure.Persistence;
using Harvestry.Tasks.Infrastructure.Readiness;
using Harvestry.Tasks.Infrastructure.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Harvestry.Tasks.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTasksInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TasksDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("Tasks")
                ?? configuration["WorkflowMessaging:Tasks:ConnectionString"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Tasks connection string was not provided.");
            }

            options.UseNpgsql(connectionString, builder =>
            {
                builder.MigrationsHistoryTable("__ef_migrations_history", "tasks");
            });
        });

        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<ITaskBlueprintRepository, TaskBlueprintRepository>();
        services.AddScoped<ISopRepository, SopRepository>();
        services.AddScoped<ITaskLibraryRepository, TaskLibraryRepository>();
        services.AddScoped<IUserNotificationRepository, UserNotificationRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<ISlackWorkspaceRepository, SlackWorkspaceRepository>();
        services.AddScoped<ISlackChannelMappingRepository, SlackChannelMappingRepository>();
        services.AddScoped<ISlackNotificationQueueRepository, SlackNotificationQueueRepository>();
        services.AddScoped<ISlackMessageBridgeLogRepository, SlackMessageBridgeLogRepository>();
        services.AddScoped<ITaskGatingResolverService, TaskGatingResolverService>();
        services.AddScoped<ITaskLifecycleService, TaskLifecycleService>();
        services.AddScoped<ITaskBlueprintService, TaskBlueprintService>();
        services.AddScoped<ISopService, SopService>();
        services.AddScoped<ITaskLibraryService, TaskLibraryService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ITaskGenerationService, TaskGenerationService>();
        services.AddScoped<IConversationService, ConversationService>();
        services.AddScoped<ISlackConfigurationService, SlackConfigurationService>();
        services.AddScoped<ISlackNotificationService, SlackNotificationService>();
        services.AddScoped<ISlackWebhookHandler, SlackWebhookHandler>();
        services.AddHttpClient<ISlackApiClient, SlackApiClient>(client =>
        {
            client.BaseAddress = new Uri("https://slack.com/api/");
        });

        services.AddSingleton(sp =>
        {
            var configurationRoot = sp.GetRequiredService<IConfiguration>();
            var connectionString = configurationRoot.GetConnectionString("Identity")
                ?? configurationRoot["Identity:ConnectionString"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Identity connection string was not provided for readiness provider.");
            }

            return NpgsqlDataSource.Create(connectionString);
        });

        services.AddScoped<IUserReadinessProvider, UserReadinessProvider>();
        services.AddScoped<ISiteAuthorizationService, SiteAuthorizationService>();
        services.AddHostedService<SlackNotificationWorker>();
        services.AddHostedService<TaskOverdueMonitorWorker>();
        services.AddHostedService<TaskDependencyResolverWorker>();

        return services;
    }
}
