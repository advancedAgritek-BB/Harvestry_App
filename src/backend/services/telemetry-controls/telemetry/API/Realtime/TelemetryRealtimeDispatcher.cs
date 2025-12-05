using Harvestry.Telemetry.API.Hubs;
using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Harvestry.Telemetry.API.Realtime;

/// <summary>
/// SignalR-based implementation of telemetry real-time fan-out.
/// </summary>
public sealed class TelemetryRealtimeDispatcher : ITelemetryRealtimeDispatcher
{
    private readonly IHubContext<TelemetryHub> _hubContext;
    private readonly ILogger<TelemetryRealtimeDispatcher> _logger;

    public TelemetryRealtimeDispatcher(
        IHubContext<TelemetryHub> hubContext,
        ILogger<TelemetryRealtimeDispatcher> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PublishAsync(IEnumerable<SensorReading> readings, CancellationToken cancellationToken = default)
    {
        var readingList = readings.ToList();
        if (readingList.Count == 0)
        {
            return;
        }

        foreach (var reading in readingList)
        {
            await PublishWalEventAsync(reading, cancellationToken).ConfigureAwait(false);
        }

        _logger.LogDebug("Published {Count} telemetry readings to real-time subscribers", readingList.Count);
    }

    public async Task PublishWalEventAsync(SensorReading reading, CancellationToken cancellationToken = default)
    {
        if (reading == null)
        {
            throw new ArgumentNullException(nameof(reading));
        }

        var group = $"stream:{reading.StreamId}";
        await _hubContext.Clients
            .Group(group)
            .SendAsync(
                "telemetry",
                new
                {
                    streamId = reading.StreamId,
                    time = reading.Time,
                    value = reading.Value,
                    qualityCode = reading.QualityCode,
                    sourceTimestamp = reading.SourceTimestamp,
                    ingestionTimestamp = reading.IngestionTimestamp,
                    messageId = reading.MessageId,
                    metadata = reading.Metadata
                },
                cancellationToken);
    }
}
