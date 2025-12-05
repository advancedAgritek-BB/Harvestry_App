using Harvestry.Genetics.Domain.Enums;
using Harvestry.Genetics.Domain.ValueObjects;

namespace Harvestry.Genetics.Application.DTOs;

/// <summary>
/// Request to create a new batch
/// </summary>
public record CreateBatchRequest(
    Guid StrainId,
    string BatchCode,
    string BatchName,
    BatchType BatchType,
    BatchSourceType SourceType,
    int PlantCount,
    Guid CurrentStageId,
    Guid? ParentBatchId = null,
    int Generation = 1,
    int? TargetPlantCount = null,
    Guid? LocationId = null,
    Guid? RoomId = null,
    Guid? ZoneId = null,
    string? Notes = null,
    Dictionary<string, object>? Metadata = null);

/// <summary>
/// Request to update an existing batch
/// </summary>
public record UpdateBatchRequest(
    string BatchName,
    int PlantCount,
    int? TargetPlantCount,
    Guid? LocationId,
    Guid? RoomId,
    Guid? ZoneId,
    string? Notes,
    Dictionary<string, object>? Metadata);

/// <summary>
/// Request to transition batch to a new stage
/// </summary>
public record TransitionBatchStageRequest(
    Guid NewStageId,
    string? TransitionNotes = null);

/// <summary>
/// Request to update batch plant count
/// </summary>
public record UpdatePlantCountRequest(
    int NewPlantCount,
    string Reason);

/// <summary>
/// Request to split a batch
/// </summary>
public record SplitBatchRequest(
    int PlantCountToSplit,
    string NewBatchName,
    string? SplitReason = null);

/// <summary>
/// Request to merge batches
/// </summary>
public record MergeBatchesRequest(
    Guid[] SourceBatchIds,
    string MergedBatchName,
    string? MergeReason = null);

/// <summary>
/// Request to terminate a batch
/// </summary>
public record TerminateBatchRequest(
    string Reason);

/// <summary>
/// Batch response DTO
/// </summary>
public record BatchResponse(
    Guid Id,
    Guid SiteId,
    Guid StrainId,
    string BatchCode,
    string BatchName,
    BatchType BatchType,
    BatchSourceType SourceType,
    Guid? ParentBatchId,
    int Generation,
    int PlantCount,
    int TargetPlantCount,
    Guid CurrentStageId,
    DateTime StageStartedAt,
    DateOnly? ExpectedHarvestDate,
    DateOnly? ActualHarvestDate,
    Guid? LocationId,
    Guid? RoomId,
    Guid? ZoneId,
    BatchStatus Status,
    string? Notes,
    Dictionary<string, object> Metadata,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid CreatedByUserId,
    Guid UpdatedByUserId);

/// <summary>
/// Batch event response (read-only)
/// </summary>
public record BatchEventResponse(
    Guid Id,
    Guid SiteId,
    Guid BatchId,
    EventType EventType,
    DateTime OccurredAt,
    Guid? FromStageId,
    Guid? ToStageId,
    int? PreviousPlantCount,
    int? NewPlantCount,
    Guid? RelatedBatchId,
    string? Notes,
    Dictionary<string, object> EventData,
    Guid PerformedByUserId);

/// <summary>
/// Batch relationship response (read-only)
/// </summary>
public record BatchRelationshipResponse(
    Guid Id,
    Guid SiteId,
    Guid ParentBatchId,
    Guid ChildBatchId,
    RelationshipType RelationshipType,
    int? PlantCountTransferred,
    DateOnly TransferDate,
    DateTime CreatedAt,
    Guid CreatedByUserId,
    string? Notes);

