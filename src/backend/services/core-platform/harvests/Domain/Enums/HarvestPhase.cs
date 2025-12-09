namespace Harvestry.Harvests.Domain.Enums;

/// <summary>
/// Tracks the current phase of the harvest workflow
/// </summary>
public enum HarvestPhase
{
    /// <summary>
    /// Plants just cut, capturing wet weight
    /// </summary>
    WetHarvest = 0,

    /// <summary>
    /// Plants are in the drying room
    /// </summary>
    Drying = 1,

    /// <summary>
    /// Plants are being bucked (flower separated from stems)
    /// </summary>
    Bucking = 2,

    /// <summary>
    /// Final dry weight has been recorded
    /// </summary>
    DryWeighed = 3,

    /// <summary>
    /// Harvest has been grouped for lot creation
    /// </summary>
    Batched = 4,

    /// <summary>
    /// Inventory lots have been created from this harvest
    /// </summary>
    LotCreated = 5,

    /// <summary>
    /// Harvest workflow is fully complete
    /// </summary>
    Complete = 6
}
