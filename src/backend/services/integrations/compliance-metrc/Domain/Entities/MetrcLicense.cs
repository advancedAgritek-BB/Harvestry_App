namespace Harvestry.Compliance.Metrc.Domain.Entities;

/// <summary>
/// Represents a METRC license configuration for a site.
/// Stores credentials and sync settings per license.
/// </summary>
public sealed class MetrcLicense
{
    public Guid Id { get; private set; }
    public Guid SiteId { get; private set; }
    public string LicenseNumber { get; private set; } = string.Empty;
    public string StateCode { get; private set; } = string.Empty;
    public string FacilityName { get; private set; } = string.Empty;
    public string? VendorApiKeyEncrypted { get; private set; }
    public string? UserApiKeyEncrypted { get; private set; }
    public bool IsActive { get; private set; }
    public bool UseSandbox { get; private set; }
    public bool AutoSyncEnabled { get; private set; }
    public int SyncIntervalMinutes { get; private set; }
    public DateTimeOffset? LastSyncAt { get; private set; }
    public DateTimeOffset? LastSuccessfulSyncAt { get; private set; }
    public string? LastSyncError { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }

    private MetrcLicense() { }

    /// <summary>
    /// Creates a new METRC license configuration
    /// </summary>
    public static MetrcLicense Create(
        Guid siteId,
        string licenseNumber,
        string stateCode,
        string facilityName,
        Guid? createdByUserId = null,
        bool useSandbox = false,
        bool autoSyncEnabled = true,
        int syncIntervalMinutes = 15)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID is required", nameof(siteId));
        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new ArgumentException("License number is required", nameof(licenseNumber));
        if (string.IsNullOrWhiteSpace(stateCode))
            throw new ArgumentException("State code is required", nameof(stateCode));
        if (stateCode.Length != 2)
            throw new ArgumentException("State code must be 2 characters", nameof(stateCode));
        if (string.IsNullOrWhiteSpace(facilityName))
            throw new ArgumentException("Facility name is required", nameof(facilityName));
        if (syncIntervalMinutes < 5)
            throw new ArgumentException("Sync interval must be at least 5 minutes", nameof(syncIntervalMinutes));

        var now = DateTimeOffset.UtcNow;
        return new MetrcLicense
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            LicenseNumber = licenseNumber.Trim().ToUpperInvariant(),
            StateCode = stateCode.Trim().ToUpperInvariant(),
            FacilityName = facilityName.Trim(),
            IsActive = true,
            UseSandbox = useSandbox,
            AutoSyncEnabled = autoSyncEnabled,
            SyncIntervalMinutes = syncIntervalMinutes,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedByUserId = createdByUserId
        };
    }

    /// <summary>
    /// Reconstitutes a license from persistence
    /// </summary>
    public static MetrcLicense FromPersistence(
        Guid id,
        Guid siteId,
        string licenseNumber,
        string stateCode,
        string facilityName,
        string? vendorApiKeyEncrypted,
        string? userApiKeyEncrypted,
        bool isActive,
        bool useSandbox,
        bool autoSyncEnabled,
        int syncIntervalMinutes,
        DateTimeOffset? lastSyncAt,
        DateTimeOffset? lastSuccessfulSyncAt,
        string? lastSyncError,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        Guid? createdByUserId,
        Guid? updatedByUserId)
    {
        return new MetrcLicense
        {
            Id = id,
            SiteId = siteId,
            LicenseNumber = licenseNumber,
            StateCode = stateCode,
            FacilityName = facilityName,
            VendorApiKeyEncrypted = vendorApiKeyEncrypted,
            UserApiKeyEncrypted = userApiKeyEncrypted,
            IsActive = isActive,
            UseSandbox = useSandbox,
            AutoSyncEnabled = autoSyncEnabled,
            SyncIntervalMinutes = syncIntervalMinutes,
            LastSyncAt = lastSyncAt,
            LastSuccessfulSyncAt = lastSuccessfulSyncAt,
            LastSyncError = lastSyncError,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            CreatedByUserId = createdByUserId,
            UpdatedByUserId = updatedByUserId
        };
    }

    /// <summary>
    /// Sets the API credentials (should be encrypted before storage)
    /// </summary>
    public void SetCredentials(string vendorApiKeyEncrypted, string userApiKeyEncrypted, Guid? updatedByUserId = null)
    {
        if (string.IsNullOrWhiteSpace(vendorApiKeyEncrypted))
            throw new ArgumentException("Vendor API key is required", nameof(vendorApiKeyEncrypted));
        if (string.IsNullOrWhiteSpace(userApiKeyEncrypted))
            throw new ArgumentException("User API key is required", nameof(userApiKeyEncrypted));

        VendorApiKeyEncrypted = vendorApiKeyEncrypted;
        UserApiKeyEncrypted = userApiKeyEncrypted;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedByUserId = updatedByUserId;
    }

    /// <summary>
    /// Checks if credentials are configured
    /// </summary>
    public bool HasCredentials => !string.IsNullOrWhiteSpace(VendorApiKeyEncrypted) 
        && !string.IsNullOrWhiteSpace(UserApiKeyEncrypted);

    /// <summary>
    /// Updates sync settings
    /// </summary>
    public void UpdateSyncSettings(bool autoSyncEnabled, int syncIntervalMinutes, Guid? updatedByUserId = null)
    {
        if (syncIntervalMinutes < 5)
            throw new ArgumentException("Sync interval must be at least 5 minutes", nameof(syncIntervalMinutes));

        AutoSyncEnabled = autoSyncEnabled;
        SyncIntervalMinutes = syncIntervalMinutes;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedByUserId = updatedByUserId;
    }

    /// <summary>
    /// Activates the license
    /// </summary>
    public void Activate(Guid? updatedByUserId = null)
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedByUserId = updatedByUserId;
    }

    /// <summary>
    /// Deactivates the license
    /// </summary>
    public void Deactivate(Guid? updatedByUserId = null)
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedByUserId = updatedByUserId;
    }

    /// <summary>
    /// Records a successful sync
    /// </summary>
    public void RecordSuccessfulSync()
    {
        var now = DateTimeOffset.UtcNow;
        LastSyncAt = now;
        LastSuccessfulSyncAt = now;
        LastSyncError = null;
        UpdatedAt = now;
    }

    /// <summary>
    /// Records a failed sync
    /// </summary>
    public void RecordFailedSync(string errorMessage)
    {
        var now = DateTimeOffset.UtcNow;
        LastSyncAt = now;
        LastSyncError = errorMessage;
        UpdatedAt = now;
    }

    /// <summary>
    /// Checks if sync is due based on interval
    /// </summary>
    public bool IsSyncDue => AutoSyncEnabled && IsActive && HasCredentials &&
        (!LastSyncAt.HasValue || LastSyncAt.Value.AddMinutes(SyncIntervalMinutes) <= DateTimeOffset.UtcNow);

    /// <summary>
    /// Toggles sandbox mode
    /// </summary>
    public void SetSandboxMode(bool useSandbox, Guid? updatedByUserId = null)
    {
        UseSandbox = useSandbox;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedByUserId = updatedByUserId;
    }
}
