namespace Harvestry.Irrigation.Domain.Enums;

/// <summary>
/// Status of a device command in the outbox queue
/// </summary>
public enum CommandStatus
{
    /// <summary>
    /// Command is pending dispatch
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Command has been sent to device
    /// </summary>
    Sent = 2,

    /// <summary>
    /// Command was acknowledged by device
    /// </summary>
    Acknowledged = 3,

    /// <summary>
    /// Command execution confirmed complete
    /// </summary>
    Completed = 4,

    /// <summary>
    /// Command failed (will retry)
    /// </summary>
    Failed = 5,

    /// <summary>
    /// Command permanently failed (max retries exceeded)
    /// </summary>
    FailedPermanent = 6,

    /// <summary>
    /// Command was cancelled before execution
    /// </summary>
    Cancelled = 7,

    /// <summary>
    /// Command timed out waiting for acknowledgment
    /// </summary>
    TimedOut = 8
}

/// <summary>
/// Priority levels for device commands
/// </summary>
public enum CommandPriority
{
    /// <summary>
    /// Low priority (deferred processing ok)
    /// </summary>
    Low = 1,

    /// <summary>
    /// Normal priority
    /// </summary>
    Normal = 2,

    /// <summary>
    /// High priority (process before normal)
    /// </summary>
    High = 3,

    /// <summary>
    /// Emergency (immediate processing, skip queue)
    /// </summary>
    Emergency = 4
}

/// <summary>
/// Types of device commands
/// </summary>
public enum CommandType
{
    /// <summary>
    /// Open a valve
    /// </summary>
    ValveOpen = 1,

    /// <summary>
    /// Close a valve
    /// </summary>
    ValveClose = 2,

    /// <summary>
    /// Start a pump
    /// </summary>
    PumpStart = 3,

    /// <summary>
    /// Stop a pump
    /// </summary>
    PumpStop = 4,

    /// <summary>
    /// Start nutrient injector
    /// </summary>
    InjectorStart = 5,

    /// <summary>
    /// Stop nutrient injector
    /// </summary>
    InjectorStop = 6,

    /// <summary>
    /// Emergency close all valves
    /// </summary>
    EmergencyCloseAll = 7,

    /// <summary>
    /// Request device status
    /// </summary>
    StatusRequest = 8
}
