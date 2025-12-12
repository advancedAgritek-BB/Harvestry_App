using Harvestry.Compliance.Metrc.Domain.Enums;

namespace Harvestry.Compliance.Metrc.Domain.Entities;

/// <summary>
/// Represents a METRC synchronization job that tracks the overall sync state for a license.
/// A sync job coordinates multiple queue items and tracks progress across entity types.
/// </summary>
public sealed class MetrcSyncJob
{
    public Guid Id { get; private set; }
    public Guid SiteId { get; private set; }
    public string LicenseNumber { get; private set; } = string.Empty;
    public string StateCode { get; private set; } = string.Empty;
    public SyncDirection Direction { get; private set; }
    public SyncStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset? LastHeartbeatAt { get; private set; }
    public int TotalItems { get; private set; }
    public int ProcessedItems { get; private set; }
    public int SuccessfulItems { get; private set; }
    public int FailedItems { get; private set; }
    public int RetryCount { get; private set; }
    public int MaxRetries { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? ErrorDetails { get; private set; }
    public string? InitiatedBy { get; private set; }
    public Guid? InitiatedByUserId { get; private set; }

    private MetrcSyncJob() { }

    /// <summary>
    /// Creates a new METRC sync job
    /// </summary>
    public static MetrcSyncJob Create(
        Guid siteId,
        string licenseNumber,
        string stateCode,
        SyncDirection direction,
        Guid? initiatedByUserId = null,
        string? initiatedBy = null,
        int maxRetries = 3)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID is required", nameof(siteId));
        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new ArgumentException("License number is required", nameof(licenseNumber));
        if (string.IsNullOrWhiteSpace(stateCode))
            throw new ArgumentException("State code is required", nameof(stateCode));

        return new MetrcSyncJob
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            LicenseNumber = licenseNumber.Trim().ToUpperInvariant(),
            StateCode = stateCode.Trim().ToUpperInvariant(),
            Direction = direction,
            Status = SyncStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            MaxRetries = maxRetries,
            InitiatedByUserId = initiatedByUserId,
            InitiatedBy = initiatedBy ?? "system"
        };
    }

    /// <summary>
    /// Reconstitutes a sync job from persistence
    /// </summary>
    public static MetrcSyncJob FromPersistence(
        Guid id,
        Guid siteId,
        string licenseNumber,
        string stateCode,
        SyncDirection direction,
        SyncStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset? startedAt,
        DateTimeOffset? completedAt,
        DateTimeOffset? lastHeartbeatAt,
        int totalItems,
        int processedItems,
        int successfulItems,
        int failedItems,
        int retryCount,
        int maxRetries,
        string? errorMessage,
        string? errorDetails,
        string? initiatedBy,
        Guid? initiatedByUserId)
    {
        return new MetrcSyncJob
        {
            Id = id,
            SiteId = siteId,
            LicenseNumber = licenseNumber,
            StateCode = stateCode,
            Direction = direction,
            Status = status,
            CreatedAt = createdAt,
            StartedAt = startedAt,
            CompletedAt = completedAt,
            LastHeartbeatAt = lastHeartbeatAt,
            TotalItems = totalItems,
            ProcessedItems = processedItems,
            SuccessfulItems = successfulItems,
            FailedItems = failedItems,
            RetryCount = retryCount,
            MaxRetries = maxRetries,
            ErrorMessage = errorMessage,
            ErrorDetails = errorDetails,
            InitiatedBy = initiatedBy,
            InitiatedByUserId = initiatedByUserId
        };
    }

    /// <summary>
    /// Marks the job as started
    /// </summary>
    public void Start(int totalItems)
    {
        if (Status != SyncStatus.Pending && Status != SyncStatus.Failed)
            throw new InvalidOperationException($"Cannot start job in status {Status}");

        Status = SyncStatus.Processing;
        StartedAt = DateTimeOffset.UtcNow;
        LastHeartbeatAt = DateTimeOffset.UtcNow;
        TotalItems = totalItems;
    }

    /// <summary>
    /// Updates progress with a heartbeat
    /// </summary>
    public void RecordProgress(int processedItems, int successfulItems, int failedItems)
    {
        ProcessedItems = processedItems;
        SuccessfulItems = successfulItems;
        FailedItems = failedItems;
        LastHeartbeatAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Marks the job as completed successfully
    /// </summary>
    public void Complete()
    {
        Status = SyncStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        LastHeartbeatAt = DateTimeOffset.UtcNow;
        ErrorMessage = null;
        ErrorDetails = null;
    }

    /// <summary>
    /// Marks the job as failed
    /// </summary>
    public void Fail(string errorMessage, string? errorDetails = null)
    {
        RetryCount++;
        LastHeartbeatAt = DateTimeOffset.UtcNow;
        ErrorMessage = errorMessage;
        ErrorDetails = errorDetails;

        if (RetryCount >= MaxRetries)
        {
            Status = SyncStatus.FailedPermanent;
            CompletedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            Status = SyncStatus.Failed;
        }
    }

    /// <summary>
    /// Marks the job as requiring manual review
    /// </summary>
    public void RequireManualReview(string reason)
    {
        Status = SyncStatus.ManualReviewRequired;
        ErrorMessage = reason;
        LastHeartbeatAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Cancels the job
    /// </summary>
    public void Cancel(string? reason = null)
    {
        Status = SyncStatus.Cancelled;
        CompletedAt = DateTimeOffset.UtcNow;
        ErrorMessage = reason;
    }

    /// <summary>
    /// Checks if the job can be retried
    /// </summary>
    public bool CanRetry => Status == SyncStatus.Failed && RetryCount < MaxRetries;

    /// <summary>
    /// Checks if the job is in a terminal state
    /// </summary>
    public bool IsTerminal => Status is SyncStatus.Completed 
        or SyncStatus.FailedPermanent 
        or SyncStatus.Cancelled;

    /// <summary>
    /// Gets the duration of the job
    /// </summary>
    public TimeSpan? Duration => StartedAt.HasValue
        ? (CompletedAt ?? DateTimeOffset.UtcNow) - StartedAt.Value
        : null;
}
