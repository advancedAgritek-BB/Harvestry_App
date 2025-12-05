using Harvestry.Telemetry.Domain.Entities;

namespace Harvestry.Telemetry.Application.Interfaces;

/// <summary>
/// Persistence operations for alert instances.
/// </summary>
public interface IAlertInstanceRepository
{
    Task<AlertInstance?> GetActiveByRuleAndStreamAsync(Guid ruleId, Guid streamId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AlertInstance>> GetActiveBySiteAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<AlertInstance?> GetByIdAsync(Guid alertId, CancellationToken cancellationToken = default);
    Task CreateAsync(AlertInstance alert, CancellationToken cancellationToken = default);
    Task UpdateAsync(AlertInstance alert, CancellationToken cancellationToken = default);
}
