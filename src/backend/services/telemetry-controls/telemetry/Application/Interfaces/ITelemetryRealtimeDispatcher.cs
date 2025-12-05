using Harvestry.Telemetry.Domain.Entities;

namespace Harvestry.Telemetry.Application.Interfaces;

/// <summary>
/// Dispatches telemetry events to real-time subscribers.
/// </summary>
public interface ITelemetryRealtimeDispatcher
{
    Task PublishAsync(IEnumerable<SensorReading> readings, CancellationToken cancellationToken = default);
    Task PublishWalEventAsync(SensorReading reading, CancellationToken cancellationToken = default);
}
