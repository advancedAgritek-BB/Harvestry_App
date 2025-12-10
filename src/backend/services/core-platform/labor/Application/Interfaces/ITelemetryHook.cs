namespace Harvestry.Labor.Application.Interfaces;

public interface ITelemetryHook
{
    Task PublishProductivityEventAsync(string referenceId, string metric, decimal value, string unit, DateTime observedAtUtc, CancellationToken ct);
}


