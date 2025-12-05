using Harvestry.Telemetry.Application.DTOs;

namespace Harvestry.Telemetry.Application.DeviceAdapters;

/// <summary>
/// Adapter entry point for HTTP telemetry ingestion.
/// </summary>
public interface IHttpIngestAdapter
{
    Task<IngestResultDto> HandleAsync(Guid equipmentId, HttpIngestRequestDto request, CancellationToken cancellationToken = default);
}
