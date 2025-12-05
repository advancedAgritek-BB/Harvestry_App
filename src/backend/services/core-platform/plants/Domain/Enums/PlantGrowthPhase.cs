namespace Harvestry.Plants.Domain.Enums;

/// <summary>
/// Growth phase of an individual tagged plant (matches METRC plant phases)
/// </summary>
public enum PlantGrowthPhase
{
    /// <summary>
    /// Immature/clone plant not yet tagged individually
    /// </summary>
    Immature = 0,

    /// <summary>
    /// Plant in vegetative growth phase
    /// </summary>
    Vegetative = 1,

    /// <summary>
    /// Plant in flowering phase
    /// </summary>
    Flowering = 2,

    /// <summary>
    /// Mother plant used for propagation
    /// </summary>
    Mother = 3,

    /// <summary>
    /// Plant has been harvested
    /// </summary>
    Harvested = 4,

    /// <summary>
    /// Plant has been destroyed
    /// </summary>
    Destroyed = 5,

    /// <summary>
    /// Plant is inactive (no longer tracked)
    /// </summary>
    Inactive = 6
}



