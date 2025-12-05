using Harvestry.Telemetry.Domain.Enums;

namespace Harvestry.Telemetry.Domain.ValueObjects;

/// <summary>
/// Configuration for alert rule thresholds and conditions.
/// Supports multiple alert rule types with type-specific parameters.
/// </summary>
public readonly record struct ThresholdConfig
{
    public AlertRuleType RuleType { get; init; }
    
    // Threshold values
    public double? ThresholdValue { get; init; }
    public double? MinValue { get; init; }
    public double? MaxValue { get; init; }
    
    // Deviation parameters
    public double? DeviationPercent { get; init; }
    public double? DeviationAbsolute { get; init; }
    public double? BaselineValue { get; init; }
    
    // Rate of change parameters
    public double? RateOfChangePerMinute { get; init; }
    
    // Missing data parameters
    public int? MissingDataMinutes { get; init; }
    
    // Quality parameters
    public QualityCode? MinimumQualityCode { get; init; }
    
    /// <summary>
    /// Validates that configuration is appropriate for the rule type.
    /// </summary>
    public bool Validate()
    {
        return RuleType switch
        {
            AlertRuleType.ThresholdAbove => ThresholdValue.HasValue,
            AlertRuleType.ThresholdBelow => ThresholdValue.HasValue,
            AlertRuleType.ThresholdRange => MinValue.HasValue && MaxValue.HasValue && MinValue < MaxValue,
            AlertRuleType.DeviationPercent => DeviationPercent.HasValue && DeviationPercent > 0,
            AlertRuleType.DeviationAbsolute => DeviationAbsolute.HasValue && DeviationAbsolute > 0,
            AlertRuleType.RateOfChange => RateOfChangePerMinute.HasValue,
            AlertRuleType.MissingData => MissingDataMinutes.HasValue && MissingDataMinutes > 0,
            AlertRuleType.QualityDegraded => MinimumQualityCode.HasValue,
            _ => false
        };
    }
    
    /// <summary>
    /// Gets a human-readable description of the threshold configuration.
    /// </summary>
    public string GetDescription()
    {
        return RuleType switch
        {
            AlertRuleType.ThresholdAbove => $"Value above {ThresholdValue}",
            AlertRuleType.ThresholdBelow => $"Value below {ThresholdValue}",
            AlertRuleType.ThresholdRange => $"Value outside range {MinValue} to {MaxValue}",
            AlertRuleType.DeviationPercent => $"Deviation > {DeviationPercent}% from baseline",
            AlertRuleType.DeviationAbsolute => $"Deviation > {DeviationAbsolute} from baseline",
            AlertRuleType.RateOfChange => $"Rate of change > {RateOfChangePerMinute}/min",
            AlertRuleType.MissingData => $"No data for {MissingDataMinutes} minutes",
            AlertRuleType.QualityDegraded => $"Quality worse than {MinimumQualityCode}",
            _ => "Unknown rule type"
        };
    }
}

