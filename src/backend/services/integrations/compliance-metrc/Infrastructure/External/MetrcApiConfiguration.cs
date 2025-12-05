namespace Harvestry.Compliance.Metrc.Infrastructure.External;

/// <summary>
/// Configuration settings for METRC API connections
/// </summary>
public sealed class MetrcApiConfiguration
{
    public const string SectionName = "MetrcApi";

    /// <summary>
    /// Base URL for the METRC API (state-specific)
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Vendor/Integrator API key (encrypted at rest)
    /// </summary>
    public string VendorApiKey { get; set; } = string.Empty;

    /// <summary>
    /// API version to use (e.g., "v2")
    /// </summary>
    public string ApiVersion { get; set; } = "v2";

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum retry attempts for failed requests
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Initial retry delay in milliseconds
    /// </summary>
    public int RetryDelayMs { get; set; } = 1000;

    /// <summary>
    /// Rate limit: requests per minute
    /// </summary>
    public int RateLimitPerMinute { get; set; } = 60;

    /// <summary>
    /// Enable detailed request/response logging
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;

    /// <summary>
    /// Get base URL for a specific state
    /// </summary>
    public static string GetBaseUrlForState(string stateCode)
    {
        return stateCode?.ToUpperInvariant() switch
        {
            "IL" => "https://api-il.metrc.com/",
            "CO" => "https://api-co.metrc.com/",
            "NY" => "https://api-ny.metrc.com/",
            "CA" => "https://api-ca.metrc.com/",
            "MI" => "https://api-mi.metrc.com/",
            "OR" => "https://api-or.metrc.com/",
            "WA" => "https://api-wa.metrc.com/",
            _ => throw new ArgumentException($"Unsupported state code: {stateCode}", nameof(stateCode))
        };
    }

    /// <summary>
    /// Get sandbox base URL for a specific state
    /// </summary>
    public static string GetSandboxUrlForState(string stateCode)
    {
        return stateCode?.ToUpperInvariant() switch
        {
            "IL" => "https://sandbox-api-il.metrc.com/",
            "CO" => "https://sandbox-api-co.metrc.com/",
            "NY" => "https://sandbox-api-ny.metrc.com/",
            "CA" => "https://sandbox-api-ca.metrc.com/",
            _ => throw new ArgumentException($"Unsupported state code for sandbox: {stateCode}", nameof(stateCode))
        };
    }
}




