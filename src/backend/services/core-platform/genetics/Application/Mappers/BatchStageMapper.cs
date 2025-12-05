using System;
using System.Linq;
using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Domain.Entities;

namespace Harvestry.Genetics.Application.Mappers;

/// <summary>
/// Mapper for BatchStage entities and DTOs
/// </summary>
public static class BatchStageMapper
{
    /// <summary>
    /// Map BatchStageDefinition entity to response DTO
    /// </summary>
    /// <param name="stage">Stage must be non-null</param>
    public static BatchStageResponse ToResponse(BatchStageDefinition stage)
    {
        if (stage == null)
            throw new ArgumentNullException(nameof(stage));

        return new BatchStageResponse(
            Id: stage.Id,
            SiteId: stage.SiteId,
            StageKey: stage.StageKey,
            DisplayName: stage.DisplayName,
            Description: stage.Description,
            SequenceOrder: stage.SequenceOrder,
            IsTerminal: stage.IsTerminal,
            RequiresHarvestMetrics: stage.RequiresHarvestMetrics,
            CreatedAt: stage.CreatedAt,
            UpdatedAt: stage.UpdatedAt,
            CreatedByUserId: stage.CreatedByUserId,
            UpdatedByUserId: stage.UpdatedByUserId
        );
    }

    /// <summary>
    /// Map list of BatchStageDefinition entities to response DTOs
    /// </summary>
    public static IReadOnlyList<BatchStageResponse> ToResponseList(IEnumerable<BatchStageDefinition> stages)
    {
        if (stages == null)
            throw new ArgumentNullException(nameof(stages));

        return stages.Select(ToResponse).ToList();
    }

    /// <summary>
    /// Map BatchStageTransition entity to response DTO
    /// </summary>
    public static StageTransitionResponse ToTransitionResponse(BatchStageTransition transition)
    {
        if (transition == null)
            throw new ArgumentNullException(nameof(transition));

        return new StageTransitionResponse(
            Id: transition.Id,
            SiteId: transition.SiteId,
            FromStageId: transition.FromStageId,
            ToStageId: transition.ToStageId,
            AutoAdvance: transition.AutoAdvance,
            RequiresApproval: transition.RequiresApproval,
            ApprovalRole: transition.ApprovalRole,
            CreatedAt: transition.CreatedAt,
            UpdatedAt: transition.UpdatedAt,
            CreatedByUserId: transition.CreatedByUserId,
            UpdatedByUserId: transition.UpdatedByUserId
        );
    }

    /// <summary>
    /// Map list of BatchStageTransition entities to response DTOs
    /// </summary>
    public static IReadOnlyList<StageTransitionResponse> ToTransitionResponseList(IEnumerable<BatchStageTransition> transitions)
    {
        if (transitions == null)
            throw new ArgumentNullException(nameof(transitions));

        return transitions.Select(ToTransitionResponse).ToList();
    }

    /// <summary>
    /// Map BatchStageHistory entity to response DTO
    /// </summary>
    public static StageHistoryResponse ToHistoryResponse(BatchStageHistory history)
    {
        if (history == null)
            throw new ArgumentNullException(nameof(history));

        return new StageHistoryResponse(
            Id: history.Id,
            BatchId: history.BatchId,
            FromStageId: history.FromStageId,
            ToStageId: history.ToStageId,
            ChangedAt: history.ChangedAt,
            ChangedByUserId: history.ChangedByUserId,
            Notes: history.Notes
        );
    }

    /// <summary>
    /// Map list of BatchStageHistory entities to response DTOs
    /// </summary>
    public static IReadOnlyList<StageHistoryResponse> ToHistoryResponseList(IEnumerable<BatchStageHistory> historyEntries)
    {
        if (historyEntries == null)
            throw new ArgumentNullException(nameof(historyEntries));

        return historyEntries.Select(ToHistoryResponse).ToList();
    }
}
