using Harvestry.Telemetry.Application.DTOs;

namespace Harvestry.Telemetry.Application.Interfaces;

public interface ICultivationInsightProvider
{
    Task<IReadOnlyCollection<CultivationInsightContext>> GetActiveContextsAsync(CancellationToken cancellationToken);
}




