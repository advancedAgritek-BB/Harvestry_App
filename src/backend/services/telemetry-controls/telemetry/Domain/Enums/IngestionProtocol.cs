namespace Harvestry.Telemetry.Domain.Enums;

/// <summary>
/// Protocols supported for telemetry ingestion.
/// </summary>
public enum IngestionProtocol
{
    /// <summary>
    /// MQTT protocol (pub/sub)
    /// </summary>
    Mqtt = 1,
    
    /// <summary>
    /// HTTP POST requests
    /// </summary>
    Http = 2,
    
    /// <summary>
    /// SDI-12 serial protocol
    /// </summary>
    Sdi12 = 3,
    
    /// <summary>
    /// Modbus RTU/TCP
    /// </summary>
    Modbus = 4,
    
    /// <summary>
    /// BACnet (HVAC systems)
    /// </summary>
    BacNet = 5,

    /// <summary>
    /// Internal Simulation
    /// </summary>
    Simulation = 99
}

