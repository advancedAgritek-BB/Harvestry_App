namespace Harvestry.Telemetry.Application.DTOs;

/// <summary>
/// HTTP-based telemetry ingest request payload.
/// </summary>
public record HttpIngestRequestDto(
    Guid SiteId,
    List<SensorReadingDto> Readings,
    string? Metadata = null
);
