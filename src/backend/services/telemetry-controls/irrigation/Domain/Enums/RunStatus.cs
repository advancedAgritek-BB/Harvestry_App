namespace Harvestry.Irrigation.Domain.Enums;

/// <summary>
/// Status of an irrigation run
/// </summary>
public enum RunStatus
{
    /// <summary>
    /// Run is queued and waiting to start
    /// </summary>
    Queued = 1,

    /// <summary>
    /// Run is currently executing
    /// </summary>
    Running = 2,

    /// <summary>
    /// Run completed successfully
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Run was manually aborted
    /// </summary>
    Aborted = 4,

    /// <summary>
    /// Run was stopped due to interlock trip
    /// </summary>
    InterlockTripped = 5,

    /// <summary>
    /// Run encountered an unrecoverable error
    /// </summary>
    Faulted = 6,

    /// <summary>
    /// Run is paused (can be resumed)
    /// </summary>
    Paused = 7
}
