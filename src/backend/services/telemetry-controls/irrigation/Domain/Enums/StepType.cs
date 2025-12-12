namespace Harvestry.Irrigation.Domain.Enums;

/// <summary>
/// Types of irrigation program steps
/// </summary>
public enum StepType
{
    /// <summary>
    /// Single shot irrigation (time or volume based)
    /// </summary>
    Shot = 1,

    /// <summary>
    /// Cycle/soak pattern (irrigate, wait, repeat)
    /// </summary>
    CycleSoak = 2,

    /// <summary>
    /// Fresh water flush
    /// </summary>
    Flush = 3,

    /// <summary>
    /// Wait/delay between steps
    /// </summary>
    Wait = 4,

    /// <summary>
    /// Dryback monitoring (sensor-triggered next step)
    /// </summary>
    DrybackMonitor = 5
}
