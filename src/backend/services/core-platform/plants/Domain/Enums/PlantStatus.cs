namespace Harvestry.Plants.Domain.Enums;

/// <summary>
/// Status of an individual tagged plant
/// </summary>
public enum PlantStatus
{
    /// <summary>
    /// Plant is actively growing
    /// </summary>
    Active = 0,

    /// <summary>
    /// Plant is on regulatory hold (pending lab results, investigation, etc.)
    /// </summary>
    OnHold = 1,

    /// <summary>
    /// Plant has been harvested
    /// </summary>
    Harvested = 2,

    /// <summary>
    /// Plant has been destroyed
    /// </summary>
    Destroyed = 3,

    /// <summary>
    /// Plant is no longer in active inventory
    /// </summary>
    Inactive = 4
}









