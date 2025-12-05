using Harvestry.Telemetry.Domain.Enums;

namespace Harvestry.Telemetry.Application.DTOs;

/// <summary>
/// Request to ingest a batch of telemetry readings.
/// </summary>
public record IngestTelemetryRequestDto(
    Guid SiteId,
    Guid EquipmentId,
    IngestionProtocol Protocol,
    List<SensorReadingDto> Readings
);

