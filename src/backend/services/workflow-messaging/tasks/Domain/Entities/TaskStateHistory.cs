using System;
using TaskStatusEnum = Harvestry.Tasks.Domain.Enums.TaskStatus;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Tasks.Domain.Entities;

/// <summary>
/// Represents a state transition in the lifecycle of a task.
/// </summary>
public sealed class TaskStateHistory : Entity<Guid>
{
    private TaskStateHistory(
        Guid id,
        Guid taskId,
        TaskStatusEnum fromStatus,
        TaskStatusEnum toStatus,
        Guid changedBy,
        DateTimeOffset changedAt,
        string? reason) : base(id)
    {
        if (taskId == Guid.Empty)
            throw new ArgumentException("Task identifier is required.", nameof(taskId));
        if (changedBy == Guid.Empty)
            throw new ArgumentException("ChangedBy identifier is required.", nameof(changedBy));

        TaskId = taskId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        ChangedBy = changedBy;
        ChangedAt = changedAt;
        Reason = reason;
    }

    public Guid TaskId { get; }
    public TaskStatusEnum FromStatus { get; }
    public TaskStatusEnum ToStatus { get; }
    public Guid ChangedBy { get; }
    public DateTimeOffset ChangedAt { get; }
    public string? Reason { get; }

    public static TaskStateHistory Create(
        Guid taskId,
        TaskStatusEnum fromStatus,
        TaskStatusEnum toStatus,
        Guid changedBy,
        string? reason = null,
        DateTimeOffset? changedAt = null)
    {
        return new TaskStateHistory(
            Guid.NewGuid(),
            taskId,
            fromStatus,
            toStatus,
            changedBy,
            changedAt ?? DateTimeOffset.UtcNow,
            reason);
    }

    public static TaskStateHistory FromPersistence(
        Guid id,
        Guid taskId,
        TaskStatusEnum fromStatus,
        TaskStatusEnum toStatus,
        Guid changedBy,
        DateTimeOffset changedAt,
        string? reason)
    {
        return new TaskStateHistory(id, taskId, fromStatus, toStatus, changedBy, changedAt, reason);
    }
}
