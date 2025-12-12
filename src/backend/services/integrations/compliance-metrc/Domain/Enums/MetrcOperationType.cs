namespace Harvestry.Compliance.Metrc.Domain.Enums;

/// <summary>
/// Types of operations that can be performed against METRC
/// </summary>
public enum MetrcOperationType
{
    /// <summary>
    /// Create a new entity in METRC
    /// </summary>
    Create = 1,

    /// <summary>
    /// Update an existing entity in METRC
    /// </summary>
    Update = 2,

    /// <summary>
    /// Delete/destroy an entity in METRC
    /// </summary>
    Delete = 3,

    /// <summary>
    /// Move/relocate an entity in METRC
    /// </summary>
    Move = 4,

    /// <summary>
    /// Change growth phase of a plant
    /// </summary>
    ChangePhase = 5,

    /// <summary>
    /// Harvest plants
    /// </summary>
    Harvest = 6,

    /// <summary>
    /// Create packages from harvest
    /// </summary>
    Package = 7,

    /// <summary>
    /// Adjust quantity/weight
    /// </summary>
    Adjust = 8,

    /// <summary>
    /// Finish/close a batch or harvest
    /// </summary>
    Finish = 9,

    /// <summary>
    /// Record waste/destruction
    /// </summary>
    RecordWaste = 10,

    /// <summary>
    /// Remediate a failed lab test
    /// </summary>
    Remediate = 11,

    /// <summary>
    /// Transfer to another facility
    /// </summary>
    Transfer = 12,

    /// <summary>
    /// Read/sync from METRC (no write)
    /// </summary>
    Read = 13
}
