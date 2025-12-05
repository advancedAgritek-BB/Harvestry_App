using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Application.Mappers;
using Harvestry.Genetics.Domain.Entities;
using Harvestry.Genetics.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Harvestry.Genetics.Application.Services;

/// <summary>
/// Service for managing batch stage definitions and transitions
/// </summary>
public class BatchStageConfigurationService : IBatchStageConfigurationService
{
    private readonly IBatchStageDefinitionRepository _stageRepository;
    private readonly IBatchStageTransitionRepository _transitionRepository;
    private readonly ILogger<BatchStageConfigurationService> _logger;

    public BatchStageConfigurationService(
        IBatchStageDefinitionRepository stageRepository,
        IBatchStageTransitionRepository transitionRepository,
        ILogger<BatchStageConfigurationService> logger)
    {
        _stageRepository = stageRepository;
        _transitionRepository = transitionRepository;
        _logger = logger;
    }

    // ===== Stage Definition Operations =====

    public async Task<BatchStageResponse> CreateStageAsync(CreateBatchStageRequest request, Guid siteId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating batch stage {StageName} for site {SiteId}", request.DisplayName, siteId);

        var stageKey = StageKey.Create(request.StageKey);

        var stage = BatchStageDefinition.Create(
            siteId: siteId,
            stageKey: stageKey,
            displayName: request.DisplayName,
            sequenceOrder: request.SequenceOrder,
            createdByUserId: userId,
            description: request.Description,
            isTerminal: request.IsTerminal,
            requiresHarvestMetrics: request.RequiresHarvestMetrics);

        var createdStage = await _stageRepository.CreateAsync(stage, cancellationToken);

        _logger.LogInformation("Created batch stage {StageId} with key {StageKey}", createdStage.Id, createdStage.StageKey.Value);
        return BatchStageMapper.ToResponse(createdStage);
    }

    public async Task<BatchStageResponse> GetStageByIdAsync(Guid stageId, Guid siteId, CancellationToken cancellationToken = default)
    {
        var stage = await _stageRepository.GetByIdAsync(stageId, siteId, cancellationToken);
        if (stage == null)
            throw new KeyNotFoundException($"Stage {stageId} not found");

        return BatchStageMapper.ToResponse(stage);
    }

