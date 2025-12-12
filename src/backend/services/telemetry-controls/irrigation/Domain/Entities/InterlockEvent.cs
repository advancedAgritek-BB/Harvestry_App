using Harvestry.Irrigation.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Irrigation.Domain.Entities;

/// <summary>
/// Records an interlock trip event for audit and analysis.
/// Created whenever a safety interlock prevents or stops irrigation.
/// </summary>
public sealed class InterlockEvent : Entity<Guid>
{
    private InterlockEvent(Guid id) : base(id) { }

    public Guid SiteId { get; private set; }
    public Guid? RunId { get; private set; }
    public Guid? GroupId { get; private set; }
    public InterlockType InterlockType { get; private set; }
    public string Details { get; private set; } = string.Empty;
    public string? SensorStreamId { get; private set; }
    public double? SensorValue { get; private set; }
    public double? ThresholdValue { get; private set; }
    public bool WasPreventive { get; private set; }
    public DateTimeOffset TrippedAt { get; private set; }
    public DateTimeOffset? ClearedAt { get; private set; }
    public Guid? ClearedByUserId { get; private set; }
    public string? ClearanceNotes { get; private set; }
    public bool RequiresAcknowledgment { get; private set; }
    public DateTimeOffset? AcknowledgedAt { get; private set; }
    public Guid? AcknowledgedByUserId { get; private set; }

    public static InterlockEvent Create(
        Guid siteId,
        InterlockType interlockType,
        string details,
        Guid? runId = null,
        Guid? groupId = null,
        string? sensorStreamId = null,
        double? sensorValue = null,
        double? thresholdValue = null,
        bool wasPreventive = false,
        bool requiresAcknowledgment = false)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID is required", nameof(siteId));
        if (string.IsNullOrWhiteSpace(details))
            throw new ArgumentException("Details are required", nameof(details));

        return new InterlockEvent(Guid.NewGuid())
        {
            SiteId = siteId,
            RunId = runId,
            GroupId = groupId,
            InterlockType = interlockType,
            Details = details,
            SensorStreamId = sensorStreamId,
            SensorValue = sensorValue,
            ThresholdValue = thresholdValue,
            WasPreventive = wasPreventive,
            TrippedAt = DateTimeOffset.UtcNow,
            RequiresAcknowledgment = requiresAcknowledgment
        };
    }

    public static InterlockEvent FromPersistence(
        Guid id,
        Guid siteId,
        Guid? runId,
        Guid? groupId,
        InterlockType interlockType,
        string details,
        string? sensorStreamId,
        double? sensorValue,
        double? thresholdValue,
        bool wasPreventive,
        DateTimeOffset trippedAt,
        DateTimeOffset? clearedAt,
        Guid? clearedByUserId,
        string? clearanceNotes,
        bool requiresAcknowledgment,
        DateTimeOffset? acknowledgedAt,
        Guid? acknowledgedByUserId)
    {
        return new InterlockEvent(id)
        {
            SiteId = siteId,
            RunId = runId,
            GroupId = groupId,
            InterlockType = interlockType,
            Details = details,
            SensorStreamId = sensorStreamId,
            SensorValue = sensorValue,
            ThresholdValue = thresholdValue,
            WasPreventive = wasPreventive,
            TrippedAt = trippedAt,
            ClearedAt = clearedAt,
            ClearedByUserId = clearedByUserId,
            ClearanceNotes = clearanceNotes,
            RequiresAcknowledgment = requiresAcknowledgment,
            AcknowledgedAt = acknowledgedAt,
            AcknowledgedByUserId = acknowledgedByUserId
        };
    }

    public void Clear(Guid? userId, string? notes = null)
    {
        if (ClearedAt.HasValue)
            throw new InvalidOperationException("Interlock is already cleared");

        ClearedAt = DateTimeOffset.UtcNow;
        ClearedByUserId = userId;
        ClearanceNotes = notes;
    }

    public void Acknowledge(Guid userId)
    {
        if (!RequiresAcknowledgment)
            throw new InvalidOperationException("This interlock does not require acknowledgment");
        if (AcknowledgedAt.HasValue)
            throw new InvalidOperationException("Interlock is already acknowledged");

        AcknowledgedAt = DateTimeOffset.UtcNow;
        AcknowledgedByUserId = userId;
    }

    public bool IsActive => !ClearedAt.HasValue;
    public bool IsPendingAcknowledgment => RequiresAcknowledgment && !AcknowledgedAt.HasValue;

    public TimeSpan? ActiveDuration => ClearedAt.HasValue
        ? ClearedAt.Value - TrippedAt
        : DateTimeOffset.UtcNow - TrippedAt;
}
