using System.Text.Json;

namespace Harvestry.AiModels.Domain.ValueObjects;

/// <summary>
/// Properties specific to Task nodes for work graph analysis.
/// </summary>
public sealed record TaskNodeProperties
{
    /// <summary>Task type</summary>
    public string TaskType { get; init; } = string.Empty;

    /// <summary>Custom task type if applicable</summary>
    public string? CustomTaskType { get; init; }

    /// <summary>Task title</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Task status</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Task priority</summary>
    public string Priority { get; init; } = string.Empty;

    /// <summary>Assigned to user ID</summary>
    public Guid? AssignedToUserId { get; init; }

    /// <summary>Assigned to role</summary>
    public string? AssignedToRole { get; init; }

    /// <summary>Due date</summary>
    public DateTime? DueDate { get; init; }

    /// <summary>Started at</summary>
    public DateTime? StartedAt { get; init; }

    /// <summary>Completed at</summary>
    public DateTime? CompletedAt { get; init; }

    /// <summary>Related entity type</summary>
    public string? RelatedEntityType { get; init; }

    /// <summary>Related entity ID</summary>
    public Guid? RelatedEntityId { get; init; }

    /// <summary>Is blocked</summary>
    public bool IsBlocked { get; init; }

    /// <summary>Blocking reason</summary>
    public string? BlockingReason { get; init; }

    /// <summary>Number of dependencies</summary>
    public int DependencyCount { get; init; }

    /// <summary>Number of dependents (tasks depending on this)</summary>
    public int DependentCount { get; init; }

    /// <summary>Total time logged (minutes)</summary>
    public int TotalTimeLoggedMinutes { get; init; }

    /// <summary>Serialize to JSON</summary>
    public string ToJson() => JsonSerializer.Serialize(this);

    /// <summary>Deserialize from JSON</summary>
    public static TaskNodeProperties? FromJson(string? json)
        => string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<TaskNodeProperties>(json);
}

/// <summary>
/// Properties specific to User nodes.
/// </summary>
public sealed record UserNodeProperties
{
    /// <summary>User display name</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>User email</summary>
    public string? Email { get; init; }

    /// <summary>Primary role</summary>
    public string? PrimaryRole { get; init; }

    /// <summary>Is active</summary>
    public bool IsActive { get; init; }

    /// <summary>Team IDs user belongs to</summary>
    public Guid[] TeamIds { get; init; } = Array.Empty<Guid>();

    /// <summary>Badge ID if assigned</summary>
    public string? BadgeId { get; init; }

    /// <summary>Total tasks completed</summary>
    public int TotalTasksCompleted { get; init; }

    /// <summary>Average task completion time (hours)</summary>
    public double? AvgTaskCompletionHours { get; init; }

    /// <summary>Serialize to JSON</summary>
    public string ToJson() => JsonSerializer.Serialize(this);

    /// <summary>Deserialize from JSON</summary>
    public static UserNodeProperties? FromJson(string? json)
        => string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<UserNodeProperties>(json);
}

/// <summary>
/// Properties for task dependency edges.
/// </summary>
public sealed record TaskDependencyEdgeProperties
{
    /// <summary>Dependency type (FinishToStart, etc.)</summary>
    public string DependencyType { get; init; } = string.Empty;

    /// <summary>Is blocking</summary>
    public bool IsBlocking { get; init; }

    /// <summary>Minimum lag time</summary>
    public TimeSpan? MinimumLag { get; init; }

    /// <summary>Serialize to JSON</summary>
    public string ToJson() => JsonSerializer.Serialize(this);

    /// <summary>Deserialize from JSON</summary>
    public static TaskDependencyEdgeProperties? FromJson(string? json)
        => string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<TaskDependencyEdgeProperties>(json);
}
