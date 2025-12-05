namespace Harvestry.Telemetry.Domain.Enums;

/// <summary>
/// Types of alert rules for telemetry monitoring.
/// Each type defines a different condition evaluation strategy.
/// </summary>
public enum AlertRuleType
{
    /// <summary>
    /// Alert when value exceeds threshold
    /// </summary>
    ThresholdAbove = 1,
    
    /// <summary>
    /// Alert when value falls below threshold
    /// </summary>
    ThresholdBelow = 2,
    
    /// <summary>
    /// Alert when value is outside specified range
    /// </summary>
    ThresholdRange = 3,
    
    /// <summary>
    /// Alert when value deviates by percentage from baseline
    /// </summary>
    DeviationPercent = 10,
    
    /// <summary>
    /// Alert when value deviates by absolute amount from baseline
    /// </summary>
    DeviationAbsolute = 11,
    
    /// <summary>
    /// Alert when rate of change exceeds limit
    /// </summary>
    RateOfChange = 20,
    
    /// <summary>
    /// Alert when no data received in timeframe
    /// </summary>
    MissingData = 30,
    
    /// <summary>
    /// Alert when quality code indicates degraded readings
    /// </summary>
    QualityDegraded = 40
}

