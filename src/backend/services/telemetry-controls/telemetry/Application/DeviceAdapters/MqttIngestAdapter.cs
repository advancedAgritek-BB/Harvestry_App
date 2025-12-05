using Harvestry.Telemetry.Application.DTOs;
using Harvestry.Telemetry.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Harvestry.Telemetry.Application.DeviceAdapters;

/// <summary>
/// Handles inbound MQTT telemetry messages and delegates to the ingest service.
/// </summary>
public sealed class MqttIngestAdapter : IMqttIngestAdapter
{
    private readonly ITelemetryIngestService _ingestService;
    private readonly ILogger<MqttIngestAdapter> _logger;

    public MqttIngestAdapter(
        ITelemetryIngestService ingestService,
        ILogger<MqttIngestAdapter> logger)
    {
        _ingestService = ingestService ?? throw new ArgumentNullException(nameof(ingestService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IngestResultDto> HandleAsync(string topic, byte[] payload, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _ingestService.IngestMqttMessageAsync(topic, payload, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "MQTT ingest completed for topic {Topic}: Accepted={Accepted}, Rejected={Rejected}, Duplicates={Duplicates}",
                topic,
                result.Accepted,
                result.Rejected,
                result.Duplicates);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ingest MQTT payload for topic {Topic}", topic);
            throw;
        }
    }
}
