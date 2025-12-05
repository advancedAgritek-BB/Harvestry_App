using Harvestry.Telemetry.Domain.Enums;

namespace Harvestry.Telemetry.Application.DTOs;

/// <summary>
/// Request to query telemetry data.
/// </summary>
public record QueryTelemetryRequestDto(
    Guid StreamId,
    DateTimeOffset Start,
    DateTimeOffset End,
    RollupInterval? RollupInterval = null,
    int? Limit = null
);

/// <summary>
/// Response with telemetry reading data.
/// </summary>
public record TelemetryReadingResponseDto(
    DateTimeOffset Time,
    double Value,
    QualityCode QualityCode
);

/// <summary>
/// Response with rollup/aggregate data.
/// </summary>
public record TelemetryRollupResponseDto(
    DateTimeOffset Bucket,
    int SampleCount,
    double AvgValue,
    double MinValue,
    double MaxValue,
    double? MedianValue = null,
    double? StdDevValue = null
);

/// <summary>
/// Response with latest reading per stream.
/// </summary>
public record LatestReadingDto(
    Guid StreamId,
    DateTimeOffset Time,
    double Value,
    QualityCode QualityCode,
    TimeSpan Age
)
{
    public bool IsStale(TimeSpan staleThreshold) => Age > staleThreshold;
}

