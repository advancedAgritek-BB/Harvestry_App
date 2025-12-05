using System;
using Harvestry.Shared.Kernel.Domain;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.Domain.Entities;

/// <summary>
/// Represents a dependency relationship between two tasks.
/// </summary>
public sealed class TaskDependency : Entity<Guid>
{
    private TaskDependency(
        Guid id,
        Guid taskId,
        Guid dependsOnTaskId,
        DependencyType dependencyType,
        bool isBlocking,
        TimeSpan? minimumLag) : base(id)
    {
        if (taskId == Guid.Empty)
            throw new ArgumentException("Task identifier is required.", nameof(taskId));
        if (dependsOnTaskId == Guid.Empty)
            throw new ArgumentException("Dependency task identifier is required.", nameof(dependsOnTaskId));
        if (taskId == dependsOnTaskId)
            throw new ArgumentException("A task cannot depend on itself.", nameof(dependsOnTaskId));

        TaskId = taskId;
        DependsOnTaskId = dependsOnTaskId;
        DependencyType = dependencyType == DependencyType.Undefined
            ? DependencyType.FinishToStart
            : dependencyType;
        IsBlocking = isBlocking;
        MinimumLag = minimumLag;
    }

    public Guid TaskId { get; }
    public Guid DependsOnTaskId { get; }
    public DependencyType DependencyType { get; }
    public bool IsBlocking { get; }
    public TimeSpan? MinimumLag { get; }

    public static TaskDependency Create(
        Guid taskId,
        Guid dependsOnTaskId,
        DependencyType dependencyType = DependencyType.FinishToStart,
        bool isBlocking = true,
        TimeSpan? minimumLag = null)
    {
        return new TaskDependency(
            Guid.NewGuid(),
            taskId,
            dependsOnTaskId,
            dependencyType,
            isBlocking,
            minimumLag);
    }

    public static TaskDependency FromPersistence(
        Guid id,
        Guid taskId,
        Guid dependsOnTaskId,
        DependencyType dependencyType,
        bool isBlocking,
        TimeSpan? minimumLag)
    {
        return new TaskDependency(id, taskId, dependsOnTaskId, dependencyType, isBlocking, minimumLag);
    }
}
