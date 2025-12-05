namespace Harvestry.Genetics.Domain.Enums;

/// <summary>
/// Status of a mother plant
/// </summary>
public enum MotherPlantStatus
{
    /// <summary>
    /// Mother plant is active and available for propagation
    /// </summary>
    Active = 0,
    
    /// <summary>
    /// Mother plant is quarantined (pest/disease/health issue)
    /// </summary>
    Quarantine = 1,
    
    /// <summary>
    /// Mother plant has been retired (age, genetic drift, or quality decline)
    /// </summary>
    Retired = 2,
    
    /// <summary>
    /// Mother plant has been destroyed
    /// </summary>
    Destroyed = 3
}

