using Harvestry.Integration.QuickBooks.Domain.Enums;

namespace Harvestry.Integration.QuickBooks.Domain.Entities;

/// <summary>
/// Represents a QuickBooks Online connection for a site.
/// Stores OAuth tokens and connection state.
/// </summary>
public sealed class QuickBooksConnection
{
    public Guid Id { get; private set; }
    public Guid SiteId { get; private set; }
    public string RealmId { get; private set; } = string.Empty;
    public string CompanyName { get; private set; } = string.Empty;
    public string? AccessTokenEncrypted { get; private set; }
    public string? RefreshTokenEncrypted { get; private set; }
    public DateTimeOffset? AccessTokenExpiresAt { get; private set; }
    public DateTimeOffset? RefreshTokenExpiresAt { get; private set; }
    public QuickBooksConnectionStatus Status { get; private set; }
    public bool IsActive { get; private set; }
    public bool AutoSyncEnabled { get; private set; }
    public int SyncIntervalMinutes { get; private set; }
    public DateTimeOffset? LastSyncAt { get; private set; }
    public DateTimeOffset? LastSuccessfulSyncAt { get; private set; }
    public string? LastSyncError { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public Guid? ConnectedByUserId { get; private set; }

    private QuickBooksConnection() { }

    public static QuickBooksConnection Create(
        Guid siteId,
        string realmId,
        string companyName,
        Guid? connectedByUserId = null)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID is required", nameof(siteId));
        if (string.IsNullOrWhiteSpace(realmId))
            throw new ArgumentException("Realm ID is required", nameof(realmId));
        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Company name is required", nameof(companyName));

        var now = DateTimeOffset.UtcNow;
        return new QuickBooksConnection
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            RealmId = realmId,
            CompanyName = companyName,
            Status = QuickBooksConnectionStatus.NotConnected,
            IsActive = true,
            AutoSyncEnabled = true,
            SyncIntervalMinutes = 30,
            CreatedAt = now,
            UpdatedAt = now,
            ConnectedByUserId = connectedByUserId
        };
    }

    public void SetTokens(
        string accessTokenEncrypted,
        string refreshTokenEncrypted,
        DateTimeOffset accessTokenExpiresAt,
        DateTimeOffset refreshTokenExpiresAt)
    {
        AccessTokenEncrypted = accessTokenEncrypted;
        RefreshTokenEncrypted = refreshTokenEncrypted;
        AccessTokenExpiresAt = accessTokenExpiresAt;
        RefreshTokenExpiresAt = refreshTokenExpiresAt;
        Status = QuickBooksConnectionStatus.Connected;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RefreshAccessToken(
        string accessTokenEncrypted,
        DateTimeOffset accessTokenExpiresAt)
    {
        AccessTokenEncrypted = accessTokenEncrypted;
        AccessTokenExpiresAt = accessTokenExpiresAt;
        Status = QuickBooksConnectionStatus.Connected;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkTokenExpired()
    {
        Status = QuickBooksConnectionStatus.TokenExpired;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkRefreshRequired()
    {
        Status = QuickBooksConnectionStatus.RefreshRequired;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkError(string errorMessage)
    {
        Status = QuickBooksConnectionStatus.Error;
        LastSyncError = errorMessage;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Disconnect()
    {
        Status = QuickBooksConnectionStatus.NotConnected;
        AccessTokenEncrypted = null;
        RefreshTokenEncrypted = null;
        AccessTokenExpiresAt = null;
        RefreshTokenExpiresAt = null;
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RecordSuccessfulSync()
    {
        var now = DateTimeOffset.UtcNow;
        LastSyncAt = now;
        LastSuccessfulSyncAt = now;
        LastSyncError = null;
        UpdatedAt = now;
    }

    public void RecordFailedSync(string errorMessage)
    {
        LastSyncAt = DateTimeOffset.UtcNow;
        LastSyncError = errorMessage;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsConnected => Status == QuickBooksConnectionStatus.Connected
        && !string.IsNullOrWhiteSpace(AccessTokenEncrypted)
        && AccessTokenExpiresAt.HasValue
        && AccessTokenExpiresAt.Value > DateTimeOffset.UtcNow;

    public bool NeedsTokenRefresh => Status == QuickBooksConnectionStatus.Connected
        && AccessTokenExpiresAt.HasValue
        && AccessTokenExpiresAt.Value <= DateTimeOffset.UtcNow.AddMinutes(5);

    public bool IsSyncDue => IsActive && AutoSyncEnabled && IsConnected &&
        (!LastSyncAt.HasValue || LastSyncAt.Value.AddMinutes(SyncIntervalMinutes) <= DateTimeOffset.UtcNow);
}
