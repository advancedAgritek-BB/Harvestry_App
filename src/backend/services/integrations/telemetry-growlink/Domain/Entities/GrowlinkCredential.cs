using Harvestry.Integration.Growlink.Domain.Enums;

namespace Harvestry.Integration.Growlink.Domain.Entities;

/// <summary>
/// Stores OAuth credentials for Growlink API access per site.
/// Tokens are encrypted at rest.
/// </summary>
public sealed class GrowlinkCredential : AggregateRoot<Guid>
{
    public Guid SiteId { get; private set; }
    public string GrowlinkAccountId { get; private set; } = string.Empty;
    public string AccessToken { get; private set; } = string.Empty;
    public string RefreshToken { get; private set; } = string.Empty;
    public DateTimeOffset TokenExpiresAt { get; private set; }
    public GrowlinkConnectionStatus Status { get; private set; }
    public DateTimeOffset? LastSyncAt { get; private set; }
    public string? LastSyncError { get; private set; }
    public int ConsecutiveFailures { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private GrowlinkCredential() { }

    private GrowlinkCredential(
        Guid id,
        Guid siteId,
        string growlinkAccountId,
        string accessToken,
        string refreshToken,
        DateTimeOffset tokenExpiresAt)
        : base(id)
    {
        SiteId = siteId;
        GrowlinkAccountId = growlinkAccountId;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        TokenExpiresAt = tokenExpiresAt;
        Status = GrowlinkConnectionStatus.Connected;
        ConsecutiveFailures = 0;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Creates a new Growlink credential after successful OAuth.
    /// </summary>
    public static GrowlinkCredential Create(
        Guid siteId,
        string growlinkAccountId,
        string accessToken,
        string refreshToken,
        DateTimeOffset tokenExpiresAt)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID is required", nameof(siteId));

        if (string.IsNullOrWhiteSpace(growlinkAccountId))
            throw new ArgumentException("Growlink account ID is required", nameof(growlinkAccountId));

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new ArgumentException("Access token is required", nameof(accessToken));

        return new GrowlinkCredential(
            Guid.NewGuid(),
            siteId,
            growlinkAccountId,
            accessToken,
            refreshToken,
            tokenExpiresAt);
    }

    /// <summary>
    /// Updates tokens after a refresh.
    /// </summary>
    public void UpdateTokens(string accessToken, string refreshToken, DateTimeOffset expiresAt)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new ArgumentException("Access token is required", nameof(accessToken));

        AccessToken = accessToken;
        RefreshToken = refreshToken;
        TokenExpiresAt = expiresAt;
        Status = GrowlinkConnectionStatus.Connected;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Records a successful sync.
    /// </summary>
    public void RecordSuccessfulSync()
    {
        LastSyncAt = DateTimeOffset.UtcNow;
        LastSyncError = null;
        ConsecutiveFailures = 0;
        Status = GrowlinkConnectionStatus.Connected;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Records a sync failure.
    /// </summary>
    public void RecordSyncFailure(string error)
    {
        LastSyncAt = DateTimeOffset.UtcNow;
        LastSyncError = error;
        ConsecutiveFailures++;
        UpdatedAt = DateTimeOffset.UtcNow;

        // Mark as error state after 5 consecutive failures
        if (ConsecutiveFailures >= 5)
        {
            Status = GrowlinkConnectionStatus.Error;
        }
    }

    /// <summary>
    /// Marks the credential as requiring token refresh.
    /// </summary>
    public void MarkTokenExpired()
    {
        Status = GrowlinkConnectionStatus.TokenExpired;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Disconnects the integration.
    /// </summary>
    public void Disconnect()
    {
        Status = GrowlinkConnectionStatus.Disconnected;
        AccessToken = string.Empty;
        RefreshToken = string.Empty;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Checks if the token needs refresh.
    /// </summary>
    public bool NeedsTokenRefresh(TimeSpan bufferTime)
    {
        return DateTimeOffset.UtcNow.Add(bufferTime) >= TokenExpiresAt;
    }

    /// <summary>
    /// Checks if the credential is in a usable state.
    /// </summary>
    public bool IsUsable()
    {
        return Status is GrowlinkConnectionStatus.Connected or GrowlinkConnectionStatus.TokenExpired
               && !string.IsNullOrWhiteSpace(RefreshToken);
    }

    /// <summary>
    /// Rehydrates from persistence.
    /// </summary>
    public static GrowlinkCredential FromPersistence(
        Guid id,
        Guid siteId,
        string growlinkAccountId,
        string accessToken,
        string refreshToken,
        DateTimeOffset tokenExpiresAt,
        GrowlinkConnectionStatus status,
        DateTimeOffset? lastSyncAt,
        string? lastSyncError,
        int consecutiveFailures,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        return new GrowlinkCredential
        {
            Id = id,
            SiteId = siteId,
            GrowlinkAccountId = growlinkAccountId,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenExpiresAt = tokenExpiresAt,
            Status = status,
            LastSyncAt = lastSyncAt,
            LastSyncError = lastSyncError,
            ConsecutiveFailures = consecutiveFailures,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }
}





