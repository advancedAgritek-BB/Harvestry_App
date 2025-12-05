using Harvestry.Telemetry.Application.DTOs;
using Harvestry.Telemetry.Domain.Entities;
using Harvestry.Telemetry.Domain.Enums;

namespace Harvestry.Telemetry.Application.Interfaces;

/// <summary>
/// Service for normalizing sensor values and units.
/// Converts readings to canonical units for consistent storage and querying.
/// </summary>
public interface INormalizationService
{
    /// <summary>
    /// Normalizes a sensor reading DTO to a domain entity with canonical units and quality validation.
    /// </summary>
    Task<SensorReading> NormalizeAsync(
        SensorReadingDto dto,
        SensorStream stream,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Converts a value from one unit to another.
    /// </summary>
    double ConvertUnit(double value, Unit sourceUnit, Unit targetUnit);
    
    /// <summary>
    /// Validates that a reading is within acceptable range for stream type.
    /// </summary>
    bool ValidateReading(double value, Unit unit, StreamType streamType);
    
    /// <summary>
    /// Determines quality code based on value, unit, and timestamp validation.
    /// </summary>
    QualityCode DetermineQualityCode(
        double value,
        Unit unit,
        StreamType streamType,
        DateTimeOffset? sourceTimestamp);
    
    /// <summary>
    /// Normalizes a reading to canonical unit for stream type.
    /// Returns normalized value and target unit.
    /// </summary>
    (double normalizedValue, Unit targetUnit) NormalizeReading(
        double value,
        Unit sourceUnit,
        StreamType streamType);
    
    /// <summary>
    /// Gets the canonical unit for a stream type.
    /// </summary>
    Unit GetCanonicalUnit(StreamType streamType);
    
    /// <summary>
    /// Gets the expected range for a stream type (for validation).
    /// </summary>
    (double min, double max) GetExpectedRange(StreamType streamType);
}

