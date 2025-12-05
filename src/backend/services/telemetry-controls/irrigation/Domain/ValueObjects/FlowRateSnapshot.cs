namespace Harvestry.Irrigation.Domain.ValueObjects;

/// <summary>
/// Value object representing the current flow rate state of the system
/// </summary>
public sealed record FlowRateSnapshot
{
    public FlowRateSnapshot(
        decimal currentFlowRateLitersPerMinute,
        decimal maxFlowRateLitersPerMinute,
        decimal effectiveMaxFlowRateLitersPerMinute,
        IReadOnlyList<ActiveZoneFlow> activeZoneFlows)
    {
        CurrentFlowRateLitersPerMinute = currentFlowRateLitersPerMinute;
        MaxFlowRateLitersPerMinute = maxFlowRateLitersPerMinute;
        EffectiveMaxFlowRateLitersPerMinute = effectiveMaxFlowRateLitersPerMinute;
        ActiveZoneFlows = activeZoneFlows;
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Current total flow rate across all active zones in L/min
    /// </summary>
    public decimal CurrentFlowRateLitersPerMinute { get; }
    
    /// <summary>
    /// Maximum system flow rate in L/min
    /// </summary>
    public decimal MaxFlowRateLitersPerMinute { get; }
    
    /// <summary>
    /// Effective max (with safety margin applied) in L/min
    /// </summary>
    public decimal EffectiveMaxFlowRateLitersPerMinute { get; }
    
    /// <summary>
    /// List of currently active zone flows
    /// </summary>
    public IReadOnlyList<ActiveZoneFlow> ActiveZoneFlows { get; }
    
    /// <summary>
    /// When this snapshot was taken
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// Available flow rate capacity in L/min
    /// </summary>
    public decimal AvailableFlowRateLitersPerMinute =>
        Math.Max(0, EffectiveMaxFlowRateLitersPerMinute - CurrentFlowRateLitersPerMinute);

    /// <summary>
    /// Current utilization as a percentage
    /// </summary>
    public decimal UtilizationPercent =>
        MaxFlowRateLitersPerMinute > 0
            ? CurrentFlowRateLitersPerMinute / MaxFlowRateLitersPerMinute * 100
            : 0;

    /// <summary>
    /// Check if adding a zone with the given flow rate would exceed limits
    /// </summary>
    public bool WouldExceedLimit(decimal additionalFlowRateLitersPerMinute)
    {
        return CurrentFlowRateLitersPerMinute + additionalFlowRateLitersPerMinute > EffectiveMaxFlowRateLitersPerMinute;
    }

    /// <summary>
    /// Calculate the flow rate that would exceed the limit
    /// </summary>
    public decimal ExcessFlowRate(decimal additionalFlowRateLitersPerMinute)
    {
        var total = CurrentFlowRateLitersPerMinute + additionalFlowRateLitersPerMinute;
        return Math.Max(0, total - EffectiveMaxFlowRateLitersPerMinute);
    }
}

/// <summary>
/// Represents an active zone's flow contribution
/// </summary>
public sealed record ActiveZoneFlow(
    Guid ZoneId,
    string ZoneName,
    decimal FlowRateLitersPerMinute,
    DateTime StartedAt,
    DateTime? ExpectedEndAt);