    public async Task<IReadOnlyList<BatchStageResponse>> GetAllStagesAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var stages = await _stageRepository.GetAllAsync(siteId, cancellationToken);
        return BatchStageMapper.ToResponseList(stages);
    }

    public async Task<IReadOnlyList<BatchStageResponse>> GetActiveStagesAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var stages = await _stageRepository.GetActiveAsync(siteId, cancellationToken);
        return BatchStageMapper.ToResponseList(stages);
    }

    public async Task<BatchStageResponse> UpdateStageAsync(Guid stageId, UpdateBatchStageRequest request, Guid siteId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating batch stage {StageId}", stageId);

        var stage = await _stageRepository.GetByIdAsync(stageId, siteId, cancellationToken);
        if (stage == null)
            throw new KeyNotFoundException($"Stage {stageId} not found");

        stage.Update(
            displayName: request.DisplayName,
            description: request.Description,
            sequenceOrder: request.SequenceOrder,
            isTerminal: request.IsTerminal,
            requiresHarvestMetrics: request.RequiresHarvestMetrics,
            updatedByUserId: userId);

        var updatedStage = await _stageRepository.UpdateAsync(stage, cancellationToken);

        _logger.LogInformation("Updated batch stage {StageId}", stageId);
        return BatchStageMapper.ToResponse(updatedStage);
    }

    public async Task DeleteStageAsync(Guid stageId, Guid siteId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting batch stage {StageId}", stageId);

        var stage = await _stageRepository.GetByIdAsync(stageId, siteId, cancellationToken);
        if (stage == null)
            throw new KeyNotFoundException($"Stage {stageId} not found");

        // Check for transitions referencing this stage
        var transitionsFrom = await _transitionRepository.GetFromStageAsync(stageId, siteId, cancellationToken);
        var transitionsTo = await _transitionRepository.GetToStageAsync(stageId, siteId, cancellationToken);

        if (transitionsFrom.Count > 0 || transitionsTo.Count > 0)
            throw new InvalidOperationException($"Cannot delete stage {stageId} because it has {transitionsFrom.Count + transitionsTo.Count} transitions");

        await _stageRepository.DeleteAsync(stageId, siteId, cancellationToken);
        _logger.LogInformation("Deleted batch stage {StageId}", stageId);
    }

    public async Task<BatchStageResponse> ActivateStageAsync(Guid stageId, Guid siteId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Activating batch stage {StageId}", stageId);

        var stage = await _stageRepository.GetByIdAsync(stageId, siteId, cancellationToken);
        if (stage == null)
            throw new KeyNotFoundException($"Stage {stageId} not found");

        // For simplicity, we're treating all stages as "active" by default.
        // If you need an IsActive flag, add it to the entity and update this logic.
        // For now, just update the stage to trigger an update timestamp.
        stage.Update(
            displayName: stage.DisplayName,
            description: stage.Description,
            sequenceOrder: stage.SequenceOrder,
            isTerminal: stage.IsTerminal,
            requiresHarvestMetrics: stage.RequiresHarvestMetrics,
            updatedByUserId: userId);

        var updatedStage = await _stageRepository.UpdateAsync(stage, cancellationToken);

        _logger.LogInformation("Activated batch stage {StageId}", stageId);
        return BatchStageMapper.ToResponse(updatedStage);
    }

    public async Task<BatchStageResponse> DeactivateStageAsync(Guid stageId, Guid siteId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deactivating batch stage {StageId}", stageId);

        var stage = await _stageRepository.GetByIdAsync(stageId, siteId, cancellationToken);
        if (stage == null)
            throw new KeyNotFoundException($"Stage {stageId} not found");

        // Similar to Activate, if you need an IsActive flag, add it to the entity.
        // For now, this is a no-op that just updates the timestamp.
        stage.Update(
            displayName: stage.DisplayName,
            description: stage.Description,
            sequenceOrder: stage.SequenceOrder,
            isTerminal: stage.IsTerminal,
            requiresHarvestMetrics: stage.RequiresHarvestMetrics,
            updatedByUserId: userId);

        var updatedStage = await _stageRepository.UpdateAsync(stage, cancellationToken);

        _logger.LogInformation("Deactivated batch stage {StageId}", stageId);
        return BatchStageMapper.ToResponse(updatedStage);
    }

    // ===== Stage Transition Operations =====

    public async Task<StageTransitionResponse> CreateTransitionAsync(CreateStageTransitionRequest request, Guid siteId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating stage transition from {FromStageId} to {ToStageId}", request.FromStageId, request.ToStageId);

        // Validate stages exist
        var fromStageExists = await _stageRepository.ExistsAsync(request.FromStageId, siteId, cancellationToken);
        if (!fromStageExists)
            throw new InvalidOperationException($"From stage {request.FromStageId} not found");

        var toStageExists = await _stageRepository.ExistsAsync(request.ToStageId, siteId, cancellationToken);
        if (!toStageExists)
            throw new InvalidOperationException($"To stage {request.ToStageId} not found");

        // Check if transition already exists
        var existingTransition = await _transitionRepository.GetTransitionAsync(request.FromStageId, request.ToStageId, siteId, cancellationToken);
        if (existingTransition != null)
            throw new InvalidOperationException($"Transition from {request.FromStageId} to {request.ToStageId} already exists");

        var transition = BatchStageTransition.Create(
            siteId: siteId,
            fromStageId: request.FromStageId,
            toStageId: request.ToStageId,
            createdByUserId: userId,
            autoAdvance: request.AutoAdvance,
            requiresApproval: request.RequiresApproval,
            approvalRole: request.ApprovalRole);

        var createdTransition = await _transitionRepository.CreateAsync(transition, cancellationToken);

        _logger.LogInformation("Created stage transition {TransitionId}", createdTransition.Id);
        return BatchStageMapper.ToTransitionResponse(createdTransition);
    }

    public async Task<StageTransitionResponse> GetTransitionByIdAsync(Guid transitionId, Guid siteId, CancellationToken cancellationToken = default)
    {
        var transition = await _transitionRepository.GetByIdAsync(transitionId, siteId, cancellationToken);
        if (transition == null)
            throw new KeyNotFoundException($"Transition {transitionId} not found");

        return BatchStageMapper.ToTransitionResponse(transition);
    }

    public async Task<IReadOnlyList<StageTransitionResponse>> GetAllTransitionsAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var transitions = await _transitionRepository.GetAllAsync(siteId, cancellationToken);
        return BatchStageMapper.ToTransitionResponseList(transitions);
    }

    public async Task<IReadOnlyList<StageTransitionResponse>> GetTransitionsFromStageAsync(Guid fromStageId, Guid siteId, CancellationToken cancellationToken = default)
    {
        var transitions = await _transitionRepository.GetFromStageAsync(fromStageId, siteId, cancellationToken);
        return BatchStageMapper.ToTransitionResponseList(transitions);
    }

    public async Task<IReadOnlyList<StageTransitionResponse>> GetTransitionsToStageAsync(Guid toStageId, Guid siteId, CancellationToken cancellationToken = default)
    {
        var transitions = await _transitionRepository.GetToStageAsync(toStageId, siteId, cancellationToken);
        return BatchStageMapper.ToTransitionResponseList(transitions);
    }

    public async Task<StageTransitionResponse> UpdateTransitionAsync(Guid transitionId, UpdateStageTransitionRequest request, Guid siteId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating stage transition {TransitionId}", transitionId);

        var transition = await _transitionRepository.GetByIdAsync(transitionId, siteId, cancellationToken);
        if (transition == null)
            throw new KeyNotFoundException($"Transition {transitionId} not found");

        transition.Update(
            autoAdvance: request.AutoAdvance,
            requiresApproval: request.RequiresApproval,
            approvalRole: request.ApprovalRole,
            updatedByUserId: userId);

        var updatedTransition = await _transitionRepository.UpdateAsync(transition, cancellationToken);

        _logger.LogInformation("Updated stage transition {TransitionId}", transitionId);
        return BatchStageMapper.ToTransitionResponse(updatedTransition);
    }

    public async Task DeleteTransitionAsync(Guid transitionId, Guid siteId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting stage transition {TransitionId}", transitionId);

        var transition = await _transitionRepository.GetByIdAsync(transitionId, siteId, cancellationToken);
        if (transition == null)
            throw new KeyNotFoundException($"Transition {transitionId} not found");

        await _transitionRepository.DeleteAsync(transitionId, siteId, cancellationToken);
        _logger.LogInformation("Deleted stage transition {TransitionId}", transitionId);
    }

    // ===== Validation & Reordering =====

    public async Task<bool> CanTransitionAsync(Guid fromStageId, Guid toStageId, Guid siteId, CancellationToken cancellationToken = default)
    {
        var transition = await _transitionRepository.GetTransitionAsync(fromStageId, toStageId, siteId, cancellationToken);
        return transition != null;
    }

    public async Task ReorderStagesAsync(Dictionary<Guid, int> stageOrders, Guid siteId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Reordering {Count} stages for site {SiteId}", stageOrders.Count, siteId);

        foreach (var (stageId, newOrder) in stageOrders)
        {
            var stage = await _stageRepository.GetByIdAsync(stageId, siteId, cancellationToken);
            if (stage == null)
            {
                _logger.LogWarning("Stage {StageId} not found, skipping reorder", stageId);
                continue;
            }

            stage.UpdateSequenceOrder(newOrder, userId);
            await _stageRepository.UpdateAsync(stage, cancellationToken);
        }

        _logger.LogInformation("Reordered {Count} stages", stageOrders.Count);
    }
}

