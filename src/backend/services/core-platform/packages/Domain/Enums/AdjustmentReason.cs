namespace Harvestry.Packages.Domain.Enums;

/// <summary>
/// Reasons for package adjustments (METRC adjustment reasons)
/// </summary>
public enum AdjustmentReason
{
    /// <summary>
    /// Weight loss due to drying
    /// </summary>
    Drying = 0,

    /// <summary>
    /// Scale variance/calibration
    /// </summary>
    ScaleVariance = 1,

    /// <summary>
    /// Entry error correction
    /// </summary>
    EntryError = 2,

    /// <summary>
    /// Moisture loss
    /// </summary>
    MoistureLoss = 3,

    /// <summary>
    /// Processing loss
    /// </summary>
    ProcessingLoss = 4,

    /// <summary>
    /// Theft
    /// </summary>
    Theft = 5,

    /// <summary>
    /// Audit adjustment
    /// </summary>
    AuditAdjustment = 6,

    /// <summary>
    /// Waste
    /// </summary>
    Waste = 7,

    /// <summary>
    /// Contamination
    /// </summary>
    Contamination = 8,

    /// <summary>
    /// Other (requires note)
    /// </summary>
    Other = 99
}




