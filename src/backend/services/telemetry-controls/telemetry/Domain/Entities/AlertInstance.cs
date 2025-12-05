using Harvestry.Telemetry.Domain.Enums;

namespace Harvestry.Telemetry.Domain.Entities;

/// <summary>
/// Represents an instance of a fired alert.
/// Aggregate root for alert lifecycle (fired, cleared, acknowledged).
/// </summary>
public class AlertInstance : AggregateRoot<Guid>
{
    public Guid SiteId { get; private set; }
    public Guid RuleId { get; private set; }
    public Guid StreamId { get; private set; }
    public DateTimeOffset FiredAt { get; private set; }
    public DateTimeOffset? ClearedAt { get; private set; }
    public AlertSeverity Severity { get; private set; }
    public double? CurrentValue { get; private set; }
    public double? ThresholdValue { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public DateTimeOffset? AcknowledgedAt { get; private set; }
    public Guid? AcknowledgedBy { get; private set; }
    public string? AcknowledgmentNotes { get; private set; }
    public Dictionary<string, object>? Metadata { get; private set; }
    
    // For EF Core
    private AlertInstance() { }
    
    private AlertInstance(
        Guid id,
        Guid siteId,
        Guid ruleId,
        Guid streamId,
        AlertSeverity severity,
        string message)
    {
        Id = id;
        SiteId = siteId;
        RuleId = ruleId;
        StreamId = streamId;
        Severity = severity;
        Message = message;
        FiredAt = DateTimeOffset.UtcNow;
    }
    
    /// <summary>
    /// Creates a new alert instance when rule fires.
    /// </summary>
    public static AlertInstance Fire(
        Guid siteId,
        Guid ruleId,
        Guid streamId,
        AlertSeverity severity,
        string message,
        double? currentValue = null,
        double? thresholdValue = null,
        Dictionary<string, object>? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Alert message is required", nameof(message));
            
        var alert = new AlertInstance(Guid.NewGuid(), siteId, ruleId, streamId, severity, message)
        {
            CurrentValue = currentValue,
            ThresholdValue = thresholdValue,
            Metadata = metadata
        };
        
        return alert;
    }
    
    /// <summary>
    /// Rehydrates alert instance from persistence layer.
    /// </summary>
    public static AlertInstance FromPersistence(
        Guid id,
        Guid siteId,
        Guid ruleId,
        Guid streamId,
        DateTimeOffset firedAt,
        DateTimeOffset? clearedAt,
        AlertSeverity severity,
        double? currentValue,
        double? thresholdValue,
        string message,
        DateTimeOffset? acknowledgedAt,
        Guid? acknowledgedBy,
        string? acknowledgmentNotes,
        Dictionary<string, object>? metadata)
    {
        return new AlertInstance
        {
            Id = id,
            SiteId = siteId,
            RuleId = ruleId,
            StreamId = streamId,
            FiredAt = firedAt,
            ClearedAt = clearedAt,
            Severity = severity,
            CurrentValue = currentValue,
            ThresholdValue = thresholdValue,
            Message = message,
            AcknowledgedAt = acknowledgedAt,
            AcknowledgedBy = acknowledgedBy,
            AcknowledgmentNotes = acknowledgmentNotes,
            Metadata = metadata
        };
    }
    
    /// <summary>
    /// Clears the alert (condition no longer met).
    /// </summary>
    public void Clear(DateTimeOffset clearedAt)
    {
        if (ClearedAt.HasValue)
            throw new InvalidOperationException("Alert already cleared");
            
        ClearedAt = clearedAt;
    }
    
    /// <summary>
    /// Acknowledges the alert (user has seen and noted it).
    /// </summary>
    public void Acknowledge(Guid userId, string? notes, DateTimeOffset acknowledgedAt)
    {
        if (AcknowledgedAt.HasValue)
            throw new InvalidOperationException("Alert already acknowledged");
            
        AcknowledgedAt = acknowledgedAt;
        AcknowledgedBy = userId;
        AcknowledgmentNotes = notes;
    }

    /// <summary>
    /// Refreshes the current alert details with new telemetry context.
    /// </summary>
    public void Refresh(double? currentValue, double? thresholdValue, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty", nameof(message));

        CurrentValue = currentValue;
        ThresholdValue = thresholdValue;
        Message = message;
    }
    
    /// <summary>
    /// Checks if alert is currently active (fired but not cleared).
    /// </summary>
    public bool IsActive() => !ClearedAt.HasValue;
    
    /// <summary>
    /// Gets the duration the alert has been/was active.
    /// </summary>
    public TimeSpan GetDuration(DateTimeOffset? referenceTime = null)
    {
        var endTime = ClearedAt ?? referenceTime ?? DateTimeOffset.UtcNow;
        return endTime - FiredAt;
    }
    
    /// <summary>
    /// Checks if alert was acknowledged.
    /// </summary>
    public bool IsAcknowledged() => AcknowledgedAt.HasValue;
}
