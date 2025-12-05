using Harvestry.Genetics.Domain.Entities;

namespace Harvestry.Genetics.Application.Interfaces;

/// <summary>
/// Repository contract for mother plant health logs.
/// </summary>
public interface IMotherHealthLogRepository
{
    Task AddAsync(MotherHealthLog healthLog, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MotherHealthLog>> GetByMotherPlantAsync(Guid siteId, Guid motherPlantId, CancellationToken cancellationToken = default);
    Task<MotherHealthLog?> GetLatestAsync(Guid siteId, Guid motherPlantId, CancellationToken cancellationToken = default);
}
