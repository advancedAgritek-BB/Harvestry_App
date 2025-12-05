namespace Harvestry.Shared.Authentication;

/// <summary>
/// Configuration settings for Supabase authentication.
/// </summary>
public sealed class SupabaseSettings
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Supabase";
    
    /// <summary>
    /// The Supabase project URL (e.g., https://your-project.supabase.co).
    /// </summary>
    public string Url { get; set; } = string.Empty;
    
    /// <summary>
    /// The JWT secret used to validate Supabase tokens.
    /// Found in Supabase Dashboard > Settings > API > JWT Secret.
    /// </summary>
    public string JwtSecret { get; set; } = string.Empty;
    
    /// <summary>
    /// The webhook secret for validating Supabase webhook requests.
    /// </summary>
    public string WebhookSecret { get; set; } = string.Empty;
    
    /// <summary>
    /// The issuer URL for JWT validation.
    /// Defaults to {Url}/auth/v1.
    /// </summary>
    public string GetIssuer() => $"{Url.TrimEnd('/')}/auth/v1";
    
    /// <summary>
    /// The expected audience for JWT tokens.
    /// </summary>
    public string Audience { get; set; } = "authenticated";
    
    /// <summary>
    /// Clock skew tolerance for token validation in seconds.
    /// </summary>
    public int ClockSkewSeconds { get; set; } = 30;
}



