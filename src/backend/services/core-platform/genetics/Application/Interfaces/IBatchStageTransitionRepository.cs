using Harvestry.Genetics.Domain.Entities;

namespace Harvestry.Genetics.Application.Interfaces;

/// <summary>
/// Repository interface for BatchStageTransition operations
/// </summary>
public interface IBatchStageTransitionRepository
{
    Task<BatchStageTransition> CreateAsync(BatchStageTransition transition, CancellationToken cancellationToken = default);
    Task<BatchStageTransition?> GetByIdAsync(Guid transitionId, Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchStageTransition>> GetAllAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchStageTransition>> GetFromStageAsync(Guid fromStageId, Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchStageTransition>> GetToStageAsync(Guid toStageId, Guid siteId, CancellationToken cancellationToken = default);
    Task<BatchStageTransition?> GetTransitionAsync(Guid fromStageId, Guid toStageId, Guid siteId, CancellationToken cancellationToken = default);
    Task<BatchStageTransition> UpdateAsync(BatchStageTransition transition, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid transitionId, Guid siteId, CancellationToken cancellationToken = default);
    Task<bool> TransitionExistsAsync(Guid fromStageId, Guid toStageId, Guid siteId, CancellationToken cancellationToken = default);
}

