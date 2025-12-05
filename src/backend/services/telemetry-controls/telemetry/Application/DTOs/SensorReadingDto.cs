using Harvestry.Telemetry.Domain.Enums;

namespace Harvestry.Telemetry.Application.DTOs;

/// <summary>
/// DTO for a single sensor reading.
/// </summary>
public record SensorReadingDto(
    Guid StreamId,
    DateTimeOffset Time,
    double Value,
    Unit Unit,
    DateTimeOffset? SourceTimestamp = null,
    string? MessageId = null,
    string? Metadata = null
);

