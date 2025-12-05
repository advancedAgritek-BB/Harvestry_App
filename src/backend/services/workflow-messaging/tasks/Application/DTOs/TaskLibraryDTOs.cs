using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.Application.DTOs;

/// <summary>
/// Request payload to create a new task library item (template).
/// </summary>
public sealed class CreateTaskLibraryItemRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Title is required.")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 500 characters.")]
    public string Title { get; init; } = string.Empty;

    [StringLength(4000, ErrorMessage = "Description must not exceed 4000 characters.")]
    public string? Description { get; init; }

    public TaskPriority DefaultPriority { get; init; } = TaskPriority.Normal;
    public TaskType TaskType { get; init; } = TaskType.Custom;

    [StringLength(100, ErrorMessage = "CustomTaskType must not exceed 100 characters.")]
    public string? CustomTaskType { get; init; }

    [StringLength(100, ErrorMessage = "DefaultAssignedToRole must not exceed 100 characters.")]
    public string? DefaultAssignedToRole { get; init; }

    public int? DefaultDueDaysOffset { get; init; }
    public IReadOnlyCollection<Guid>? DefaultSopIds { get; init; }
}

/// <summary>
/// Request payload to update an existing task library item.
/// </summary>
public sealed class UpdateTaskLibraryItemRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Title is required.")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 500 characters.")]
    public string Title { get; init; } = string.Empty;

    [StringLength(4000, ErrorMessage = "Description must not exceed 4000 characters.")]
    public string? Description { get; init; }

    public TaskPriority DefaultPriority { get; init; } = TaskPriority.Normal;
    public TaskType TaskType { get; init; } = TaskType.Custom;

    [StringLength(100, ErrorMessage = "CustomTaskType must not exceed 100 characters.")]
    public string? CustomTaskType { get; init; }

    [StringLength(100, ErrorMessage = "DefaultAssignedToRole must not exceed 100 characters.")]
    public string? DefaultAssignedToRole { get; init; }

    public int? DefaultDueDaysOffset { get; init; }
    public IReadOnlyCollection<Guid>? DefaultSopIds { get; init; }
}

/// <summary>
/// Response payload for a task library item.
/// </summary>
public sealed class TaskLibraryItemResponse
{
    public Guid Id { get; init; }
    public Guid OrgId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public TaskPriority DefaultPriority { get; init; }
    public TaskType TaskType { get; init; }
    public string? CustomTaskType { get; init; }
    public string? DefaultAssignedToRole { get; init; }
    public int? DefaultDueDaysOffset { get; init; }
    public bool IsActive { get; init; }
    public Guid CreatedByUserId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public IReadOnlyCollection<Guid> DefaultSopIds { get; init; } = Array.Empty<Guid>();
}

