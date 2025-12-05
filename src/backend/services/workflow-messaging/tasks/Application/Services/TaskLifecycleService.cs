using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Application.Mappers;
using Harvestry.Tasks.Domain.Enums;
using Microsoft.Extensions.Logging;
using DomainTask = Harvestry.Tasks.Domain.Entities.Task;
using TaskPriorityEnum = Harvestry.Tasks.Domain.Enums.TaskPriority;
using TaskStatusEnum = Harvestry.Tasks.Domain.Enums.TaskStatus;

namespace Harvestry.Tasks.Application.Services;

public sealed class TaskLifecycleService : ITaskLifecycleService
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITaskGatingResolverService _gatingResolver;
    private readonly ISlackNotificationService _slackNotificationService;
    private readonly ISiteAuthorizationService _siteAuthorizationService;
    private readonly ILogger<TaskLifecycleService> _logger;

    public TaskLifecycleService(
        ITaskRepository taskRepository,
        ITaskGatingResolverService gatingResolver,
        ISlackNotificationService slackNotificationService,
        ISiteAuthorizationService siteAuthorizationService,
        ILogger<TaskLifecycleService> logger)
    {
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
        _gatingResolver = gatingResolver ?? throw new ArgumentNullException(nameof(gatingResolver));
        _slackNotificationService = slackNotificationService ?? throw new ArgumentNullException(nameof(slackNotificationService));
        _siteAuthorizationService = siteAuthorizationService ?? throw new ArgumentNullException(nameof(siteAuthorizationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TaskResponse> CreateTaskAsync(Guid siteId, CreateTaskRequest request, Guid userId, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var task = DomainTask.Create(
            siteId,
            request.TaskType,
            request.CustomTaskType,
            request.Title,
            request.Description,
            createdByUserId: userId,
            assignedByUserId: userId,
            priority: request.Priority,
            requiredSopIds: request.RequiredSopIds ?? Array.Empty<Guid>(),
            requiredTrainingIds: request.RequiredTrainingIds ?? Array.Empty<Guid>(),
            relatedEntityType: request.RelatedEntityType,
            relatedEntityId: request.RelatedEntityId);

        if (request.AssignedToUserId.HasValue || !string.IsNullOrWhiteSpace(request.AssignedToRole))
        {
            task.Assign(request.AssignedToUserId, request.AssignedToRole, userId);
        }

        if (request.DueDate.HasValue)
        {
            task.UpdateDueDate(request.DueDate, userId);
        }

        await _taskRepository.AddAsync(task, cancellationToken).ConfigureAwait(false);
        await _taskRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await NotifyAsync(siteId, NotificationType.TaskCreated, new
        {
            TaskId = task.Id,
            task.Title,
            task.Priority,
            task.DueDate,
            task.AssignedToUserId
        }, 5, cancellationToken).ConfigureAwait(false);

        return TaskMapper.ToResponse(task);
    }

    public async Task<TaskResponse?> GetTaskByIdAsync(Guid siteId, Guid taskId, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(siteId, taskId, cancellationToken).ConfigureAwait(false);
        return task is null ? null : TaskMapper.ToResponse(task);
    }

    public async Task<IReadOnlyList<TaskResponse>> GetTasksBySiteAsync(Guid siteId, TaskStatusEnum? statusFilter, Guid? assignedToUserId, CancellationToken cancellationToken)
    {
        var tasks = await _taskRepository
            .GetBySiteAsync(siteId, statusFilter, assignedToUserId, cancellationToken)
            .ConfigureAwait(false);

        return tasks.Select(TaskMapper.ToResponse).ToArray();
    }

    public async Task<IReadOnlyList<TaskResponse>> GetOverdueTasksAsync(Guid siteId, CancellationToken cancellationToken)
    {
        var tasks = await _taskRepository
            .GetOverdueAsync(siteId, DateTimeOffset.UtcNow, cancellationToken)
            .ConfigureAwait(false);

        return tasks.Select(TaskMapper.ToResponse).ToArray();
    }

    public async Task<TaskResponse> AssignTaskAsync(Guid siteId, Guid taskId, AssignTaskRequest request, Guid userId, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var task = await EnsureTaskAsync(siteId, taskId, cancellationToken).ConfigureAwait(false);

        // Validate cross-site assignment if assigning to a specific user
        if (request.UserId.HasValue && request.UserId.Value != Guid.Empty)
        {
            var validationResult = await _siteAuthorizationService
                .ValidateCrossSiteAssignmentAsync(userId, request.UserId.Value, siteId, cancellationToken)
                .ConfigureAwait(false);

            if (!validationResult.IsAllowed)
            {
                throw new InvalidOperationException(
                    $"Task assignment denied: {validationResult.FailureReason}");
            }

            if (validationResult.IsCrossSite)
            {
                _logger.LogInformation(
                    "Cross-site task assignment: Assigner={AssignerId}, Assignee={AssigneeId}, Task={TaskId}, Site={SiteId}",
                    userId, request.UserId.Value, taskId, siteId);
            }
        }

        task.Assign(request.UserId, request.Role, userId, request.AssignedAt);

        await _taskRepository.UpdateAsync(task, cancellationToken).ConfigureAwait(false);
        await _taskRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await NotifyAsync(siteId, NotificationType.TaskAssigned, new
        {
            TaskId = task.Id,
            task.Title,
            task.Priority,
            task.AssignedToUserId,
            request.Role
        }, 6, cancellationToken).ConfigureAwait(false);

        return TaskMapper.ToResponse(task);
    }

    public async Task<TaskWithGatingResponse> StartTaskAsync(Guid siteId, Guid taskId, Guid userId, CancellationToken cancellationToken)
    {
        var task = await EnsureTaskAsync(siteId, taskId, cancellationToken).ConfigureAwait(false);

        var gating = await _gatingResolver.EvaluateAsync(task, userId, cancellationToken).ConfigureAwait(false);
        if (gating.IsGated)
        {
            _logger.LogInformation("Task {TaskId} gated for user {UserId}: {Reasons}", taskId, userId, string.Join(", ", gating.Reasons));
            return TaskMapper.ToResponse(task, gating);
        }

        var dependencyTasks = await LoadDependenciesAsync(task, cancellationToken).ConfigureAwait(false);
        var dependencyResult = task.CheckDependencies(dependencyTasks);

        if (!dependencyResult.IsSatisfied)
        {
            var reason = dependencyResult.Reasons.FirstOrDefault() ?? "Dependencies not satisfied";
            task.Block(reason, userId);

            await _taskRepository.UpdateAsync(task, cancellationToken).ConfigureAwait(false);
            await _taskRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await NotifyAsync(task.SiteId, NotificationType.TaskBlocked, new
            {
                TaskId = task.Id,
                task.Title,
                Reasons = dependencyResult.Reasons,
                BlockingTaskIds = dependencyResult.BlockingTaskIds
            }, 8, cancellationToken).ConfigureAwait(false);

            var dependencyGating = new TaskGatingStatusResponse
            {
                IsGated = true,
                MissingSopIds = Array.Empty<Guid>(),
                MissingTrainingIds = Array.Empty<Guid>(),
                Reasons = dependencyResult.Reasons
            };

            return TaskMapper.ToResponse(task, dependencyGating);
        }

        task.Unblock(userId);
        task.Start(userId);

        await _taskRepository.UpdateAsync(task, cancellationToken).ConfigureAwait(false);
        await _taskRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await NotifyAsync(siteId, NotificationType.TaskStarted, new
        {
            TaskId = task.Id,
            task.Title,
            task.AssignedToUserId,
            StartedAt = task.StartedAt
        }, 5, cancellationToken).ConfigureAwait(false);

        var successGating = new TaskGatingStatusResponse
        {
            IsGated = false,
            MissingSopIds = Array.Empty<Guid>(),
            MissingTrainingIds = Array.Empty<Guid>(),
            Reasons = Array.Empty<string>()
        };

        return TaskMapper.ToResponse(task, successGating);
    }

    public async Task<TaskResponse> CompleteTaskAsync(Guid siteId, Guid taskId, Guid userId, CancellationToken cancellationToken)
    {
        var task = await EnsureTaskAsync(siteId, taskId, cancellationToken).ConfigureAwait(false);
        task.Complete(userId);

        await _taskRepository.UpdateAsync(task, cancellationToken).ConfigureAwait(false);
        await _taskRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await NotifyAsync(siteId, NotificationType.TaskCompleted, new
        {
            TaskId = task.Id,
            task.Title,
            task.CompletedAt
        }, 4, cancellationToken).ConfigureAwait(false);

        return TaskMapper.ToResponse(task);
    }

    public async Task<TaskResponse> CancelTaskAsync(Guid siteId, Guid taskId, CancelTaskRequest request, Guid userId, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var task = await EnsureTaskAsync(siteId, taskId, cancellationToken).ConfigureAwait(false);
        task.Cancel(request.Reason ?? "Cancelled", userId);

        await _taskRepository.UpdateAsync(task, cancellationToken).ConfigureAwait(false);
        await _taskRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return TaskMapper.ToResponse(task);
    }

    public async Task<TaskResponse> UpdateTaskAsync(Guid siteId, Guid taskId, UpdateTaskRequest request, Guid userId, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var task = await EnsureTaskAsync(siteId, taskId, cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            task.UpdateDescription(request.Description, userId);
        }

        if (request.DueDate.HasValue)
        {
            task.UpdateDueDate(request.DueDate, userId);
        }

        if (request.Priority.HasValue)
        {
            task.UpdatePriority(request.Priority.Value, userId);
        }

        if (!string.IsNullOrWhiteSpace(request.RelatedEntityType) || request.RelatedEntityId.HasValue)
        {
            task.SetRelatedEntity(request.RelatedEntityType, request.RelatedEntityId);
        }

        await _taskRepository.UpdateAsync(task, cancellationToken).ConfigureAwait(false);
        await _taskRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return TaskMapper.ToResponse(task);
    }

    public async Task<TaskResponse> UpdatePriorityAsync(Guid siteId, Guid taskId, TaskPriorityEnum priority, Guid userId, CancellationToken cancellationToken)
    {
        var task = await EnsureTaskAsync(siteId, taskId, cancellationToken).ConfigureAwait(false);
        task.UpdatePriority(priority, userId);

        await _taskRepository.UpdateAsync(task, cancellationToken).ConfigureAwait(false);
        await _taskRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return TaskMapper.ToResponse(task);
    }

    public async Task<IReadOnlyList<TaskStateHistoryResponse>> GetTaskHistoryAsync(Guid siteId, Guid taskId, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(siteId, taskId, cancellationToken).ConfigureAwait(false);
        if (task is null)
        {
            return Array.Empty<TaskStateHistoryResponse>();
        }

        return task.StateHistory.Select(h => new TaskStateHistoryResponse
        {
            PreviousStatus = h.FromStatus,
            Status = h.ToStatus,
            ChangedBy = h.ChangedBy,
            ChangedAt = h.ChangedAt,
            Reason = h.Reason
        }).ToArray();
    }

    public async Task<TaskResponse> AddWatcherAsync(Guid siteId, Guid taskId, Guid userId, CancellationToken cancellationToken)
    {
        var task = await EnsureTaskAsync(siteId, taskId, cancellationToken).ConfigureAwait(false);
        task.AddWatcher(userId);

        await _taskRepository.UpdateAsync(task, cancellationToken).ConfigureAwait(false);
        await _taskRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return TaskMapper.ToResponse(task);
    }

    public async Task<TaskResponse> RemoveWatcherAsync(Guid siteId, Guid taskId, Guid userId, CancellationToken cancellationToken)
    {
        var task = await EnsureTaskAsync(siteId, taskId, cancellationToken).ConfigureAwait(false);
        task.RemoveWatcher(userId);

        await _taskRepository.UpdateAsync(task, cancellationToken).ConfigureAwait(false);
        await _taskRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return TaskMapper.ToResponse(task);
    }

    private async Task<DomainTask> EnsureTaskAsync(Guid siteId, Guid taskId, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(siteId, taskId, cancellationToken).ConfigureAwait(false);
        if (task is null)
        {
            throw new KeyNotFoundException($"Task {taskId} not found for site {siteId}.");
        }

        return task;
    }

    private async Task<IReadOnlyList<DomainTask>> LoadDependenciesAsync(DomainTask task, CancellationToken cancellationToken)
    {
        var dependencyIds = task.Dependencies.Select(d => d.DependsOnTaskId).Distinct().ToArray();
        if (dependencyIds.Length == 0)
        {
            return Array.Empty<DomainTask>();
        }

        return await _taskRepository
            .GetByIdsAsync(task.SiteId, dependencyIds, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task NotifyAsync(Guid siteId, NotificationType notificationType, object payload, int priority, CancellationToken cancellationToken)
    {
        try
        {
            await _slackNotificationService
                .SendNotificationAsync(siteId, notificationType, payload, priority, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish Slack notification {NotificationType} for site {SiteId}", notificationType, siteId);
        }
    }
}
