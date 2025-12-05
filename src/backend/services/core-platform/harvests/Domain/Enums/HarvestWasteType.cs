namespace Harvestry.Harvests.Domain.Enums;

/// <summary>
/// Types of waste recorded during harvest (METRC waste types)
/// </summary>
public enum HarvestWasteType
{
    /// <summary>
    /// Plant material waste (stems, roots, etc.)
    /// </summary>
    PlantMaterial = 0,

    /// <summary>
    /// Fibrous material waste
    /// </summary>
    FibrousMaterial = 1,

    /// <summary>
    /// Trim/leaf waste
    /// </summary>
    Trim = 2,

    /// <summary>
    /// Root waste
    /// </summary>
    Roots = 3,

    /// <summary>
    /// Stem waste
    /// </summary>
    Stems = 4,

    /// <summary>
    /// Other plant waste
    /// </summary>
    Other = 99
}




