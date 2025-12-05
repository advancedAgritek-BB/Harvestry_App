using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.Application.DTOs;

/// <summary>
/// Request payload to create a new task blueprint.
/// </summary>
public sealed class CreateTaskBlueprintRequest : IValidatableObject
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Title is required.")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 500 characters.")]
    public string Title { get; init; } = string.Empty;

    [StringLength(4000, ErrorMessage = "Description must not exceed 4000 characters.")]
    public string? Description { get; init; }

    public GrowthPhase GrowthPhase { get; init; } = GrowthPhase.Any;
    public BlueprintRoomType RoomType { get; init; } = BlueprintRoomType.Any;
    public Guid? StrainId { get; init; }
    public TaskPriority Priority { get; init; } = TaskPriority.Normal;

    /// <summary>
    /// Offset in hours from when phase begins to when task should be due.
    /// </summary>
    public int TimeOffsetHours { get; init; }

    [StringLength(100, ErrorMessage = "AssignedToRole must not exceed 100 characters.")]
    public string? AssignedToRole { get; init; }

    public IReadOnlyCollection<Guid>? RequiredSopIds { get; init; }
    public IReadOnlyCollection<Guid>? RequiredTrainingIds { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (GrowthPhase == GrowthPhase.Any && RoomType == BlueprintRoomType.Any && !StrainId.HasValue)
        {
            yield return new ValidationResult(
                "At least one matching criterion (GrowthPhase, RoomType, or StrainId) must be specified.",
                new[] { nameof(GrowthPhase), nameof(RoomType), nameof(StrainId) });
        }
    }
}

/// <summary>
/// Request payload to update an existing task blueprint.
/// </summary>
public sealed class UpdateTaskBlueprintRequest : IValidatableObject
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Title is required.")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 500 characters.")]
    public string Title { get; init; } = string.Empty;

    [StringLength(4000, ErrorMessage = "Description must not exceed 4000 characters.")]
    public string? Description { get; init; }

    public GrowthPhase GrowthPhase { get; init; } = GrowthPhase.Any;
    public BlueprintRoomType RoomType { get; init; } = BlueprintRoomType.Any;
    public Guid? StrainId { get; init; }
    public TaskPriority Priority { get; init; } = TaskPriority.Normal;

    /// <summary>
    /// Offset in hours from when phase begins to when task should be due.
    /// </summary>
    public int TimeOffsetHours { get; init; }

    [StringLength(100, ErrorMessage = "AssignedToRole must not exceed 100 characters.")]
    public string? AssignedToRole { get; init; }

    public IReadOnlyCollection<Guid>? RequiredSopIds { get; init; }
    public IReadOnlyCollection<Guid>? RequiredTrainingIds { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (GrowthPhase == GrowthPhase.Any && RoomType == BlueprintRoomType.Any && !StrainId.HasValue)
        {
            yield return new ValidationResult(
                "At least one matching criterion (GrowthPhase, RoomType, or StrainId) must be specified.",
                new[] { nameof(GrowthPhase), nameof(RoomType), nameof(StrainId) });
        }
    }
}

/// <summary>
/// Response payload for a task blueprint.
/// </summary>
public sealed class TaskBlueprintResponse
{
    public Guid Id { get; init; }
    public Guid SiteId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public GrowthPhase GrowthPhase { get; init; }
    public BlueprintRoomType RoomType { get; init; }
    public Guid? StrainId { get; init; }
    public TaskPriority Priority { get; init; }
    public int TimeOffsetHours { get; init; }
    public string? AssignedToRole { get; init; }
    public bool IsActive { get; init; }
    public Guid CreatedByUserId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public IReadOnlyCollection<Guid> RequiredSopIds { get; init; } = Array.Empty<Guid>();
    public IReadOnlyCollection<Guid> RequiredTrainingIds { get; init; } = Array.Empty<Guid>();
}

