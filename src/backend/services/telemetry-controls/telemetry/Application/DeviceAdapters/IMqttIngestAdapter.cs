using Harvestry.Telemetry.Application.DTOs;

namespace Harvestry.Telemetry.Application.DeviceAdapters;

/// <summary>
/// Adapter entry point for MQTT telemetry ingestion.
/// </summary>
public interface IMqttIngestAdapter
{
    Task<IngestResultDto> HandleAsync(string topic, byte[] payload, CancellationToken cancellationToken = default);
}
