using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.DTOs;
using TaskPriorityEnum = Harvestry.Tasks.Domain.Enums.TaskPriority;
using TaskStatusEnum = Harvestry.Tasks.Domain.Enums.TaskStatus;

namespace Harvestry.Tasks.Application.Interfaces;

public interface ITaskLifecycleService
{
    Task<TaskResponse> CreateTaskAsync(Guid siteId, CreateTaskRequest request, Guid userId, CancellationToken cancellationToken);
    Task<TaskResponse?> GetTaskByIdAsync(Guid siteId, Guid taskId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TaskResponse>> GetTasksBySiteAsync(Guid siteId, TaskStatusEnum? statusFilter, Guid? assignedToUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TaskResponse>> GetOverdueTasksAsync(Guid siteId, CancellationToken cancellationToken);
    Task<TaskResponse> AssignTaskAsync(Guid siteId, Guid taskId, AssignTaskRequest request, Guid userId, CancellationToken cancellationToken);
    Task<TaskWithGatingResponse> StartTaskAsync(Guid siteId, Guid taskId, Guid userId, CancellationToken cancellationToken);
    Task<TaskResponse> CompleteTaskAsync(Guid siteId, Guid taskId, Guid userId, CancellationToken cancellationToken);
    Task<TaskResponse> CancelTaskAsync(Guid siteId, Guid taskId, CancelTaskRequest request, Guid userId, CancellationToken cancellationToken);
    Task<TaskResponse> UpdateTaskAsync(Guid siteId, Guid taskId, UpdateTaskRequest request, Guid userId, CancellationToken cancellationToken);
    Task<TaskResponse> UpdatePriorityAsync(Guid siteId, Guid taskId, TaskPriorityEnum priority, Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TaskStateHistoryResponse>> GetTaskHistoryAsync(Guid siteId, Guid taskId, CancellationToken cancellationToken);
    Task<TaskResponse> AddWatcherAsync(Guid siteId, Guid taskId, Guid userId, CancellationToken cancellationToken);
    Task<TaskResponse> RemoveWatcherAsync(Guid siteId, Guid taskId, Guid userId, CancellationToken cancellationToken);
}
