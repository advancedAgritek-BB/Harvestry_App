namespace Harvestry.Compliance.BioTrack.Domain.Entities;

/// <summary>
/// Represents a BioTrack license configuration for a site.
/// BioTrack is used in WA, FL, and other states.
/// </summary>
public sealed class BioTrackLicense
{
    public Guid Id { get; private set; }
    public Guid SiteId { get; private set; }
    public string LicenseNumber { get; private set; } = string.Empty;
    public string StateCode { get; private set; } = string.Empty;
    public string FacilityName { get; private set; } = string.Empty;
    public string? UsernameEncrypted { get; private set; }
    public string? PasswordEncrypted { get; private set; }
    public bool IsActive { get; private set; }
    public bool UseSandbox { get; private set; }
    public bool AutoSyncEnabled { get; private set; }
    public int SyncIntervalMinutes { get; private set; }
    public DateTimeOffset? LastSyncAt { get; private set; }
    public DateTimeOffset? LastSuccessfulSyncAt { get; private set; }
    public string? LastSyncError { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private BioTrackLicense() { }

    public static BioTrackLicense Create(
        Guid siteId,
        string licenseNumber,
        string stateCode,
        string facilityName,
        bool useSandbox = false,
        bool autoSyncEnabled = true,
        int syncIntervalMinutes = 15)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID is required", nameof(siteId));
        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new ArgumentException("License number is required", nameof(licenseNumber));
        if (string.IsNullOrWhiteSpace(stateCode) || stateCode.Length != 2)
            throw new ArgumentException("State code must be 2 characters", nameof(stateCode));

        var now = DateTimeOffset.UtcNow;
        return new BioTrackLicense
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
            UpdatedAt = now
        };
    }

    public void SetCredentials(string usernameEncrypted, string passwordEncrypted)
    {
        UsernameEncrypted = usernameEncrypted;
        PasswordEncrypted = passwordEncrypted;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool HasCredentials => !string.IsNullOrWhiteSpace(UsernameEncrypted) 
        && !string.IsNullOrWhiteSpace(PasswordEncrypted);

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

    public void Activate() { IsActive = true; UpdatedAt = DateTimeOffset.UtcNow; }
    public void Deactivate() { IsActive = false; UpdatedAt = DateTimeOffset.UtcNow; }

    public bool IsSyncDue => AutoSyncEnabled && IsActive && HasCredentials &&
        (!LastSyncAt.HasValue || LastSyncAt.Value.AddMinutes(SyncIntervalMinutes) <= DateTimeOffset.UtcNow);
}
