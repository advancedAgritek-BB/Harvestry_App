namespace Harvestry.Harvests.Domain.Enums;

/// <summary>
/// Defines how a harvest batch is composed
/// </summary>
public enum HarvestBatchingMode
{
    /// <summary>
    /// Batch contains a single strain only
    /// </summary>
    SingleStrain = 0,

    /// <summary>
    /// Batch contains multiple strains blended together
    /// </summary>
    MixedStrain = 1,

    /// <summary>
    /// Batch is a portion (sub-lot) of a larger harvest
    /// </summary>
    SubLot = 2
}




