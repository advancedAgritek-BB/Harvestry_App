namespace Harvestry.Spatial.Domain.Enums;

/// <summary>
/// Core equipment type enumeration (stable for APIs, policies, reporting)
/// Custom types in equipment_type_registry must map to one of these core types
/// </summary>
public enum CoreEquipmentType
{
    // -------------------------------------------------------------------------
    // CONTROLLERS
    // -------------------------------------------------------------------------
    
    /// <summary>
    /// Environmental or irrigation controller (generic)
    /// </summary>
    Controller,
    
    /// <summary>
    /// EC/pH controller (inline fertigation control)
    /// </summary>
    EcPhController,
    
    // -------------------------------------------------------------------------
    // WATER QUALITY SENSORS
    // -------------------------------------------------------------------------
    
    /// <summary>
    /// pH sensor - measures acidity/alkalinity
    /// </summary>
    PhSensor,
    
    /// <summary>
    /// EC sensor - measures electrical conductivity (nutrient strength)
    /// </summary>
    EcSensor,
    
    /// <summary>
    /// Dissolved Oxygen (DO) sensor
    /// </summary>
    DoSensor,
    
    /// <summary>
    /// Oxidation-Reduction Potential (ORP) sensor
    /// </summary>
    OrpSensor,
    
    /// <summary>
    /// Water temperature sensor
    /// </summary>
    WaterTempSensor,
    
    // -------------------------------------------------------------------------
    // ENVIRONMENTAL SENSORS
    // -------------------------------------------------------------------------
    
    /// <summary>
    /// Temperature/Humidity sensor (combined)
    /// </summary>
    TempHumiditySensor,
    
    /// <summary>
    /// CO2 sensor
    /// </summary>
    Co2Sensor,
    
    /// <summary>
    /// Light sensor (PAR/PPFD)
    /// </summary>
    LightSensor,
    
    /// <summary>
    /// Substrate/soil moisture sensor (VWC)
    /// </summary>
    SubstrateSensor,
    
    // -------------------------------------------------------------------------
    // FLOW & LEVEL SENSORS
    // -------------------------------------------------------------------------
    
    /// <summary>
    /// Flow meter (measures flow rate/volume)
    /// </summary>
    FlowMeter,
    
    /// <summary>
    /// Pressure sensor
    /// </summary>
    PressureSensor,
    
    /// <summary>
    /// Level sensor (tank/reservoir level)
    /// </summary>
    LevelSensor,
    
    // -------------------------------------------------------------------------
    // ACTUATORS & PUMPS
    // -------------------------------------------------------------------------
    
    /// <summary>
    /// Valve (solenoid, motorized, etc.)
    /// </summary>
    Valve,
    
    /// <summary>
    /// Pump (irrigation, recirculation, etc.)
    /// </summary>
    Pump,
    
    /// <summary>
    /// Nutrient injector/dosing pump
    /// </summary>
    Injector,
    
    /// <summary>
    /// Actuator (generic)
    /// </summary>
    Actuator,
    
    // -------------------------------------------------------------------------
    // VESSELS & TANKS
    // -------------------------------------------------------------------------
    
    /// <summary>
    /// Mix tank for nutrient preparation
    /// </summary>
    MixTank,
    
    /// <summary>
    /// Reservoir tank
    /// </summary>
    Reservoir,
    
    // -------------------------------------------------------------------------
    // LEGACY/GENERIC (for backward compatibility)
    // -------------------------------------------------------------------------
    
    /// <summary>
    /// Generic sensor (legacy - prefer specific sensor types)
    /// </summary>
    Sensor,
    
    /// <summary>
    /// Generic meter (legacy - prefer FlowMeter, PressureSensor, etc.)
    /// </summary>
    Meter
}

/// <summary>
/// Sensor placement/installation context - describes where a sensor is installed
/// </summary>
public enum SensorPlacement
{
    /// <summary>
    /// In-line installation (in piping/tubing)
    /// </summary>
    Inline,
    
    /// <summary>
    /// Installed in batch/mixing tank
    /// </summary>
    BatchTank,
    
    /// <summary>
    /// Measures runoff/drain water
    /// </summary>
    Runoff,
    
    /// <summary>
    /// Installed in reservoir/storage tank
    /// </summary>
    Reservoir,
    
    /// <summary>
    /// Ambient/environmental measurement
    /// </summary>
    Ambient,
    
    /// <summary>
    /// In substrate/growing media
    /// </summary>
    Substrate
}

/// <summary>
/// Equipment status enumeration
/// </summary>
public enum EquipmentStatus
{
    /// <summary>
    /// Equipment is active and operational
    /// </summary>
    Active,
    
    /// <summary>
    /// Equipment is inactive/not in use
    /// </summary>
    Inactive,
    
    /// <summary>
    /// Equipment is under maintenance
    /// </summary>
    Maintenance,
    
    /// <summary>
    /// Equipment is faulty/needs repair
    /// </summary>
    Faulty
}

/// <summary>
/// Calibration method enumeration
/// </summary>
public enum CalibrationMethod
{
    /// <summary>
    /// Single-point calibration
    /// </summary>
    Single,
    
    /// <summary>
    /// Two-point calibration (common for 4-20mA, pH, EC)
    /// </summary>
    TwoPoint,
    
    /// <summary>
    /// Multi-point calibration curve
    /// </summary>
    MultiPoint
}

/// <summary>
/// Calibration outcome enumeration (overall pass/fail)
/// </summary>
public enum CalibrationOutcome
{
    /// <summary>
    /// Calibration passed all checks
    /// </summary>
    Pass,
    
    /// <summary>
    /// Calibration failed
    /// </summary>
    Fail
}

/// <summary>
/// Tolerance status for calibration readings
/// </summary>
public enum ToleranceStatus
{
    /// <summary>
    /// Reading is within acceptable tolerance
    /// </summary>
    WithinTolerance,
    
    /// <summary>
    /// Reading is outside acceptable tolerance
    /// </summary>
    OutOfTolerance
}
