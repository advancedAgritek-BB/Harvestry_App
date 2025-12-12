namespace Harvestry.Integration.Growlink.Domain.Enums;

/// <summary>
/// Status of the Growlink integration connection.
/// </summary>
public enum GrowlinkConnectionStatus
{
    /// <summary>
    /// Integration not set up.
    /// </summary>
    NotConnected = 0,

    /// <summary>
    /// OAuth in progress.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Successfully connected and syncing.
    /// </summary>
    Connected = 2,

    /// <summary>
    /// Access token expired, needs refresh.
    /// </summary>
    TokenExpired = 3,

    /// <summary>
    /// Multiple consecutive sync failures.
    /// </summary>
    Error = 4,

    /// <summary>
    /// User disconnected the integration.
    /// </summary>
    Disconnected = 5
}





