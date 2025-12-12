using Harvestry.Identity.Domain.Entities;

namespace Harvestry.Identity.Application.Interfaces;

public interface ISiteRepository : IRepository<Site>
{
    Task<IEnumerable<Site>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default);
}











