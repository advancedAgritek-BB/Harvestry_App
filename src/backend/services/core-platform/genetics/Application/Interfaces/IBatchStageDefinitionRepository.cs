using Harvestry.Genetics.Domain.Entities;

namespace Harvestry.Genetics.Application.Interfaces;

/// <summary>
/// Repository interface for BatchStageDefinition operations
/// </summary>
public interface IBatchStageDefinitionRepository
{
    Task<BatchStageDefinition> CreateAsync(BatchStageDefinition stage, CancellationToken cancellationToken = default);
    Task<BatchStageDefinition?> GetByIdAsync(Guid stageId, Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchStageDefinition>> GetAllAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchStageDefinition>> GetActiveAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<BatchStageDefinition> UpdateAsync(BatchStageDefinition stage, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid stageId, Guid siteId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid stageId, Guid siteId, CancellationToken cancellationToken = default);
}

