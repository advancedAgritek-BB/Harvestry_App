namespace Harvestry.Compliance.Metrc.Domain.Enums;

/// <summary>
/// Status of a METRC synchronization job or queue item
/// </summary>
public enum SyncStatus
{
    /// <summary>
    /// Item is pending synchronization
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Item is currently being processed
    /// </summary>
    Processing = 2,

    /// <summary>
    /// Synchronization completed successfully
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Synchronization failed and will be retried
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Synchronization failed permanently after max retries
    /// </summary>
    FailedPermanent = 5,

    /// <summary>
    /// Item was cancelled/skipped
    /// </summary>
    Cancelled = 6,

    /// <summary>
    /// Item requires manual intervention
    /// </summary>
    ManualReviewRequired = 7
}
