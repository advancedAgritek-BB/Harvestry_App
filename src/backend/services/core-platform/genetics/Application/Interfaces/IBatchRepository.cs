using Harvestry.Genetics.Domain.Entities;
using Harvestry.Genetics.Domain.Enums;

namespace Harvestry.Genetics.Application.Interfaces;

/// <summary>
/// Repository interface for Batch aggregate operations
/// </summary>
public interface IBatchRepository
{
    // Basic CRUD
    Task<Batch> CreateAsync(Batch batch, CancellationToken cancellationToken = default);
    Task<Batch?> GetByIdAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Batch>> GetByStrainIdAsync(Guid strainId, Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Batch>> GetByStageIdAsync(Guid stageId, Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Batch>> GetByStatusAsync(BatchStatus status, Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Batch>> GetActiveAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<Batch> UpdateAsync(Batch batch, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default);

    // Genealogy Queries
    Task<IReadOnlyList<Batch>> GetDescendantsAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default);
    Task<Batch?> GetParentAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default);

    // Validation
    Task<bool> BatchCodeExistsAsync(string batchCode, Guid siteId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default);
}

