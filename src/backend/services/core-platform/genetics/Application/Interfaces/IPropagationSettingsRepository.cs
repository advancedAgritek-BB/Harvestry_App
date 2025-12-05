using Harvestry.Genetics.Domain.Entities;

namespace Harvestry.Genetics.Application.Interfaces;

/// <summary>
/// Repository contract for propagation settings management.
/// </summary>
public interface IPropagationSettingsRepository
{
    Task<PropagationSettings?> GetBySiteAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<PropagationSettings> UpsertAsync(PropagationSettings settings, CancellationToken cancellationToken = default);
}
