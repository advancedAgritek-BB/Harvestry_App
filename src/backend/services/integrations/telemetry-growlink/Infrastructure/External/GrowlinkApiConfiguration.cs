namespace Harvestry.Integration.Growlink.Infrastructure.External;

/// <summary>
/// Configuration options for the Growlink API client.
/// </summary>
public sealed class GrowlinkApiConfiguration
{
    public const string SectionName = "Growlink";

    /// <summary>
    /// Base URL for the Growlink API.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.growby.net";

    /// <summary>
    /// OAuth authorization endpoint.
    /// </summary>
    public string AuthorizationEndpoint { get; set; } = "https://api.growby.net/oauth/authorize";

    /// <summary>
    /// OAuth token endpoint.
    /// </summary>
    public string TokenEndpoint { get; set; } = "https://api.growby.net/oauth/token";

    /// <summary>
    /// OAuth client ID for Harvestry.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// OAuth client secret for Harvestry.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// OAuth redirect URI after authorization.
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;

    /// <summary>
    /// Request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum retry attempts for transient failures.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Rate limit: maximum requests per minute.
    /// </summary>
    public int RateLimitPerMinute { get; set; } = 60;

    /// <summary>
    /// Buffer time before token expiry to trigger refresh.
    /// </summary>
    public int TokenRefreshBufferMinutes { get; set; } = 5;

    /// <summary>
    /// Sync interval in seconds.
    /// </summary>
    public int SyncIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Enable detailed request/response logging.
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;
}
