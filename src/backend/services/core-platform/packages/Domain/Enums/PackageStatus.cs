namespace Harvestry.Packages.Domain.Enums;

/// <summary>
/// Status of a package (matches METRC package statuses)
/// </summary>
public enum PackageStatus
{
    /// <summary>
    /// Package is active and in inventory
    /// </summary>
    Active = 0,

    /// <summary>
    /// Package is on regulatory hold
    /// </summary>
    OnHold = 1,

    /// <summary>
    /// Package is in transit (transfer)
    /// </summary>
    InTransit = 2,

    /// <summary>
    /// Package is finished (fully consumed/transferred)
    /// </summary>
    Finished = 3,

    /// <summary>
    /// Package is inactive
    /// </summary>
    Inactive = 4
}



