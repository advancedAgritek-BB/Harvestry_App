namespace Harvestry.Telemetry.Domain.Enums;

/// <summary>
/// Time intervals for continuous aggregate rollups.
/// Corresponds to TimescaleDB continuous aggregates.
/// </summary>
public enum RollupInterval
{
    /// <summary>
    /// Raw data (no aggregation)
    /// </summary>
    Raw = 0,
    
    /// <summary>
    /// 1-minute rollup (for recent detailed view)
    /// </summary>
    OneMinute = 1,
    
    /// <summary>
    /// 5-minute rollup (for hourly/daily views)
    /// </summary>
    FiveMinutes = 5,
    
    /// <summary>
    /// 1-hour rollup (for weekly/monthly views)
    /// </summary>
    OneHour = 60
}

