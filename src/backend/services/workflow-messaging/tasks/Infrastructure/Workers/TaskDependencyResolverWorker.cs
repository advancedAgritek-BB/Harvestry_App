using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Domain.Enums;
using Harvestry.Tasks.Domain.ValueObjects;
using Harvestry.Tasks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Harvestry.Tasks.Infrastructure.Workers;

public sealed class TaskDependencyResolverWorker : BackgroundService
{
    private const int BatchSize = 50;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(45);
    private static readonly Guid SystemUserId = Guid.Parse("00000000-0000-0000-0000-0000000000F2");

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TaskDependencyResolverWorker> _logger;

    public TaskDependencyResolverWorker(IServiceScopeFactory scopeFactory, ILogger<TaskDependencyResolverWorker> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Task dependency resolver worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ResolveDependenciesAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Host is shutting down; exit loop gracefully.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Task dependency resolver encountered an error");
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Task dependency resolver worker stopped");
    }

    internal async Task ResolveDependenciesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
        var slackNotificationService = scope.ServiceProvider.GetRequiredService<ISlackNotificationService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TasksDbContext>();

        var blockedTasks = await taskRepository
            .GetBlockedWithDependenciesAsync(BatchSize, cancellationToken)
            .ConfigureAwait(false);

        if (blockedTasks.Count == 0)
        {
            return;
        }

        foreach (var task in blockedTasks)
        {
            var dependencyIds = task.Dependencies
                .Select(d => d.DependsOnTaskId)
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToArray();

            if (dependencyIds.Length == 0)
            {
                continue;
            }

            var dependencyTasks = await taskRepository
                .GetByIdsAsync(task.SiteId, dependencyIds, cancellationToken)
                .ConfigureAwait(false);

            if (dependencyTasks.Count == 0)
            {
                _logger.LogWarning(
                    "Blocked task {TaskId} on site {SiteId} has missing dependency records; skipping auto-unblock.",
                    task.Id,
                    task.SiteId);
                continue;
            }

            TaskDependencyResult dependencyResult;
            try
            {
                dependencyResult = task.CheckDependencies(dependencyTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to evaluate dependencies for task {TaskId} on site {SiteId}",
                    task.Id,
                    task.SiteId);
                continue;
            }

            if (!dependencyResult.IsSatisfied)
            {
                continue;
            }

            task.Unblock(SystemUserId);

            await using var transaction = await TryBeginTransactionAsync(dbContext, cancellationToken).ConfigureAwait(false);
            try
            {
                await taskRepository.UpdateAsync(task, cancellationToken).ConfigureAwait(false);
                await taskRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                if (transaction is not null)
                {
                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                if (transaction is not null)
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                }
                _logger.LogError(ex, "Failed to persist unblocked task {TaskId} on site {SiteId}", task.Id, task.SiteId);
            }

            var payload = new
            {
                taskId = task.Id,
                siteId = task.SiteId,
                title = task.Title,
                status = task.Status.ToString(),
                message = "Task dependencies satisfied; task ready to start.",
                updatedAt = task.UpdatedAt
            };

            await slackNotificationService
                .SendNotificationAsync(task.SiteId, NotificationType.TaskAssigned, payload, priority: 6, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private static async ValueTask<IDbContextTransaction?> TryBeginTransactionAsync(TasksDbContext dbContext, CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (NotSupportedException)
        {
            return null;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Transactions are not supported", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
    }
}
