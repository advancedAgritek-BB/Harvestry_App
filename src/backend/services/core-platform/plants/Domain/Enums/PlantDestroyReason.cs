namespace Harvestry.Plants.Domain.Enums;

/// <summary>
/// Reasons for destroying a plant (METRC waste reasons)
/// </summary>
public enum PlantDestroyReason
{
    /// <summary>
    /// Plant disease or pest infestation
    /// </summary>
    Disease = 0,

    /// <summary>
    /// Plant failed quality inspection
    /// </summary>
    QualityFailure = 1,

    /// <summary>
    /// Regulatory compliance destruction
    /// </summary>
    RegulatoryCompliance = 2,

    /// <summary>
    /// Plant death from natural causes
    /// </summary>
    PlantDeath = 3,

    /// <summary>
    /// Male plant identification and removal
    /// </summary>
    MalePlant = 4,

    /// <summary>
    /// Hermaphrodite plant identification and removal
    /// </summary>
    Hermaphrodite = 5,

    /// <summary>
    /// Contamination (pesticides, mold, etc.)
    /// </summary>
    Contamination = 6,

    /// <summary>
    /// Cultivation culling for space/resource management
    /// </summary>
    Culling = 7,

    /// <summary>
    /// Failed lab test results
    /// </summary>
    FailedLabTest = 8,

    /// <summary>
    /// Other reason (must provide notes)
    /// </summary>
    Other = 99
}









