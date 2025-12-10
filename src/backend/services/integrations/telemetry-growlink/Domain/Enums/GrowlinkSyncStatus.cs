namespace Harvestry.Integration.Growlink.Domain.Enums;

/// <summary>
/// Status of a Growlink sync operation.
/// </summary>
public enum GrowlinkSyncStatus
{
    /// <summary>
    /// Sync completed successfully.
    /// </summary>
    Success = 0,

    /// <summary>
    /// Sync completed with partial data.
    /// </summary>
    PartialSuccess = 1,

    /// <summary>
    /// Sync failed due to authentication error.
    /// </summary>
    AuthenticationError = 2,

    /// <summary>
    /// Sync failed due to rate limiting.
    /// </summary>
    RateLimited = 3,

    /// <summary>
    /// Sync failed due to network error.
    /// </summary>
    NetworkError = 4,

    /// <summary>
    /// Sync failed due to API error.
    /// </summary>
    ApiError = 5,

    /// <summary>
    /// Sync skipped (no active mappings).
    /// </summary>
    Skipped = 6
}




