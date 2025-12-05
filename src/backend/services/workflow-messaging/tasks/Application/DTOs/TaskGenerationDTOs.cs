using System;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.Application.DTOs;

/// <summary>
/// Request to manually trigger task generation for a batch.
/// </summary>
public sealed class TriggerTaskGenerationRequest
{
    public Guid BatchId { get; init; }
    public Guid? StrainId { get; init; }
    public GrowthPhase Phase { get; init; }
    public BlueprintRoomType RoomType { get; init; }
}

/// <summary>
/// Event payload when a batch phase changes (consumed from messaging).
/// </summary>
public sealed class BatchPhaseChangedEvent
{
    public Guid SiteId { get; init; }
    public Guid BatchId { get; init; }
    public Guid? StrainId { get; init; }
    public GrowthPhase NewPhase { get; init; }
    public GrowthPhase? PreviousPhase { get; init; }
    public BlueprintRoomType RoomType { get; init; }
    public Guid ChangedByUserId { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
}

