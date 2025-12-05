using Harvestry.Genetics.Application.DTOs;

namespace Harvestry.Genetics.Application.Interfaces;

/// <summary>
/// Service for managing batch stage definitions and transitions
/// </summary>
public interface IBatchStageConfigurationService
{
    // Stage Definition Operations
    Task<BatchStageResponse> CreateStageAsync(CreateBatchStageRequest request, Guid siteId, Guid userId, CancellationToken cancellationToken = default);
    Task<BatchStageResponse> GetStageByIdAsync(Guid stageId, Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchStageResponse>> GetAllStagesAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchStageResponse>> GetActiveStagesAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<BatchStageResponse> UpdateStageAsync(Guid stageId, UpdateBatchStageRequest request, Guid siteId, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteStageAsync(Guid stageId, Guid siteId, Guid userId, CancellationToken cancellationToken = default);
    Task<BatchStageResponse> ActivateStageAsync(Guid stageId, Guid siteId, Guid userId, CancellationToken cancellationToken = default);
    Task<BatchStageResponse> DeactivateStageAsync(Guid stageId, Guid siteId, Guid userId, CancellationToken cancellationToken = default);

    // Stage Transition Operations
    Task<StageTransitionResponse> CreateTransitionAsync(CreateStageTransitionRequest request, Guid siteId, Guid userId, CancellationToken cancellationToken = default);
    Task<StageTransitionResponse> GetTransitionByIdAsync(Guid transitionId, Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StageTransitionResponse>> GetAllTransitionsAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StageTransitionResponse>> GetTransitionsFromStageAsync(Guid fromStageId, Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StageTransitionResponse>> GetTransitionsToStageAsync(Guid toStageId, Guid siteId, CancellationToken cancellationToken = default);
    Task<StageTransitionResponse> UpdateTransitionAsync(Guid transitionId, UpdateStageTransitionRequest request, Guid siteId, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteTransitionAsync(Guid transitionId, Guid siteId, Guid userId, CancellationToken cancellationToken = default);

    // Validation & Reordering
    Task<bool> CanTransitionAsync(Guid fromStageId, Guid toStageId, Guid siteId, CancellationToken cancellationToken = default);
    Task ReorderStagesAsync(Dictionary<Guid, int> stageOrders, Guid siteId, Guid userId, CancellationToken cancellationToken = default);
}

