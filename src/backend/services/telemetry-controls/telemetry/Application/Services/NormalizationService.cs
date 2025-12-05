using Harvestry.Telemetry.Application.DTOs;
using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Domain.Entities;
using Harvestry.Telemetry.Domain.Enums;

namespace Harvestry.Telemetry.Application.Services;

/// <summary>
/// Service for normalizing sensor values and units.
/// Converts readings to canonical units for consistent storage.
/// </summary>
public class NormalizationService : INormalizationService
{
    // Canonical units for each stream type (standardized for storage)
    private static readonly Dictionary<StreamType, Unit> CanonicalUnits = new()
    {
        { StreamType.Temperature, Unit.DegreesFahrenheit },
        { StreamType.Humidity, Unit.Percent },
        { StreamType.Co2, Unit.PartsPerMillion },
        { StreamType.Vpd, Unit.Kilopascals },
        { StreamType.LightPar, Unit.Micromoles },
        { StreamType.LightPpfd, Unit.Micromoles },
        { StreamType.Ec, Unit.Microsiemens },
        { StreamType.Ph, Unit.Ph },
        { StreamType.DissolvedOxygen, Unit.MilligramsPerLiter },
        { StreamType.WaterTemp, Unit.DegreesFahrenheit },
        { StreamType.WaterLevel, Unit.Liters },
        { StreamType.SoilMoisture, Unit.Percent },
        { StreamType.SoilTemp, Unit.DegreesFahrenheit },
        { StreamType.SoilEc, Unit.Microsiemens },
        { StreamType.Pressure, Unit.Psi },
        { StreamType.FlowRate, Unit.GallonsPerMinute },
        { StreamType.FlowTotal, Unit.Gallons },
        { StreamType.PowerConsumption, Unit.Watts },
        { StreamType.EnergyConsumption, Unit.KilowattHours }
    };
    
    public async Task<SensorReading> NormalizeAsync(
        SensorReadingDto dto,
        SensorStream stream,
        CancellationToken cancellationToken = default)
    {
        // Determine quality code based on validation
        var qualityCode = DetermineQualityCode(
            dto.Value,
            dto.Unit,
            stream.StreamType,
            dto.SourceTimestamp);
        
        // Normalize value to canonical unit
        var (normalizedValue, targetUnit) = NormalizeReading(
            dto.Value,
            dto.Unit,
            stream.StreamType);
        
        // Create sensor reading entity
        var reading = SensorReading.FromIngestion(
            streamId: dto.StreamId,
            value: normalizedValue,
            qualityCode: qualityCode,
            sourceTimestamp: dto.SourceTimestamp,
            messageId: dto.MessageId,
            metadata: null); // TODO: Parse JSON metadata if needed
        
        return await Task.FromResult(reading);
    }
    
    // Expected ranges for validation (min, max)
    private static readonly Dictionary<StreamType, (double min, double max)> ExpectedRanges = new()
    {
        { StreamType.Temperature, (-50, 150) },      // Fahrenheit
        { StreamType.Humidity, (0, 100) },           // Percent
        { StreamType.Co2, (0, 5000) },               // PPM
        { StreamType.Vpd, (0, 5) },                  // kPa
        { StreamType.LightPar, (0, 3000) },          // μmol
        { StreamType.LightPpfd, (0, 3000) },         // μmol
        { StreamType.Ec, (0, 10000) },               // μS
        { StreamType.Ph, (0, 14) },                  // pH
        { StreamType.DissolvedOxygen, (0, 50) },     // mg/L
        { StreamType.WaterTemp, (32, 120) },         // Fahrenheit
        { StreamType.WaterLevel, (0, 10000) },       // Liters
        { StreamType.SoilMoisture, (0, 100) },       // Percent
        { StreamType.SoilTemp, (32, 120) },          // Fahrenheit
        { StreamType.SoilEc, (0, 10000) },           // μS
        { StreamType.Pressure, (0, 200) },           // PSI
        { StreamType.FlowRate, (0, 1000) },          // GPM
        { StreamType.FlowTotal, (0, 1000000) },      // Gallons
        { StreamType.PowerConsumption, (0, 100000) }, // Watts
        { StreamType.EnergyConsumption, (0, 100000) } // kWh
    };
    
