namespace Harvestry.Genetics.Domain.Enums;

/// <summary>
/// Level of pest or disease pressure
/// </summary>
public enum PressureLevel
{
    /// <summary>
    /// No pest or disease pressure observed
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Low pressure - minimal presence, easily managed
    /// </summary>
    Low = 1,
    
    /// <summary>
    /// Medium pressure - noticeable presence, requires monitoring
    /// </summary>
    Medium = 2,
    
    /// <summary>
    /// High pressure - significant presence, requires active intervention
    /// </summary>
    High = 3
}

