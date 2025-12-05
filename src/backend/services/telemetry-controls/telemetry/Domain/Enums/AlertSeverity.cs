namespace Harvestry.Telemetry.Domain.Enums;

/// <summary>
/// Severity levels for alerts.
/// </summary>
public enum AlertSeverity
{
    /// <summary>
    /// Informational - no action required
    /// </summary>
    Info = 1,
    
    /// <summary>
    /// Warning - should be investigated
    /// </summary>
    Warning = 2,
    
    /// <summary>
    /// Critical - immediate action required
    /// </summary>
    Critical = 3
}

