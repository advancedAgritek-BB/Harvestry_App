namespace Harvestry.Genetics.Domain.Enums;

/// <summary>
/// Type of batch origin
/// </summary>
public enum BatchType
{
    /// <summary>
    /// Started from seed
    /// </summary>
    Seed = 0,
    
    /// <summary>
    /// Started from clone/cutting
    /// </summary>
    Clone = 1,
    
    /// <summary>
    /// Started from tissue culture
    /// </summary>
    TissueCulture = 2,
    
    /// <summary>
    /// Designated mother plant batch
    /// </summary>
    MotherPlant = 3
}

