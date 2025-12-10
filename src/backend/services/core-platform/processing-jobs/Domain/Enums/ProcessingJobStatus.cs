namespace Harvestry.ProcessingJobs.Domain.Enums;

/// <summary>
/// Status of a processing job
/// </summary>
public enum ProcessingJobStatus
{
    /// <summary>
    /// Job is actively being worked on
    /// </summary>
    Active = 0,

    /// <summary>
    /// Job is on hold
    /// </summary>
    OnHold = 1,

    /// <summary>
    /// Job is finished/completed
    /// </summary>
    Finished = 2,

    /// <summary>
    /// Job was cancelled
    /// </summary>
    Cancelled = 3
}








