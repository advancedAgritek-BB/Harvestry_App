using Harvestry.Telemetry.Application.DTOs;

namespace Harvestry.Telemetry.Application.Interfaces;

public interface ICultivationInsightService
{
    Task<string> GenerateEnvironmentInsightAsync(CultivationInsightContext context, CancellationToken cancellationToken);
}



