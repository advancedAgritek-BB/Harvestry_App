namespace Harvestry.Compliance.Metrc.Domain.Enums;

/// <summary>
/// Types of entities that can be synchronized with METRC
/// </summary>
public enum MetrcEntityType
{
    /// <summary>
    /// Cultivation facility/license
    /// </summary>
    Facility = 1,

    /// <summary>
    /// Physical location within a facility
    /// </summary>
    Location = 2,

    /// <summary>
    /// Cannabis strain/cultivar
    /// </summary>
    Strain = 3,

    /// <summary>
    /// Plant batch (group of seeds/clones planted together)
    /// </summary>
    PlantBatch = 4,

    /// <summary>
    /// Individual immature plant
    /// </summary>
    ImmaturePlant = 5,

    /// <summary>
    /// Individual flowering/vegetative plant
    /// </summary>
    Plant = 6,

    /// <summary>
    /// Harvest batch
    /// </summary>
    Harvest = 7,

    /// <summary>
    /// Package of cannabis product
    /// </summary>
    Package = 8,

    /// <summary>
    /// Item/product type definition
    /// </summary>
    Item = 9,

    /// <summary>
    /// Lab test result
    /// </summary>
    LabTest = 10,

    /// <summary>
    /// Processing/manufacturing job
    /// </summary>
    ProcessingJob = 11,

    /// <summary>
    /// Transfer/shipment
    /// </summary>
    Transfer = 12,

    /// <summary>
    /// Waste/destruction event
    /// </summary>
    Waste = 13,

    /// <summary>
    /// Adjustment/remediation
    /// </summary>
    Adjustment = 14
}
