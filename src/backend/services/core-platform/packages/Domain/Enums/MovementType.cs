namespace Harvestry.Packages.Domain.Enums;

/// <summary>
/// Types of inventory movements for tracking and audit
/// </summary>
public enum MovementType
{
    /// <summary>
    /// Transfer between locations within the facility
    /// </summary>
    Transfer = 0,

    /// <summary>
    /// Receive from vendor or external transfer
    /// </summary>
    Receive = 1,

    /// <summary>
    /// Ship to customer or external transfer
    /// </summary>
    Ship = 2,

    /// <summary>
    /// Customer or vendor return
    /// </summary>
    Return = 3,

    /// <summary>
    /// Quantity adjustment (up or down)
    /// </summary>
    Adjustment = 4,

    /// <summary>
    /// Split package into multiple packages
    /// </summary>
    Split = 5,

    /// <summary>
    /// Merge multiple packages into one
    /// </summary>
    Merge = 6,

    /// <summary>
    /// Consumed as input in processing/production
    /// </summary>
    ProcessInput = 7,

    /// <summary>
    /// Created as output from processing/production
    /// </summary>
    ProcessOutput = 8,

    /// <summary>
    /// Destroyed/disposed
    /// </summary>
    Destruction = 9,

    /// <summary>
    /// Cycle count adjustment
    /// </summary>
    CycleCount = 10,

    /// <summary>
    /// Reserve quantity for order
    /// </summary>
    Reserve = 11,

    /// <summary>
    /// Release reservation
    /// </summary>
    Unreserve = 12
}




