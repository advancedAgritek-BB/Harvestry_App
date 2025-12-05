namespace Harvestry.Plants.Domain.Enums;

/// <summary>
/// Methods for disposing of plant waste (METRC waste methods)
/// </summary>
public enum WasteMethod
{
    /// <summary>
    /// Waste ground/shredded before disposal
    /// </summary>
    Grinder = 0,

    /// <summary>
    /// Waste composted
    /// </summary>
    Compost = 1,

    /// <summary>
    /// Waste incinerated
    /// </summary>
    Incinerator = 2,

    /// <summary>
    /// Mixed with non-cannabis waste and disposed
    /// </summary>
    MixedWaste = 3,

    /// <summary>
    /// Other approved disposal method
    /// </summary>
    Other = 99
}




