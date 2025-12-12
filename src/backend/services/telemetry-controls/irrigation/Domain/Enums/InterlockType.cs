namespace Harvestry.Irrigation.Domain.Enums;

/// <summary>
/// Types of safety interlocks that can prevent or stop irrigation
/// </summary>
public enum InterlockType
{
    /// <summary>
    /// Emergency stop button pressed
    /// </summary>
    EmergencyStop = 1,

    /// <summary>
    /// Access door is open
    /// </summary>
    DoorOpen = 2,

    /// <summary>
    /// Nutrient tank level is too low
    /// </summary>
    TankLevelLow = 3,

    /// <summary>
    /// EC reading is out of bounds
    /// </summary>
    EcOutOfBounds = 4,

    /// <summary>
    /// pH reading is out of bounds
    /// </summary>
    PhOutOfBounds = 5,

    /// <summary>
    /// CO2 enrichment active - irrigation locked out
    /// </summary>
    Co2Lockout = 6,

    /// <summary>
    /// Maximum runtime exceeded
    /// </summary>
    MaxRuntimeExceeded = 7,

    /// <summary>
    /// Maximum concurrent valves would be exceeded
    /// </summary>
    ConcurrencyLimitExceeded = 8,

    /// <summary>
    /// Telemetry data is stale (sensor communication lost)
    /// </summary>
    TelemetryStale = 9,

    /// <summary>
    /// Device communication timeout
    /// </summary>
    DeviceTimeout = 10,

    /// <summary>
    /// Curfew window - irrigation not allowed
    /// </summary>
    CurfewActive = 11,

    /// <summary>
    /// Flow meter detected abnormal flow
    /// </summary>
    FlowAnomaly = 12
}
