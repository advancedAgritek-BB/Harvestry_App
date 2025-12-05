namespace Harvestry.Spatial.Domain.Enums;

/// <summary>
/// Outcome of an equipment calibration run.
/// </summary>
public enum CalibrationResult
{
    /// <summary>
    /// Uninitialized/indeterminate calibration result.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Calibration meets all calibration specifications and requirements.
    /// </summary>
    Pass = 1,

    /// <summary>
    /// Calibration is out of spec but within acceptable tolerance thresholds for continued operation.
    /// </summary>
    WithinTolerance = 2,

    /// <summary>
    /// Calibration exceeds tolerance limits and requires immediate attention.
    /// </summary>
    OutOfTolerance = 3,

    /// <summary>
    /// Calibration failed and device requires service before further use.
    /// </summary>
    Fail = 4
}