    public double ConvertUnit(double value, Unit sourceUnit, Unit targetUnit)
    {
        if (sourceUnit == targetUnit)
            return value;
            
        // Temperature conversions
        if (sourceUnit == Unit.DegreesFahrenheit && targetUnit == Unit.DegreesCelsius)
            return (value - 32) * 5 / 9;
        if (sourceUnit == Unit.DegreesCelsius && targetUnit == Unit.DegreesFahrenheit)
            return (value * 9 / 5) + 32;
        if (sourceUnit == Unit.DegreesCelsius && targetUnit == Unit.Kelvin)
            return value + 273.15;
        if (sourceUnit == Unit.Kelvin && targetUnit == Unit.DegreesCelsius)
            return value - 273.15;
            
        // Pressure conversions
        if (sourceUnit == Unit.Psi && targetUnit == Unit.Kilopascals)
            return value * 6.89476;
        if (sourceUnit == Unit.Kilopascals && targetUnit == Unit.Psi)
            return value / 6.89476;
        if (sourceUnit == Unit.Bar && targetUnit == Unit.Psi)
            return value * 14.5038;
        if (sourceUnit == Unit.Psi && targetUnit == Unit.Bar)
            return value / 14.5038;
        if (sourceUnit == Unit.Bar && targetUnit == Unit.Kilopascals)
            return value * 100;
        if (sourceUnit == Unit.Kilopascals && targetUnit == Unit.Bar)
            return value / 100;
            
        // Volume conversions
        if (sourceUnit == Unit.Gallons && targetUnit == Unit.Liters)
            return value * 3.78541;
        if (sourceUnit == Unit.Liters && targetUnit == Unit.Gallons)
            return value / 3.78541;
        if (sourceUnit == Unit.Milliliters && targetUnit == Unit.Liters)
            return value / 1000;
        if (sourceUnit == Unit.Liters && targetUnit == Unit.Milliliters)
            return value * 1000;
            
        // Flow rate conversions
        if (sourceUnit == Unit.GallonsPerMinute && targetUnit == Unit.LitersPerMinute)
            return value * 3.78541;
        if (sourceUnit == Unit.LitersPerMinute && targetUnit == Unit.GallonsPerMinute)
            return value / 3.78541;
        if (sourceUnit == Unit.GallonsPerHour && targetUnit == Unit.LitersPerHour)
            return value * 3.78541;
        if (sourceUnit == Unit.LitersPerHour && targetUnit == Unit.GallonsPerHour)
            return value / 3.78541;
            
        // Distance conversions
        if (sourceUnit == Unit.Inches && targetUnit == Unit.Centimeters)
            return value * 2.54;
        if (sourceUnit == Unit.Centimeters && targetUnit == Unit.Inches)
            return value / 2.54;
        if (sourceUnit == Unit.Feet && targetUnit == Unit.Meters)
            return value * 0.3048;
        if (sourceUnit == Unit.Meters && targetUnit == Unit.Feet)
            return value / 0.3048;
            
        // Power conversions
        if (sourceUnit == Unit.Kilowatts && targetUnit == Unit.Watts)
            return value * 1000;
        if (sourceUnit == Unit.Watts && targetUnit == Unit.Kilowatts)
            return value / 1000;
        if (sourceUnit == Unit.Horsepower && targetUnit == Unit.Watts)
            return value * 745.7;
        if (sourceUnit == Unit.Watts && targetUnit == Unit.Horsepower)
            return value / 745.7;
            
        // Energy conversions
        if (sourceUnit == Unit.KilowattHours && targetUnit == Unit.WattHours)
            return value * 1000;
        if (sourceUnit == Unit.WattHours && targetUnit == Unit.KilowattHours)
            return value / 1000;
            
        // EC conversions
        if (sourceUnit == Unit.MillisiemensPerCm && targetUnit == Unit.Microsiemens)
            return value * 1000;
        if (sourceUnit == Unit.Microsiemens && targetUnit == Unit.MillisiemensPerCm)
            return value / 1000;
            
        // If no conversion found, throw exception
        throw new NotSupportedException($"Conversion from {sourceUnit} to {targetUnit} is not supported");
    }
    
    public bool ValidateReading(double value, Unit unit, StreamType streamType)
    {
        // Check for NaN or Infinity
        if (double.IsNaN(value) || double.IsInfinity(value))
            return false;
            
        // Get expected range for stream type
        if (!ExpectedRanges.TryGetValue(streamType, out var range))
            return true; // No validation range defined, assume valid
            
        // Convert value to canonical unit for range checking
        var canonicalUnit = GetCanonicalUnit(streamType);
        var normalizedValue = unit == canonicalUnit 
            ? value 
            : ConvertUnit(value, unit, canonicalUnit);
            
        return normalizedValue >= range.min && normalizedValue <= range.max;
    }
    
    public QualityCode DetermineQualityCode(
        double value,
        Unit unit,
        StreamType streamType,
        DateTimeOffset? sourceTimestamp)
    {
        // Check for invalid numeric values
        if (double.IsNaN(value) || double.IsInfinity(value))
            return QualityCode.Bad;
            
        // Check timestamp validity (reject future timestamps beyond clock skew tolerance)
        if (sourceTimestamp.HasValue && sourceTimestamp.Value > DateTimeOffset.UtcNow.AddMinutes(5))
            return QualityCode.BadFutureTimestamp;
            
        // Check if value is within expected range
        if (!ValidateReading(value, unit, streamType))
            return QualityCode.BadOutOfRange;
            
        // Check if timestamp is stale (older than 24 hours)
        if (sourceTimestamp.HasValue && DateTimeOffset.UtcNow - sourceTimestamp.Value > TimeSpan.FromHours(24))
            return QualityCode.BadStale;
            
        return QualityCode.Good;
    }
    
    public (double normalizedValue, Unit targetUnit) NormalizeReading(
        double value,
        Unit sourceUnit,
        StreamType streamType)
    {
        var targetUnit = GetCanonicalUnit(streamType);
        
        if (sourceUnit == targetUnit)
            return (value, targetUnit);
            
        try
        {
            var normalizedValue = ConvertUnit(value, sourceUnit, targetUnit);
            return (normalizedValue, targetUnit);
        }
        catch (NotSupportedException)
        {
            // If conversion not supported, return original value
            // Quality code determination will mark this as suspect
            return (value, sourceUnit);
        }
    }
    
    public Unit GetCanonicalUnit(StreamType streamType)
    {
        if (CanonicalUnits.TryGetValue(streamType, out var unit))
            return unit;
            
        // Default to the first unit if not defined
        return Unit.Count;
    }
    
    public (double min, double max) GetExpectedRange(StreamType streamType)
    {
        if (ExpectedRanges.TryGetValue(streamType, out var range))
            return range;
            
        // Return permissive range if not defined
        return (double.MinValue, double.MaxValue);
    }
}

