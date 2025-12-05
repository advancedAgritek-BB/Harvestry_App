using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TaskPriorityEnum = Harvestry.Tasks.Domain.Enums.TaskPriority;
using TaskTypeEnum = Harvestry.Tasks.Domain.Enums.TaskType;

namespace Harvestry.Tasks.Application.DTOs;

/// <summary>
/// Request payload to create a new task.
/// </summary>
public sealed class CreateTaskRequest : IValidatableObject
{
    public TaskTypeEnum TaskType { get; init; }
    
    /// <summary>
    /// Required when TaskType is Custom; otherwise should be null.
    /// </summary>
    public string? CustomTaskType { get; init; }
    
    /// <summary>
    /// The task title (required, 1-500 characters).
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Title is required.")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 500 characters.")]
    public string Title { get; init; } = string.Empty;
    
    public string? Description { get; init; }
    
    /// <summary>
    /// Optional: The user ID to assign the task to. Mutually exclusive with AssignedToRole.
    /// </summary>
    public Guid? AssignedToUserId { get; init; }
    
    /// <summary>
    /// Optional: The role to assign the task to. Mutually exclusive with AssignedToUserId.
    /// </summary>
    public string? AssignedToRole { get; init; }
    
    public TaskPriorityEnum Priority { get; init; } = TaskPriorityEnum.Normal;
    public DateTimeOffset? DueDate { get; init; }
    public IReadOnlyCollection<Guid>? RequiredSopIds { get; init; }
    public IReadOnlyCollection<Guid>? RequiredTrainingIds { get; init; }
    public string? RelatedEntityType { get; init; }
    public Guid? RelatedEntityId { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (TaskType == TaskTypeEnum.Custom && string.IsNullOrWhiteSpace(CustomTaskType))
        {
            yield return new ValidationResult(
                "CustomTaskType is required when TaskType is Custom.",
                new[] { nameof(CustomTaskType) });
        }

        if (AssignedToUserId.HasValue && !string.IsNullOrWhiteSpace(AssignedToRole))
        {
            yield return new ValidationResult(
                "AssignedToUserId and AssignedToRole are mutually exclusive; provide only one.",
                new[] { nameof(AssignedToUserId), nameof(AssignedToRole) });
        }
    }
}
