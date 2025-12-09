using Harvestry.Analytics.Domain.Entities;

namespace Harvestry.Analytics.Application.Interfaces;

public interface IDashboardRepository
{
    Task<Dashboard?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Dashboard>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Dashboard dashboard, CancellationToken cancellationToken = default);
    Task UpdateAsync(Dashboard dashboard, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
