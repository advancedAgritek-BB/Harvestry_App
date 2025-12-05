using Harvestry.Genetics.Domain.Entities;
using Harvestry.Genetics.Domain.Enums;

namespace Harvestry.Genetics.Application.Interfaces;

/// <summary>
/// Repository contract for propagation override requests.
/// </summary>
public interface IPropagationOverrideRequestRepository
{
    Task<IReadOnlyList<PropagationOverrideRequest>> GetBySiteAsync(Guid siteId, PropagationOverrideStatus? status, CancellationToken cancellationToken = default);
    Task<PropagationOverrideRequest?> GetByIdAsync(Guid siteId, Guid overrideId, CancellationToken cancellationToken = default);
    Task<PropagationOverrideRequest> AddAsync(PropagationOverrideRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(PropagationOverrideRequest request, CancellationToken cancellationToken = default);
}
