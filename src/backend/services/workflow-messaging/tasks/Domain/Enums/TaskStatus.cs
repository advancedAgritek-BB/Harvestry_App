namespace Harvestry.Tasks.Domain.Enums;

/// <summary>
/// Tracks the lifecycle state of a task.
/// </summary>
public enum TaskStatus
{
    Undefined = 0,
    Draft,
    Pending,
    Blocked,
    InProgress,
    Completed,
    Cancelled
}
