namespace Harvestry.Telemetry.Domain.Enums;

/// <summary>
/// Units of measurement for sensor readings.
/// The normalization service will convert readings to canonical units for consistent storage.
/// </summary>
public enum Unit
{
    // Temperature
    DegreesFahrenheit = 1,
    DegreesCelsius = 2,
    Kelvin = 3,
    
    // Percentage
    Percent = 10,
    
    // Concentration
    PartsPerMillion = 20,
    PartsPerBillion = 21,
    MilligramsPerLiter = 22,
    
    // Pressure
    Kilopascals = 30,
    Psi = 31,
    Bar = 32,
    Pascals = 33,
    
    // Light
    Micromoles = 40,
    Lux = 41,
    Footcandles = 42,
    
    // Electrical Conductivity
    Microsiemens = 50,
    MillisiemensPerCm = 51,
    
    // pH (dimensionless)
    Ph = 60,
    
    // Volume
    Liters = 70,
    Gallons = 71,
    Milliliters = 72,
    CubicMeters = 73,
    CubicFeet = 74,
    
    // Flow Rate
    LitersPerMinute = 80,
    GallonsPerMinute = 81,
    LitersPerHour = 82,
    GallonsPerHour = 83,
    CubicMetersPerHour = 84,
    
    // Distance/Length
    Inches = 90,
    Centimeters = 91,
    Meters = 92,
    Feet = 93,
    
    // Power
    Watts = 100,
    Kilowatts = 101,
    Horsepower = 102,
    
    // Energy
    KilowattHours = 110,
    WattHours = 111,
    Joules = 112,
    
    // Speed
    MetersPerSecond = 120,
    FeetPerSecond = 121,
    MilesPerHour = 122,
    KilometersPerHour = 123,
    
    // Dimensionless/Status
    Boolean = 200,
    Count = 201,
    Ratio = 202
}

