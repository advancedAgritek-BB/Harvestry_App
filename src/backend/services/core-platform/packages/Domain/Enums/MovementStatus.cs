namespace Harvestry.Packages.Domain.Enums;

/// <summary>
/// Status of an inventory movement
/// </summary>
public enum MovementStatus
{
    /// <summary>
    /// Movement is pending execution
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Movement is currently being executed
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Movement completed successfully
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Movement was cancelled
    /// </summary>
    Cancelled = 3,

    /// <summary>
    /// Movement failed during execution
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Movement is pending approval
    /// </summary>
    PendingApproval = 5
}



