using Harvestry.Genetics.Domain.ValueObjects;

namespace Harvestry.Genetics.Application.DTOs;

/// <summary>
/// Request to create a batch stage definition
/// </summary>
public record CreateBatchStageRequest(
    StageKey StageKey,
    string DisplayName,
    int SequenceOrder,
    string? Description = null,
    bool IsTerminal = false,
    bool RequiresHarvestMetrics = false);

/// <summary>
/// Request to update a batch stage definition
/// </summary>
public record UpdateBatchStageRequest(
    string DisplayName,
    string? Description,
    int SequenceOrder,
    bool IsTerminal,
    bool RequiresHarvestMetrics);

/// <summary>
/// Batch stage definition response
/// </summary>
public record BatchStageResponse(
    Guid Id,
    Guid SiteId,
    StageKey StageKey,
    string DisplayName,
    string? Description,
    int SequenceOrder,
    bool IsTerminal,
    bool RequiresHarvestMetrics,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid CreatedByUserId,
    Guid UpdatedByUserId);

/// <summary>
/// Request to create a stage transition rule
/// </summary>
public record CreateStageTransitionRequest(
    Guid FromStageId,
    Guid ToStageId,
    bool AutoAdvance = false,
    bool RequiresApproval = false,
    string? ApprovalRole = null);

/// <summary>
/// Request to update a stage transition rule
/// </summary>
public record UpdateStageTransitionRequest(
    bool AutoAdvance,
    bool RequiresApproval,
    string? ApprovalRole);

/// <summary>
/// Stage transition response
/// </summary>
public record StageTransitionResponse(
    Guid Id,
    Guid SiteId,
    Guid FromStageId,
    Guid ToStageId,
    bool AutoAdvance,
    bool RequiresApproval,
    string? ApprovalRole,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid CreatedByUserId,
    Guid UpdatedByUserId);

/// <summary>
/// Stage history entry response (read-only)
/// </summary>
public record StageHistoryResponse(
    Guid Id,
    Guid BatchId,
    Guid? FromStageId,
    Guid ToStageId,
    DateTime ChangedAt,
    Guid ChangedByUserId,
    string? Notes);
