using System;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Tasks.Domain.Entities;

/// <summary>
/// Captures time spent by a user working a task.
/// </summary>
public sealed class TaskTimeEntry : Entity<Guid>
{
    private TaskTimeEntry(
        Guid id,
        Guid taskId,
        Guid userId,
        DateTimeOffset startedAt,
        DateTimeOffset? endedAt,
        string? notes) : base(id)
    {
        if (taskId == Guid.Empty)
            throw new ArgumentException("Task identifier is required.", nameof(taskId));
        if (userId == Guid.Empty)
            throw new ArgumentException("User identifier is required.", nameof(userId));

        if (endedAt.HasValue && endedAt.Value < startedAt)
            throw new ArgumentException("End time cannot precede start time.", nameof(endedAt));

        TaskId = taskId;
        UserId = userId;
        StartedAt = startedAt;
        EndedAt = endedAt;
        Notes = notes;
    }

    public Guid TaskId { get; }
    public Guid UserId { get; }
    public DateTimeOffset StartedAt { get; }
    public DateTimeOffset? EndedAt { get; private set; }
    public string? Notes { get; private set; }

    public TimeSpan? Duration => EndedAt.HasValue ? EndedAt.Value - StartedAt : null;

    public void Complete(DateTimeOffset endedAt, string? notes = null)
    {
        if (endedAt < StartedAt)
            throw new ArgumentException("End time cannot precede start time.", nameof(endedAt));

        EndedAt = endedAt;
        if (!string.IsNullOrWhiteSpace(notes))
        {
            Notes = notes;
        }
    }

    public static TaskTimeEntry Create(Guid taskId, Guid userId, DateTimeOffset? startedAt = null, string? notes = null)
    {
        return new TaskTimeEntry(Guid.NewGuid(), taskId, userId, startedAt ?? DateTimeOffset.UtcNow, null, notes);
    }

    public static TaskTimeEntry FromPersistence(
        Guid id,
        Guid taskId,
        Guid userId,
        DateTimeOffset startedAt,
        DateTimeOffset? endedAt,
        string? notes)
    {
        return new TaskTimeEntry(id, taskId, userId, startedAt, endedAt, notes);
    }
}
