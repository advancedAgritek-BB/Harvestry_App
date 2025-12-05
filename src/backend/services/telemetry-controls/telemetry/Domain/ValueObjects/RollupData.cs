using Harvestry.Telemetry.Domain.Enums;

namespace Harvestry.Telemetry.Domain.ValueObjects;

/// <summary>
/// Aggregated sensor data from continuous aggregates.
/// Represents statistical summary over a time bucket.
/// </summary>
public readonly record struct RollupData
{
    public DateTimeOffset Bucket { get; init; }
    public RollupInterval Interval { get; init; }
    public int SampleCount { get; init; }
    public double AvgValue { get; init; }
    public double MinValue { get; init; }
    public double MaxValue { get; init; }
    public double? MedianValue { get; init; }
    public double? StdDevValue { get; init; }
    public QualityCode? MinQuality { get; init; }
    public QualityCode? MaxQuality { get; init; }
    
    /// <summary>
    /// Checks if rollup has sufficient samples for reliability.
    /// </summary>
    public bool HasSufficientSamples(int minimumSamples = 1) => SampleCount >= minimumSamples;
    
    /// <summary>
    /// Checks if all samples were good quality.
    /// </summary>
    public bool IsAllGoodQuality() => MaxQuality == QualityCode.Good;
    
    /// <summary>
    /// Gets the range (max - min) of values in the rollup.
    /// </summary>
    public double GetRange() => MaxValue - MinValue;
    
    /// <summary>
    /// Gets coefficient of variation (stddev / mean) as percentage.
    /// Useful for identifying high variability periods.
    /// </summary>
    public double? GetCoefficientOfVariation()
    {
        if (!StdDevValue.HasValue || AvgValue == 0) return null;
        return (StdDevValue.Value / Math.Abs(AvgValue)) * 100.0;
    }
}

