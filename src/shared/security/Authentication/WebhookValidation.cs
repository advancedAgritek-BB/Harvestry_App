using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Harvestry.Shared.Authentication;

/// <summary>
/// Service for validating Supabase webhook signatures.
/// </summary>
public interface IWebhookSignatureValidator
{
    /// <summary>
    /// Validates a webhook signature against the payload.
    /// </summary>
    /// <param name="payload">The raw webhook payload.</param>
    /// <param name="signature">The signature from the X-Supabase-Signature header.</param>
    /// <returns>True if the signature is valid, false otherwise.</returns>
    bool ValidateSignature(string payload, string signature);
}

/// <summary>
/// Supabase webhook signature validator using HMAC-SHA256.
/// </summary>
public sealed class SupabaseWebhookSignatureValidator : IWebhookSignatureValidator
{
    private readonly byte[] _secretKey;

    public SupabaseWebhookSignatureValidator(IOptions<SupabaseSettings> settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Value.WebhookSecret))
        {
            throw new InvalidOperationException("Supabase webhook secret is not configured.");
        }
        
        _secretKey = Encoding.UTF8.GetBytes(settings.Value.WebhookSecret);
    }

    public bool ValidateSignature(string payload, string signature)
    {
        if (string.IsNullOrWhiteSpace(payload) || string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        try
        {
            using var hmac = new HMACSHA256(_secretKey);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);
            var computedHash = hmac.ComputeHash(payloadBytes);
            var computedSignature = Convert.ToHexString(computedHash).ToLowerInvariant();
            
            // Use constant-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computedSignature),
                Encoding.UTF8.GetBytes(signature.ToLowerInvariant()));
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// DTOs for Supabase webhook payloads.
/// </summary>
public sealed class SupabaseWebhookPayload
{
    public string Type { get; set; } = string.Empty;
    public SupabaseWebhookUser? User { get; set; }
    public SupabaseWebhookRecord? Record { get; set; }
    public SupabaseWebhookRecord? OldRecord { get; set; }
}

public sealed class SupabaseWebhookUser
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public SupabaseUserMetadata? UserMetadata { get; set; }
    public SupabaseAppMetadata? AppMetadata { get; set; }
}

public sealed class SupabaseUserMetadata
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public string? OrganizationName { get; set; }
}

public sealed class SupabaseAppMetadata
{
    public string? Provider { get; set; }
    public string[]? Providers { get; set; }
}

public sealed class SupabaseWebhookRecord
{
    public string Id { get; set; } = string.Empty;
    // Add other fields as needed
}



