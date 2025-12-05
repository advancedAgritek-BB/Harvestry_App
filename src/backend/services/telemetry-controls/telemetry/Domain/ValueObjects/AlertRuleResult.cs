using Harvestry.Telemetry.Domain.Enums;

namespace Harvestry.Telemetry.Domain.ValueObjects;

/// <summary>
/// Result of evaluating an alert rule against sensor data.
/// </summary>
public readonly record struct AlertRuleResult
{
    public EvaluationResult Result { get; init; }
    public bool ShouldFireAlert { get; init; }
    public bool ShouldClearAlert { get; init; }
    public double? CurrentValue { get; init; }
    public double? ThresholdValue { get; init; }
    public int SampleCount { get; init; }
    public string? Message { get; init; }
    public string? ErrorMessage { get; init; }
    
    public static AlertRuleResult Pass(double currentValue, double thresholdValue, int sampleCount, string message)
    {
        return new AlertRuleResult
        {
            Result = EvaluationResult.Pass,
            ShouldFireAlert = true,
            ShouldClearAlert = false,
            CurrentValue = currentValue,
            ThresholdValue = thresholdValue,
            SampleCount = sampleCount,
            Message = message
        };
    }
    
    public static AlertRuleResult Fail(double? currentValue, int sampleCount)
    {
        return new AlertRuleResult
        {
            Result = EvaluationResult.Fail,
            ShouldFireAlert = false,
            ShouldClearAlert = true,
            CurrentValue = currentValue,
            SampleCount = sampleCount,
            Message = "Condition not met"
        };
    }
    
    public static AlertRuleResult NoData()
    {
        return new AlertRuleResult
        {
            Result = EvaluationResult.NoData,
            ShouldFireAlert = false,
            ShouldClearAlert = false,
            SampleCount = 0,
            Message = "No data available for evaluation"
        };
    }
    
    public static AlertRuleResult Error(string errorMessage)
    {
        return new AlertRuleResult
        {
            Result = EvaluationResult.Error,
            ShouldFireAlert = false,
            ShouldClearAlert = false,
            ErrorMessage = errorMessage,
            Message = "Evaluation error"
        };
    }
}

