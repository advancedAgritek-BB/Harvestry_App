namespace Harvestry.Harvests.Domain.Enums;

/// <summary>
/// Status of a harvest record
/// </summary>
public enum HarvestStatus
{
    /// <summary>
    /// Harvest is in progress (accepting waste, creating packages)
    /// </summary>
    Active = 0,

    /// <summary>
    /// Harvest is on regulatory hold
    /// </summary>
    OnHold = 1,

    /// <summary>
    /// Harvest is complete - all weight accounted for in packages/waste
    /// </summary>
    Finished = 2
}









