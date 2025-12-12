using Harvestry.Telemetry.Domain.Enums;
using Harvestry.Telemetry.Domain.Models.Simulation;

namespace Harvestry.Telemetry.Application.Interfaces;

public interface ITelemetrySimulationService
{
    Task StartSimulationAsync(StreamType streamType, CancellationToken cancellationToken = default);
    Task StopSimulationAsync(StreamType streamType, CancellationToken cancellationToken = default);
    Task ToggleSimulationAsync(Guid streamId, CancellationToken cancellationToken = default);
    Task UpdateProfileAsync(StreamType streamType, SimulationProfile profile, CancellationToken cancellationToken = default);
    
    // Called by the worker
    Task GenerateAndIngestAsync(CancellationToken cancellationToken = default);
    
    IEnumerable<SimulationState> GetActiveSimulations();
}











