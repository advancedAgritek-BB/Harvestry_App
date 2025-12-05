using Harvestry.Irrigation.Domain.ValueObjects;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Irrigation.Domain.Entities;

/// <summary>
/// Represents an irrigation event that has been queued due to flow rate constraints
/// </summary>
public sealed class QueuedIrrigationEvent : Entity<Guid>
{
    // Private constructor for EF Core
    private QueuedIrrigationEvent(Guid id) : base(id) { }

    private QueuedIrrigationEvent(
        Guid id,
        Guid siteId,
        Guid programId,
        Guid? scheduleId,
        Guid[] targetZoneIds,
        DateTime originalScheduledTime,
        DateTime expectedExecutionTime,
        string queueReason) : base(id)
    {
        SiteId = siteId;
        ProgramId = programId;
        ScheduleId = scheduleId;
        TargetZoneIds = targetZoneIds;
        OriginalScheduledTime = originalScheduledTime;
        ExpectedExecutionTime = expectedExecutionTime;
        QueueReason = queueReason;
        Status = QueuedEventStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid SiteId { get; private set; }
    public Guid ProgramId { get; private set; }
    public Guid? ScheduleId { get; private set; }
    public Guid[] TargetZoneIds { get; private set; } = Array.Empty<Guid>();
    
    /// <summary>
    /// The time this event was originally scheduled to run
    /// </summary>
    public DateTime OriginalScheduledTime { get; private set; }
    
    /// <summary>
    /// The expected time when this event will actually execute
    /// </summary>
    public DateTime ExpectedExecutionTime { get; private set; }
    
    /// <summary>
    /// Reason why this event was queued
    /// </summary>
    public string QueueReason { get; private set; } = null!;
    
    /// <summary>
    /// Current status of the queued event
    /// </summary>
    public QueuedEventStatus Status { get; private set; }
    
    /// <summary>
    /// Time when the event was actually executed (if executed)
    /// </summary>
    public DateTime? ExecutedAt { get; private set; }
    
    /// <summary>
    /// ID of the irrigation run created (if executed)
    /// </summary>
    public Guid? IrrigationRunId { get; private set; }
    
    /// <summary>
    /// Error message if the event failed
    /// </summary>
    public string? FailureMessage { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Calculate delay information
    /// </summary>
    public QueuedEventInfo DelayDuration => new QueuedEventInfo(
        OriginalScheduledTime,
        ExpectedExecutionTime,
        ExpectedExecutionTime - OriginalScheduledTime,
        0, // Position is calculated separately
        QueueReason);

    public static QueuedIrrigationEvent Create(
        Guid siteId,
        Guid programId,
        Guid? scheduleId,
        Guid[] targetZoneIds,
        DateTime originalScheduledTime,
        DateTime expectedExecutionTime,
        string queueReason)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));
        if (programId == Guid.Empty)
            throw new ArgumentException("Program ID cannot be empty", nameof(programId));
        if (targetZoneIds == null || targetZoneIds.Length == 0)
            throw new ArgumentException("At least one target zone is required", nameof(targetZoneIds));
        if (string.IsNullOrWhiteSpace(queueReason))
            throw new ArgumentException("Queue reason is required", nameof(queueReason));

        return new QueuedIrrigationEvent(
            Guid.NewGuid(),
            siteId,
            programId,
            scheduleId,
            targetZoneIds,
            originalScheduledTime,
            expectedExecutionTime,
            queueReason);
    }

    public void UpdateExpectedExecutionTime(DateTime newExpectedTime)
    {
        if (Status != QueuedEventStatus.Pending)
            throw new InvalidOperationException($"Cannot update expected time for event in status {Status}");

        ExpectedExecutionTime = newExpectedTime;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkExecuted(Guid? irrigationRunId = null)
    {
        if (Status != QueuedEventStatus.Pending)
            throw new InvalidOperationException($"Cannot mark as executed event in status {Status}");

        Status = QueuedEventStatus.Executed;
        ExecutedAt = DateTime.UtcNow;
        IrrigationRunId = irrigationRunId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string failureMessage)
    {
        if (Status != QueuedEventStatus.Pending)
            throw new InvalidOperationException($"Cannot mark as failed event in status {Status}");

        Status = QueuedEventStatus.Failed;
        FailureMessage = failureMessage;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason)
    {
        if (Status != QueuedEventStatus.Pending)
            throw new InvalidOperationException($"Cannot cancel event in status {Status}");

        Status = QueuedEventStatus.Cancelled;
        FailureMessage = reason;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum QueuedEventStatus
{
    Pending,
    Executed,
    Failed,
    Cancelled
}




