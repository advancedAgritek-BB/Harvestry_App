using System;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Tasks.Domain.Entities;

/// <summary>
/// Represents a watcher subscription for task activity.
/// </summary>
public sealed class TaskWatcher : Entity<Guid>
{
    private TaskWatcher(Guid id, Guid taskId, Guid userId, DateTimeOffset createdAt) : base(id)
    {
        if (taskId == Guid.Empty)
            throw new ArgumentException("Task identifier is required.", nameof(taskId));
        if (userId == Guid.Empty)
            throw new ArgumentException("User identifier is required.", nameof(userId));

        TaskId = taskId;
        UserId = userId;
        CreatedAt = createdAt;
    }

    public Guid TaskId { get; }
    public Guid UserId { get; }
    public DateTimeOffset CreatedAt { get; }

    public static TaskWatcher Create(Guid taskId, Guid userId)
    {
        return new TaskWatcher(Guid.NewGuid(), taskId, userId, DateTimeOffset.UtcNow);
    }

    public static TaskWatcher FromPersistence(Guid id, Guid taskId, Guid userId, DateTimeOffset createdAt)
    {
        return new TaskWatcher(id, taskId, userId, createdAt);
    }
}
