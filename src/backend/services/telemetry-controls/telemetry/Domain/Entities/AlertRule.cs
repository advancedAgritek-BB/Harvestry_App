using Harvestry.Telemetry.Domain.Enums;
using Harvestry.Telemetry.Domain.ValueObjects;

namespace Harvestry.Telemetry.Domain.Entities;

/// <summary>
/// Represents an alert rule for monitoring sensor streams.
/// Aggregate root for alert configuration and evaluation logic.
/// </summary>
public class AlertRule : AggregateRoot<Guid>
{
    public Guid SiteId { get; private set; }
    public string RuleName { get; private set; } = string.Empty;
    public AlertRuleType RuleType { get; private set; }
    public List<Guid> StreamIds { get; private set; } = new();
    public ThresholdConfig ThresholdConfig { get; private set; }
    public int EvaluationWindowMinutes { get; private set; }
    public int CooldownMinutes { get; private set; }
    public AlertSeverity Severity { get; private set; }
    public bool IsActive { get; private set; }
    public List<string> NotifyChannels { get; private set; } = new();
    public Dictionary<string, object>? Metadata { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public Guid UpdatedBy { get; private set; }
    
    // For EF Core
    private AlertRule() { }
    
    private AlertRule(
        Guid id,
        Guid siteId,
        string ruleName,
        AlertRuleType ruleType,
        ThresholdConfig thresholdConfig,
        Guid createdBy)
    {
        if (!thresholdConfig.Validate())
            throw new ArgumentException("Invalid threshold configuration for rule type", nameof(thresholdConfig));
            
        Id = id;
        SiteId = siteId;
        RuleName = ruleName;
        RuleType = ruleType;
        ThresholdConfig = thresholdConfig;
        EvaluationWindowMinutes = 5;
        CooldownMinutes = 15;
        Severity = AlertSeverity.Warning;
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        CreatedBy = createdBy;
        UpdatedBy = createdBy;
    }
    
    /// <summary>
    /// Creates a new alert rule.
    /// </summary>
    public static AlertRule Create(
        Guid siteId,
        string ruleName,
        AlertRuleType ruleType,
        ThresholdConfig thresholdConfig,
        List<Guid> streamIds,
        Guid createdBy,
        int evaluationWindowMinutes = 5,
        int cooldownMinutes = 15,
        AlertSeverity severity = AlertSeverity.Warning,
        List<string>? notifyChannels = null)
    {
        if (string.IsNullOrWhiteSpace(ruleName))
            throw new ArgumentException("Rule name is required", nameof(ruleName));
            
        if (streamIds == null || streamIds.Count == 0)
            throw new ArgumentException("At least one stream ID is required", nameof(streamIds));
            
        var rule = new AlertRule(Guid.NewGuid(), siteId, ruleName, ruleType, thresholdConfig, createdBy)
        {
            StreamIds = streamIds,
            EvaluationWindowMinutes = evaluationWindowMinutes,
            CooldownMinutes = cooldownMinutes,
            Severity = severity,
            NotifyChannels = notifyChannels ?? new List<string>()
        };
        
        return rule;
    }
    
    /// <summary>
    /// Rehydrates alert rule from persistence layer.
    /// </summary>
    public static AlertRule FromPersistence(
        Guid id,
        Guid siteId,
        string ruleName,
        AlertRuleType ruleType,
        List<Guid> streamIds,
        ThresholdConfig thresholdConfig,
        int evaluationWindowMinutes,
        int cooldownMinutes,
        AlertSeverity severity,
        bool isActive,
        List<string> notifyChannels,
        Dictionary<string, object>? metadata,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        Guid createdBy,
        Guid updatedBy)
    {
        return new AlertRule
        {
            Id = id,
            SiteId = siteId,
            RuleName = ruleName,
            RuleType = ruleType,
            StreamIds = streamIds,
            ThresholdConfig = thresholdConfig,
            EvaluationWindowMinutes = evaluationWindowMinutes,
            CooldownMinutes = cooldownMinutes,
            Severity = severity,
            IsActive = isActive,
            NotifyChannels = notifyChannels,
            Metadata = metadata,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            CreatedBy = createdBy,
            UpdatedBy = updatedBy
        };
    }
    
    /// <summary>
    /// Evaluates the rule against sensor readings.
    /// Returns result indicating whether alert should fire.
    /// </summary>
    public AlertRuleResult Evaluate(IReadOnlyCollection<SensorReading> readings, DateTimeOffset evaluationTime)
    {
        if (readings == null || readings.Count == 0)
            return AlertRuleResult.NoData();
            
        // Filter to good quality readings only
        var goodReadings = readings.Where(r => r.IsGoodQuality()).ToList();
        
        if (goodReadings.Count == 0)
            return AlertRuleResult.NoData();
            
        return RuleType switch
        {
            AlertRuleType.ThresholdAbove => EvaluateThresholdAbove(goodReadings),
            AlertRuleType.ThresholdBelow => EvaluateThresholdBelow(goodReadings),
            AlertRuleType.ThresholdRange => EvaluateThresholdRange(goodReadings),
            AlertRuleType.DeviationPercent => EvaluateDeviationPercent(goodReadings),
            AlertRuleType.DeviationAbsolute => EvaluateDeviationAbsolute(goodReadings),
            AlertRuleType.RateOfChange => EvaluateRateOfChange(goodReadings),
            _ => AlertRuleResult.Error($"Unsupported rule type: {RuleType}")
        };
    }
    
    private AlertRuleResult EvaluateThresholdAbove(List<SensorReading> readings)
    {
        var avgValue = readings.Average(r => r.Value);
        var threshold = ThresholdConfig.ThresholdValue!.Value;
        
        if (avgValue > threshold)
        {
            return AlertRuleResult.Pass(
                avgValue,
                threshold,
                readings.Count,
                $"{RuleName}: Average value {avgValue:F2} exceeds threshold {threshold:F2}");
        }
        
        return AlertRuleResult.Fail(avgValue, readings.Count);
    }
    
    private AlertRuleResult EvaluateThresholdBelow(List<SensorReading> readings)
    {
        var avgValue = readings.Average(r => r.Value);
        var threshold = ThresholdConfig.ThresholdValue!.Value;
        
        if (avgValue < threshold)
        {
            return AlertRuleResult.Pass(
                avgValue,
                threshold,
                readings.Count,
                $"{RuleName}: Average value {avgValue:F2} below threshold {threshold:F2}");
        }
        
        return AlertRuleResult.Fail(avgValue, readings.Count);
    }
    
    private AlertRuleResult EvaluateThresholdRange(List<SensorReading> readings)
    {
        var avgValue = readings.Average(r => r.Value);
        var min = ThresholdConfig.MinValue!.Value;
        var max = ThresholdConfig.MaxValue!.Value;
        
        if (avgValue < min || avgValue > max)
        {
            return AlertRuleResult.Pass(
                avgValue,
                avgValue < min ? min : max,
                readings.Count,
                $"{RuleName}: Average value {avgValue:F2} outside range [{min:F2}, {max:F2}]");
        }
        
        return AlertRuleResult.Fail(avgValue, readings.Count);
    }
    
    private AlertRuleResult EvaluateDeviationPercent(List<SensorReading> readings)
    {
        var avgValue = readings.Average(r => r.Value);
        var baseline = ThresholdConfig.BaselineValue ?? avgValue;
        var deviationPercent = Math.Abs((avgValue - baseline) / baseline * 100.0);
        var threshold = ThresholdConfig.DeviationPercent!.Value;
        
        if (deviationPercent > threshold)
        {
            return AlertRuleResult.Pass(
                avgValue,
                baseline,
                readings.Count,
                $"{RuleName}: Deviation {deviationPercent:F1}% exceeds threshold {threshold:F1}%");
        }
        
        return AlertRuleResult.Fail(avgValue, readings.Count);
    }
    
    private AlertRuleResult EvaluateDeviationAbsolute(List<SensorReading> readings)
    {
        var avgValue = readings.Average(r => r.Value);
        var baseline = ThresholdConfig.BaselineValue ?? avgValue;
        var deviation = Math.Abs(avgValue - baseline);
        var threshold = ThresholdConfig.DeviationAbsolute!.Value;
        
        if (deviation > threshold)
        {
            return AlertRuleResult.Pass(
                avgValue,
                baseline,
                readings.Count,
                $"{RuleName}: Deviation {deviation:F2} exceeds threshold {threshold:F2}");
        }
        
        return AlertRuleResult.Fail(avgValue, readings.Count);
    }
    
    private AlertRuleResult EvaluateRateOfChange(List<SensorReading> readings)
    {
        var orderedReadings = readings.OrderBy(r => r.Time).ToList();
        
        if (orderedReadings.Count < 2)
            return AlertRuleResult.NoData();
            
        var first = orderedReadings.First();
        var last = orderedReadings.Last();
        var timeDiffMinutes = (last.Time - first.Time).TotalMinutes;
        
        if (timeDiffMinutes == 0)
            return AlertRuleResult.NoData();
            
        var rateOfChange = Math.Abs(last.Value - first.Value) / timeDiffMinutes;
        var threshold = ThresholdConfig.RateOfChangePerMinute!.Value;
        
        if (rateOfChange > threshold)
        {
            return AlertRuleResult.Pass(
                rateOfChange,
                threshold,
                readings.Count,
                $"{RuleName}: Rate of change {rateOfChange:F2}/min exceeds threshold {threshold:F2}/min");
        }
        
        return AlertRuleResult.Fail(rateOfChange, readings.Count);
    }
    
    /// <summary>
    /// Checks if rule is in cooldown period since last alert.
    /// </summary>
    public bool IsInCooldown(DateTimeOffset lastFiredAt, DateTimeOffset now)
    {
        var timeSinceLastAlert = now - lastFiredAt;
        return timeSinceLastAlert.TotalMinutes < CooldownMinutes;
    }
    
    public void UpdateThreshold(ThresholdConfig newConfig, Guid updatedBy)
    {
        if (!newConfig.Validate())
            throw new ArgumentException("Invalid threshold configuration", nameof(newConfig));
            
        ThresholdConfig = newConfig;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Updates mutable rule details such as name, streams, evaluation window, cooldown, severity, and notification channels.
    /// </summary>
    public void UpdateDetails(
        string? ruleName,
        List<Guid>? streamIds,
        int? evaluationWindowMinutes,
        int? cooldownMinutes,
        AlertSeverity? severity,
        List<string>? notifyChannels,
        Guid updatedBy)
    {
        if (ruleName != null)
        {
            if (string.IsNullOrWhiteSpace(ruleName))
                throw new ArgumentException("Rule name cannot be empty", nameof(ruleName));

            RuleName = ruleName;
        }

        if (streamIds != null)
        {
            if (streamIds.Count == 0)
                throw new ArgumentException("At least one stream id is required", nameof(streamIds));

            if (streamIds.Any(id => id == Guid.Empty))
                throw new ArgumentException("Stream ids cannot contain empty GUIDs", nameof(streamIds));

            StreamIds = new List<Guid>(streamIds);
        }

        if (evaluationWindowMinutes.HasValue)
        {
            if (evaluationWindowMinutes.Value <= 0)
                throw new ArgumentOutOfRangeException(nameof(evaluationWindowMinutes), "Evaluation window must be positive");

            EvaluationWindowMinutes = evaluationWindowMinutes.Value;
        }

        if (cooldownMinutes.HasValue)
        {
            if (cooldownMinutes.Value <= 0)
                throw new ArgumentOutOfRangeException(nameof(cooldownMinutes), "Cooldown must be positive");

            CooldownMinutes = cooldownMinutes.Value;
        }

        if (severity.HasValue)
        {
            Severity = severity.Value;
        }

        if (notifyChannels != null)
        {
            if (notifyChannels.Count == 0 || notifyChannels.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("Notification channels must contain at least one non-empty, non-whitespace channel", nameof(notifyChannels));

            NotifyChannels = new List<string>(notifyChannels);
        }

        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    
    public void Activate(Guid updatedBy)
    {
        IsActive = true;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    
    public void Deactivate(Guid updatedBy)
    {
        IsActive = false;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
