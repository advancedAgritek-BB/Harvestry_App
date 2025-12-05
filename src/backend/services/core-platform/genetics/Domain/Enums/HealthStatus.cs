namespace Harvestry.Genetics.Domain.Enums;

/// <summary>
/// Health status assessment for mother plants
/// </summary>
public enum HealthStatus
{
    /// <summary>
    /// Excellent health - no issues observed
    /// </summary>
    Excellent = 0,
    
    /// <summary>
    /// Good health - minor issues that are being monitored
    /// </summary>
    Good = 1,
    
    /// <summary>
    /// Fair health - moderate issues requiring attention
    /// </summary>
    Fair = 2,
    
    /// <summary>
    /// Poor health - significant issues requiring intervention
    /// </summary>
    Poor = 3,
    
    /// <summary>
    /// Critical health - immediate intervention required
    /// </summary>
    Critical = 4
}

