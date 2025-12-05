namespace Harvestry.Telemetry.Domain.Enums;

/// <summary>
/// Result of alert rule evaluation.
/// </summary>
public enum EvaluationResult
{
    /// <summary>
    /// Rule condition met - alert triggered
    /// </summary>
    Pass = 1,
    
    /// <summary>
    /// Rule condition not met - no alert
    /// </summary>
    Fail = 2,
    
    /// <summary>
    /// Evaluation error occurred
    /// </summary>
    Error = 3,
    
    /// <summary>
    /// No data available for evaluation
    /// </summary>
    NoData = 4
}

