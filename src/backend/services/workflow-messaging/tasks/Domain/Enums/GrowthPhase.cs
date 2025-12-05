namespace Harvestry.Tasks.Domain.Enums;

/// <summary>
/// Growth phase for task blueprint matching (mirrors PlantGrowthPhase for loose coupling).
/// </summary>
public enum GrowthPhase
{
    /// <summary>
    /// Any phase - wildcard for matching.
    /// </summary>
    Any = 0,

    /// <summary>
    /// Immature/clone plant not yet tagged individually.
    /// </summary>
    Immature = 1,

    /// <summary>
    /// Plant in vegetative growth phase.
    /// </summary>
    Vegetative = 2,

    /// <summary>
    /// Plant in flowering phase.
    /// </summary>
    Flowering = 3,

    /// <summary>
    /// Mother plant used for propagation.
    /// </summary>
    Mother = 4,

    /// <summary>
    /// Plant has been harvested.
    /// </summary>
    Harvested = 5,

    /// <summary>
    /// Drying phase.
    /// </summary>
    Drying = 6,

    /// <summary>
    /// Curing phase.
    /// </summary>
    Curing = 7
}

