using Harvestry.Compliance.Metrc.Domain.Enums;

namespace Harvestry.Compliance.Metrc.Domain.Entities;

/// <summary>
/// Tracks sync progress per entity type for a license.
/// Used to enable incremental syncing by storing the last synced timestamp.
/// </summary>
public sealed class MetrcSyncCheckpoint
{
    public Guid Id { get; private set; }
    public Guid LicenseId { get; private set; }
    public MetrcEntityType EntityType { get; private set; }
    public SyncDirection Direction { get; private set; }
    public DateTimeOffset? LastSyncTimestamp { get; private set; }
    public long? LastSyncedMetrcId { get; private set; }
    public int LastSyncItemCount { get; private set; }
    public DateTimeOffset? LastSuccessfulSyncAt { get; private set; }
    public DateTimeOffset? LastFailedSyncAt { get; private set; }
    public string? LastError { get; private set; }
    public int ConsecutiveFailures { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private MetrcSyncCheckpoint() { }

    /// <summary>
    /// Creates a new sync checkpoint
    /// </summary>
    public static MetrcSyncCheckpoint Create(
        Guid licenseId,
        MetrcEntityType entityType,
        SyncDirection direction)
    {
        if (licenseId == Guid.Empty)
            throw new ArgumentException("License ID is required", nameof(licenseId));

        var now = DateTimeOffset.UtcNow;
        return new MetrcSyncCheckpoint
        {
            Id = Guid.NewGuid(),
            LicenseId = licenseId,
            EntityType = entityType,
            Direction = direction,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>
    /// Reconstitutes a checkpoint from persistence
    /// </summary>
    public static MetrcSyncCheckpoint FromPersistence(
        Guid id,
        Guid licenseId,
        MetrcEntityType entityType,
        SyncDirection direction,
        DateTimeOffset? lastSyncTimestamp,
        long? lastSyncedMetrcId,
        int lastSyncItemCount,
        DateTimeOffset? lastSuccessfulSyncAt,
        DateTimeOffset? lastFailedSyncAt,
        string? lastError,
        int consecutiveFailures,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        return new MetrcSyncCheckpoint
        {
            Id = id,
            LicenseId = licenseId,
            EntityType = entityType,
            Direction = direction,
            LastSyncTimestamp = lastSyncTimestamp,
            LastSyncedMetrcId = lastSyncedMetrcId,
            LastSyncItemCount = lastSyncItemCount,
            LastSuccessfulSyncAt = lastSuccessfulSyncAt,
            LastFailedSyncAt = lastFailedSyncAt,
            LastError = lastError,
            ConsecutiveFailures = consecutiveFailures,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }

    /// <summary>
    /// Records a successful sync
    /// </summary>
    public void RecordSuccess(DateTimeOffset syncTimestamp, long? lastMetrcId, int itemCount)
    {
        var now = DateTimeOffset.UtcNow;
        LastSyncTimestamp = syncTimestamp;
        LastSyncedMetrcId = lastMetrcId;
        LastSyncItemCount = itemCount;
        LastSuccessfulSyncAt = now;
        LastError = null;
        ConsecutiveFailures = 0;
        UpdatedAt = now;
    }

    /// <summary>
    /// Records a failed sync
    /// </summary>
    public void RecordFailure(string error)
    {
        var now = DateTimeOffset.UtcNow;
        LastFailedSyncAt = now;
        LastError = error;
        ConsecutiveFailures++;
        UpdatedAt = now;
    }

    /// <summary>
    /// Resets the checkpoint to force a full sync
    /// </summary>
    public void Reset()
    {
        LastSyncTimestamp = null;
        LastSyncedMetrcId = null;
        LastSyncItemCount = 0;
        LastError = null;
        ConsecutiveFailures = 0;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the next sync start timestamp (for incremental sync)
    /// </summary>
    public DateTimeOffset? GetNextSyncStart()
    {
        // If we have a last successful sync, start from there with a small buffer
        // to handle any items that might have been in-flight
        return LastSyncTimestamp?.AddMinutes(-5);
    }

    /// <summary>
    /// Determines if a full sync is needed (no checkpoint or too many failures)
    /// </summary>
    public bool RequiresFullSync => !LastSyncTimestamp.HasValue || ConsecutiveFailures >= 3;
}
