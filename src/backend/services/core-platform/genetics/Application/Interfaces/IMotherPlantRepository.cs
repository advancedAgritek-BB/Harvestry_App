using Harvestry.Genetics.Domain.Entities;
using Harvestry.Genetics.Domain.Enums;

namespace Harvestry.Genetics.Application.Interfaces;

/// <summary>
/// Repository contract for managing mother plants.
/// </summary>
public interface IMotherPlantRepository
{
    Task AddAsync(MotherPlant motherPlant, CancellationToken cancellationToken = default);
    Task<MotherPlant?> GetByIdAsync(Guid siteId, Guid motherPlantId, CancellationToken cancellationToken = default);
    Task<MotherPlant?> GetByPlantTagAsync(Guid siteId, string plantTag, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MotherPlant>> GetBySiteAsync(Guid siteId, MotherPlantStatus? status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MotherPlant>> GetByStrainAsync(Guid siteId, Guid strainId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MotherPlant>> GetByLocationAsync(Guid siteId, Guid locationId, CancellationToken cancellationToken = default);
    Task UpdateAsync(MotherPlant motherPlant, CancellationToken cancellationToken = default);
    Task UpdatePropagationAsync(Guid siteId, Guid motherPlantId, int newTotalCount, DateOnly lastPropagationDate, int propagatedCount, Guid userId, string? notes, CancellationToken cancellationToken = default);
    Task<bool> TryUpdatePropagationWithLimitsAsync(Guid siteId, Guid motherPlantId, int newTotalCount, DateOnly lastPropagationDate, int propagatedCount, Guid userId, string? notes, int? dailyLimit, int? weeklyLimit, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MotherPlant>> GetOverdueForHealthCheckAsync(Guid siteId, TimeSpan threshold, CancellationToken cancellationToken = default);
    Task<int> GetPropagationCountForWindowAsync(Guid siteId, DateOnly windowStart, DateOnly windowEnd, CancellationToken cancellationToken = default);
}
