using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Harvestry.Tasks.Infrastructure.Workers;

public sealed class TaskOverdueMonitorWorker : BackgroundService
{
    private const int BatchSize = 50;
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TaskOverdueMonitorWorker> _logger;

    public TaskOverdueMonitorWorker(IServiceScopeFactory scopeFactory, ILogger<TaskOverdueMonitorWorker> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Task overdue monitor worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOverdueTasksAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Swallow cancellation triggered by host shutdown.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Task overdue monitor failed");
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

        _logger.LogInformation("Task overdue monitor worker stopped");
    }

    private async Task ProcessOverdueTasksAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
        var slackNotificationService = scope.ServiceProvider.GetRequiredService<ISlackNotificationService>();

        var referenceTime = DateTimeOffset.UtcNow;
        var overdueTasks = await taskRepository
            .GetOverdueAsync(referenceTime, BatchSize, cancellationToken)
            .ConfigureAwait(false);

        if (overdueTasks.Count == 0)
        {
            return;
        }

        foreach (var task in overdueTasks)
        {
            if (!task.IsOverdue() || task.DueDate is null)
            {
                continue;
            }

            var requestId = $"task-overdue:{task.Id:N}:{task.DueDate.Value.UtcDateTime:yyyyMMddHHmmss}";
            var payload = new
            {
                taskId = task.Id,
                siteId = task.SiteId,
                title = task.Title,
                status = task.Status.ToString(),
                priority = task.Priority.ToString(),
                dueDate = task.DueDate.Value,
                assignedToUserId = task.AssignedToUserId,
                blockingReason = task.BlockingReason
            };

            var result = await slackNotificationService
                .SendNotificationAsync(task.SiteId, NotificationType.TaskOverdue, payload, priority: 9, cancellationToken, requestId)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(result))
            {
                _logger.LogDebug(
                    "Skipped overdue notification for task {TaskId} on site {SiteId}; mapping unavailable or already queued.",
                    task.Id,
                    task.SiteId);
            }
        }
    }
}
