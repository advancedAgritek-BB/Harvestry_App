using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Telemetry.Application.DTOs;
using Harvestry.Telemetry.Application.Interfaces;

namespace Harvestry.Telemetry.Infrastructure.Insights;

/// <summary>
/// Temporary provider that emits a minimal context so the insight pipeline can run.
/// Replace with a real implementation that aggregates telemetry and operational state.
/// </summary>
public sealed class StubCultivationInsightProvider : ICultivationInsightProvider
{
    public Task<IReadOnlyCollection<CultivationInsightContext>> GetActiveContextsAsync(CancellationToken cancellationToken)
    {
        var contexts = new List<CultivationInsightContext>
        {
            new(
                Room: "F1",
                Phase: "Flowering",
                TelemetrySummary: "Temp 78F, RH 58%, VPD 1.3, EC 2.4, pH 5.8",
                Issues: "Slight RH drift above target in last 30m; EC trending up 0.2 in 2h.")
        };

        return Task.FromResult<IReadOnlyCollection<CultivationInsightContext>>(contexts);
    }
}




