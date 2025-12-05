using Harvestry.Telemetry.Domain.Enums;

namespace Harvestry.Telemetry.Domain.ValueObjects;

/// <summary>
/// Value object representing a sensor reading with unit and quality.
/// Immutable to ensure consistency.
/// </summary>
public readonly record struct SensorValue
{
    public double Value { get; init; }
    public Unit Unit { get; init; }
    public QualityCode QualityCode { get; init; }
    
    public SensorValue(double value, Unit unit, QualityCode qualityCode = QualityCode.Good)
    {
        Value = value;
        Unit = unit;
        QualityCode = qualityCode;
    }
    
    /// <summary>
    /// Checks if the reading is of good quality.
    /// </summary>
    public bool IsGoodQuality() => QualityCode == QualityCode.Good;
    
    /// <summary>
    /// Checks if the reading is suspect or bad quality.
    /// </summary>
    public bool IsSuspectOrBad() => QualityCode >= QualityCode.Uncertain;
    
    /// <summary>
    /// Checks if value is within expected range for stream type.
    /// </summary>
    public bool IsWithinRange(double min, double max) => Value >= min && Value <= max;
}

