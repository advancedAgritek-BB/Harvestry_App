namespace Harvestry.Genetics.Domain.Enums;

/// <summary>
/// Type of genetic classification
/// </summary>
public enum GeneticType
{
    /// <summary>
    /// Indica cannabis strain
    /// </summary>
    Indica = 0,
    
    /// <summary>
    /// Sativa cannabis strain
    /// </summary>
    Sativa = 1,
    
    /// <summary>
    /// Hybrid cannabis strain (indica/sativa cross)
    /// </summary>
    Hybrid = 2,
    
    /// <summary>
    /// Autoflowering strain (automatic flowering independent of light cycle)
    /// </summary>
    Autoflower = 3,
    
    /// <summary>
    /// Hemp strain (high CBD, low THC)
    /// </summary>
    Hemp = 4
}

