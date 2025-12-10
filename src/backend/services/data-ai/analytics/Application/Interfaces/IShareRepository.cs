using Harvestry.Analytics.Domain.Entities;

namespace Harvestry.Analytics.Application.Interfaces;

public interface IShareRepository
{
    Task<IEnumerable<Share>> GetByResourceAsync(string resourceType, Guid resourceId, CancellationToken cancellationToken = default);
    Task AddAsync(Share share, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}




