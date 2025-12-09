namespace Harvestry.Harvests.Domain.Enums;

/// <summary>
/// Type of weight measurement in the harvest workflow
/// </summary>
public enum WeightType
{
    /// <summary>
    /// Wet weight of whole plant at harvest
    /// </summary>
    WetPlant = 0,

    /// <summary>
    /// Dry weight of whole plant after drying
    /// </summary>
    DryPlant = 1,

    /// <summary>
    /// Weight of bucked/trimmed flower
    /// </summary>
    BuckedFlower = 2,

    /// <summary>
    /// Weight of stem waste
    /// </summary>
    StemWaste = 3,

    /// <summary>
    /// Weight of leaf/fan leaf waste
    /// </summary>
    LeafWaste = 4,

    /// <summary>
    /// Weight of other waste material
    /// </summary>
    OtherWaste = 5
}
