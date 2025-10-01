namespace Harvestry.Spatial.Domain.Enums;

/// <summary>
/// Core equipment type enumeration (stable for APIs, policies, reporting)
/// Custom types in equipment_type_registry must map to one of these core types
/// </summary>
public enum CoreEquipmentType
{
    /// <summary>
    /// Environmental or irrigation controller
    /// </summary>
    Controller,
    
    /// <summary>
    /// Sensor (temperature, humidity, EC, pH, VWC, CO2, PPFD, etc.)
    /// </summary>
    Sensor,
    
    /// <summary>
    /// Actuator (generic)
    /// </summary>
    Actuator,
    
    /// <summary>
    /// Nutrient injector/dosing pump
    /// </summary>
    Injector,
    
    /// <summary>
    /// Pump (irrigation, recirculation, etc.)
    /// </summary>
    Pump,
    
    /// <summary>
    /// Valve (solenoid, motorized, etc.)
    /// </summary>
    Valve,
    
    /// <summary>
    /// Meter (flow, pressure, etc.)
    /// </summary>
    Meter,
    
    /// <summary>
    /// EC/pH controller (inline control)
    /// </summary>
    EcPhController,
    
    /// <summary>
    /// Mix tank for nutrient preparation
    /// </summary>
    MixTank
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
/// Calibration result enumeration
/// </summary>
public enum CalibrationResult
{
    /// <summary>
    /// Calibration passed all checks
    /// </summary>
    Pass,
    
    /// <summary>
    /// Calibration failed
    /// </summary>
    Fail,
    
    /// <summary>
    /// Reading is within acceptable tolerance
    /// </summary>
    WithinTolerance,
    
    /// <summary>
    /// Reading is outside acceptable tolerance
    /// </summary>
    OutOfTolerance
}
