using Harvestry.Compliance.Metrc.Domain.Enums;

namespace Harvestry.Compliance.Metrc.Domain.Entities;

/// <summary>
/// Represents a single item in the METRC sync queue (outbox pattern).
/// Each queue item represents one operation to be performed against METRC.
/// </summary>
public sealed class MetrcQueueItem
{
    public Guid Id { get; private set; }
    public Guid SyncJobId { get; private set; }
    public Guid SiteId { get; private set; }
    public string LicenseNumber { get; private set; } = string.Empty;
    public MetrcEntityType EntityType { get; private set; }
    public MetrcOperationType OperationType { get; private set; }
    public Guid HarvestryEntityId { get; private set; }
    public long? MetrcId { get; private set; }
    public string? MetrcLabel { get; private set; }
    public string PayloadJson { get; private set; } = string.Empty;
    public SyncStatus Status { get; private set; }
    public int Priority { get; private set; }
    public int RetryCount { get; private set; }
    public int MaxRetries { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ScheduledAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? ErrorCode { get; private set; }
    public string? ResponseJson { get; private set; }
    public string? IdempotencyKey { get; private set; }
    public Guid? DependsOnItemId { get; private set; }

    private MetrcQueueItem() { }

    /// <summary>
    /// Creates a new queue item for outbound sync
    /// </summary>
    public static MetrcQueueItem Create(
        Guid syncJobId,
        Guid siteId,
        string licenseNumber,
        MetrcEntityType entityType,
        MetrcOperationType operationType,
        Guid harvestryEntityId,
        string payloadJson,
        int priority = 100,
        long? metrcId = null,
        string? metrcLabel = null,
        string? idempotencyKey = null,
        Guid? dependsOnItemId = null,
        int maxRetries = 3)
    {
        if (syncJobId == Guid.Empty)
            throw new ArgumentException("Sync job ID is required", nameof(syncJobId));
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID is required", nameof(siteId));
        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new ArgumentException("License number is required", nameof(licenseNumber));
        if (harvestryEntityId == Guid.Empty)
            throw new ArgumentException("Harvestry entity ID is required", nameof(harvestryEntityId));
        if (string.IsNullOrWhiteSpace(payloadJson))
            throw new ArgumentException("Payload JSON is required", nameof(payloadJson));

        return new MetrcQueueItem
        {
            Id = Guid.NewGuid(),
            SyncJobId = syncJobId,
            SiteId = siteId,
            LicenseNumber = licenseNumber.Trim().ToUpperInvariant(),
            EntityType = entityType,
            OperationType = operationType,
            HarvestryEntityId = harvestryEntityId,
            MetrcId = metrcId,
            MetrcLabel = metrcLabel,
            PayloadJson = payloadJson,
            Status = SyncStatus.Pending,
            Priority = priority,
            MaxRetries = maxRetries,
            CreatedAt = DateTimeOffset.UtcNow,
            IdempotencyKey = idempotencyKey ?? $"{harvestryEntityId}:{operationType}:{DateTimeOffset.UtcNow.Ticks}",
            DependsOnItemId = dependsOnItemId
        };
    }

    /// <summary>
    /// Reconstitutes a queue item from persistence
    /// </summary>
    public static MetrcQueueItem FromPersistence(
        Guid id,
        Guid syncJobId,
        Guid siteId,
        string licenseNumber,
        MetrcEntityType entityType,
        MetrcOperationType operationType,
        Guid harvestryEntityId,
        long? metrcId,
        string? metrcLabel,
        string payloadJson,
        SyncStatus status,
        int priority,
        int retryCount,
        int maxRetries,
        DateTimeOffset createdAt,
        DateTimeOffset? scheduledAt,
        DateTimeOffset? processedAt,
        DateTimeOffset? completedAt,
        string? errorMessage,
        string? errorCode,
        string? responseJson,
        string? idempotencyKey,
        Guid? dependsOnItemId)
    {
        return new MetrcQueueItem
        {
            Id = id,
            SyncJobId = syncJobId,
            SiteId = siteId,
            LicenseNumber = licenseNumber,
            EntityType = entityType,
            OperationType = operationType,
            HarvestryEntityId = harvestryEntityId,
            MetrcId = metrcId,
            MetrcLabel = metrcLabel,
            PayloadJson = payloadJson,
            Status = status,
            Priority = priority,
            RetryCount = retryCount,
            MaxRetries = maxRetries,
            CreatedAt = createdAt,
            ScheduledAt = scheduledAt,
            ProcessedAt = processedAt,
            CompletedAt = completedAt,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode,
            ResponseJson = responseJson,
            IdempotencyKey = idempotencyKey,
            DependsOnItemId = dependsOnItemId
        };
    }

    /// <summary>
    /// Schedules the item for processing at a specific time
    /// </summary>
    public void Schedule(DateTimeOffset scheduledAt)
    {
        if (Status != SyncStatus.Pending)
            throw new InvalidOperationException($"Cannot schedule item in status {Status}");

        ScheduledAt = scheduledAt;
    }

    /// <summary>
    /// Marks the item as being processed
    /// </summary>
    public void MarkProcessing()
    {
        if (Status != SyncStatus.Pending && Status != SyncStatus.Failed)
            throw new InvalidOperationException($"Cannot process item in status {Status}");

        Status = SyncStatus.Processing;
        ProcessedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Marks the item as completed successfully
    /// </summary>
    public void Complete(long? metrcId = null, string? metrcLabel = null, string? responseJson = null)
    {
        Status = SyncStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        MetrcId = metrcId ?? MetrcId;
        MetrcLabel = metrcLabel ?? MetrcLabel;
        ResponseJson = responseJson;
        ErrorMessage = null;
        ErrorCode = null;
    }

    /// <summary>
    /// Marks the item as failed
    /// </summary>
    public void Fail(string errorMessage, string? errorCode = null, string? responseJson = null)
    {
        RetryCount++;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        ResponseJson = responseJson;

        if (RetryCount >= MaxRetries)
        {
            Status = SyncStatus.FailedPermanent;
            CompletedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            Status = SyncStatus.Failed;
            // Apply exponential backoff for retry scheduling
            var backoffMinutes = Math.Pow(2, RetryCount);
            ScheduledAt = DateTimeOffset.UtcNow.AddMinutes(backoffMinutes);
        }
    }

    /// <summary>
    /// Marks the item as requiring manual review
    /// </summary>
    public void RequireManualReview(string reason)
    {
        Status = SyncStatus.ManualReviewRequired;
        ErrorMessage = reason;
    }

    /// <summary>
    /// Cancels the item
    /// </summary>
    public void Cancel(string? reason = null)
    {
        Status = SyncStatus.Cancelled;
        CompletedAt = DateTimeOffset.UtcNow;
        ErrorMessage = reason;
    }

    /// <summary>
    /// Checks if the item can be retried
    /// </summary>
    public bool CanRetry => Status == SyncStatus.Failed && RetryCount < MaxRetries;

    /// <summary>
    /// Checks if the item is in a terminal state
    /// </summary>
    public bool IsTerminal => Status is SyncStatus.Completed
        or SyncStatus.FailedPermanent
        or SyncStatus.Cancelled;

    /// <summary>
    /// Checks if the item is ready to be processed
    /// </summary>
    public bool IsReadyForProcessing => 
        (Status == SyncStatus.Pending || Status == SyncStatus.Failed) &&
        (!ScheduledAt.HasValue || ScheduledAt.Value <= DateTimeOffset.UtcNow);

    /// <summary>
    /// Gets the duration from creation to completion
    /// </summary>
    public TimeSpan? TotalDuration => CompletedAt.HasValue
        ? CompletedAt.Value - CreatedAt
        : null;

    /// <summary>
    /// Gets the processing duration
    /// </summary>
    public TimeSpan? ProcessingDuration => ProcessedAt.HasValue && CompletedAt.HasValue
        ? CompletedAt.Value - ProcessedAt.Value
        : null;
}
