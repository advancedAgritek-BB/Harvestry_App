namespace Harvestry.Telemetry.Infrastructure.Configuration;

/// <summary>
/// Options controlling PostgreSQL logical replication for telemetry fan-out.
/// </summary>
public sealed class TelemetryWalReplicationOptions
{
    /// <summary>
    /// Enables the WAL fan-out pipeline when set to true.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Connection string used for logical replication. When not supplied the primary telemetry
    /// connection string is used and augmented with replication settings.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Name of the logical replication slot that should be consumed.
    /// </summary>
    public string SlotName { get; set; } = "telemetry_slot";

    /// <summary>
    /// Publication that exposes sensor reading changes.
    /// </summary>
    public string PublicationName { get; set; } = "telemetry_publication";

    /// <summary>
    /// Interval, in seconds, for sending feedback messages to PostgreSQL.
    /// </summary>
    public int StatusIntervalSeconds { get; set; } = 10;

    /// <summary>
    /// Initial retry delay when the replication connection drops.
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 5;

    /// <summary>
    /// Maximum retry delay between reconnection attempts.
    /// </summary>
    public int MaxRetryDelaySeconds { get; set; } = 60;
}
