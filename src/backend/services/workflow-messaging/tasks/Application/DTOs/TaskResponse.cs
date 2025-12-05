using System;
using System.Collections.Generic;
using TaskDependencyTypeEnum = Harvestry.Tasks.Domain.Enums.DependencyType;
using TaskPriorityEnum = Harvestry.Tasks.Domain.Enums.TaskPriority;
using TaskStatusEnum = Harvestry.Tasks.Domain.Enums.TaskStatus;
using TaskTypeEnum = Harvestry.Tasks.Domain.Enums.TaskType;

namespace Harvestry.Tasks.Application.DTOs;

public sealed class TaskResponse
{
    public Guid TaskId { get; init; }
    public Guid SiteId { get; init; }
    public TaskTypeEnum TaskType { get; init; }
    public string? CustomTaskType { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public TaskStatusEnum Status { get; init; }
    public TaskPriorityEnum Priority { get; init; }
    public Guid CreatedByUserId { get; init; }
    public Guid AssignedByUserId { get; init; }
    public Guid? AssignedToUserId { get; init; }
    public string? AssignedToRole { get; init; }
    public DateTimeOffset? AssignedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public DateTimeOffset? DueDate { get; init; }
    public DateTimeOffset? StartedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public DateTimeOffset? CancelledAt { get; init; }
    public string? CancellationReason { get; init; }
    public string? BlockingReason { get; init; }
    public string? RelatedEntityType { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public IReadOnlyCollection<Guid> RequiredSopIds { get; init; } = Array.Empty<Guid>();
    public IReadOnlyCollection<Guid> RequiredTrainingIds { get; init; } = Array.Empty<Guid>();
    public IReadOnlyCollection<TaskDependencySummary> Dependencies { get; init; } = Array.Empty<TaskDependencySummary>();
    public IReadOnlyCollection<TaskWatcherResponse> Watchers { get; init; } = Array.Empty<TaskWatcherResponse>();
    public IReadOnlyCollection<TaskTimeEntryResponse> TimeEntries { get; init; } = Array.Empty<TaskTimeEntryResponse>();
    public IReadOnlyCollection<TaskStateHistoryResponse> History { get; init; } = Array.Empty<TaskStateHistoryResponse>();

    public sealed class TaskDependencySummary
    {
        public Guid DependsOnTaskId { get; init; }
        public bool IsBlocking { get; init; }
        public TaskDependencyTypeEnum DependencyType { get; init; }
        public TimeSpan? MinimumLag { get; init; }
    }
}
