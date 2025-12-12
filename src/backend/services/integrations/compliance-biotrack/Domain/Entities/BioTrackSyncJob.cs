using Harvestry.Compliance.BioTrack.Domain.Enums;

namespace Harvestry.Compliance.BioTrack.Domain.Entities;

/// <summary>
/// Represents a BioTrack synchronization job that tracks sync state for a license.
/// Mirrors METRC sync job structure for consistent patterns.
/// </summary>
public sealed class BioTrackSyncJob
{
    public Guid Id { get; private set; }
    public Guid SiteId { get; private set; }
    public string LicenseNumber { get; private set; } = string.Empty;
    public string StateCode { get; private set; } = string.Empty;
    public BioTrackSyncDirection Direction { get; private set; }
    public BioTrackSyncStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public int TotalItems { get; private set; }
    public int ProcessedItems { get; private set; }
    public int SuccessfulItems { get; private set; }
    public int FailedItems { get; private set; }
    public int RetryCount { get; private set; }
    public int MaxRetries { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? InitiatedBy { get; private set; }

    private BioTrackSyncJob() { }

    public static BioTrackSyncJob Create(
        Guid siteId,
        string licenseNumber,
        string stateCode,
        BioTrackSyncDirection direction,
        string? initiatedBy = null,
        int maxRetries = 3)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID is required", nameof(siteId));
        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new ArgumentException("License number is required", nameof(licenseNumber));

        return new BioTrackSyncJob
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            LicenseNumber = licenseNumber.Trim().ToUpperInvariant(),
            StateCode = stateCode.Trim().ToUpperInvariant(),
            Direction = direction,
            Status = BioTrackSyncStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            MaxRetries = maxRetries,
            InitiatedBy = initiatedBy ?? "system"
        };
    }

    public void Start(int totalItems)
    {
        if (Status != BioTrackSyncStatus.Pending && Status != BioTrackSyncStatus.Failed)
            throw new InvalidOperationException($"Cannot start job in status {Status}");

        Status = BioTrackSyncStatus.Processing;
        StartedAt = DateTimeOffset.UtcNow;
        TotalItems = totalItems;
    }

    public void Complete()
    {
        Status = BioTrackSyncStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void Fail(string errorMessage)
    {
        RetryCount++;
        ErrorMessage = errorMessage;
        Status = RetryCount >= MaxRetries 
            ? BioTrackSyncStatus.FailedPermanent 
            : BioTrackSyncStatus.Failed;
        if (Status == BioTrackSyncStatus.FailedPermanent)
            CompletedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel(string? reason = null)
    {
        Status = BioTrackSyncStatus.Cancelled;
        CompletedAt = DateTimeOffset.UtcNow;
        ErrorMessage = reason;
    }

    public bool IsTerminal => Status is BioTrackSyncStatus.Completed
        or BioTrackSyncStatus.FailedPermanent
        or BioTrackSyncStatus.Cancelled;
}
