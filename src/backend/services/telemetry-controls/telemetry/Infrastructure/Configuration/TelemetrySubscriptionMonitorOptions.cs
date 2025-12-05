namespace Harvestry.Telemetry.Infrastructure.Configuration;

/// <summary>
/// Options controlling telemetry subscription monitoring and pruning.
/// </summary>
public sealed class TelemetrySubscriptionMonitorOptions
{
    /// <summary>
    /// When false, the monitoring worker is disabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Interval in seconds between subscription health checks.
    /// </summary>
    public int MonitorIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Connections inactive longer than this window are removed.
    /// </summary>
    public int StaleConnectionSeconds { get; set; } = 120;

    /// <summary>
    /// Number of busiest streams to include in diagnostic logs.
    /// </summary>
    public int TopStreamsToLog { get; set; } = 5;
}
