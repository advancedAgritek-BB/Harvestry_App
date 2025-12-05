namespace Harvestry.Telemetry.Domain.Enums;

/// <summary>
/// Types of sensor streams for telemetry data.
/// Each type represents a different measurement category with specific characteristics.
/// </summary>
public enum StreamType
{
    // Climate sensors
    Temperature = 1,
    Humidity = 2,
    Co2 = 3,
    Vpd = 4,
    LightPar = 5,
    LightPpfd = 6,
    
    // Water quality sensors
    Ec = 10,
    Ph = 11,
    DissolvedOxygen = 12,
    WaterTemp = 13,
    WaterLevel = 14,
    
    // Substrate sensors
    SoilMoisture = 20,
    SoilTemp = 21,
    SoilEc = 22,
    
    // Flow and pressure
    Pressure = 30,
    FlowRate = 31,
    FlowTotal = 32,
    
    // Power and equipment health
    PowerConsumption = 40,
    EnergyConsumption = 41,
    EquipmentStatus = 42,
    
    // Environmental
    Airflow = 50,
    WindSpeed = 51,
    
    // Custom/Other
    Custom = 99
}

