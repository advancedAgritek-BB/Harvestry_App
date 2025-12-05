using Harvestry.Telemetry.Domain.Entities;

namespace Harvestry.Telemetry.Application.Interfaces;

/// <summary>
/// Repository interface for alert rule operations.
/// </summary>
public interface IAlertRuleRepository
{
    Task<AlertRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<AlertRule>> GetActiveBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<List<AlertRule>> GetBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<List<AlertRule>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<List<AlertRule>> GetByStreamIdAsync(Guid streamId, CancellationToken cancellationToken = default);
    Task<AlertRule> CreateAsync(AlertRule rule, CancellationToken cancellationToken = default);
    Task UpdateAsync(AlertRule rule, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
