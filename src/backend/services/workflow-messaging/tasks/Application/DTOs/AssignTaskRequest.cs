using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Harvestry.Tasks.Application.DTOs;

/// <summary>
/// Request to assign or reassign a task to a user or role.
/// At least one of UserId or Role must be provided.
/// </summary>
public sealed class AssignTaskRequest : IValidatableObject
{
    /// <summary>
    /// The specific user ID to assign the task to (optional if Role is provided).
    /// </summary>
    public Guid? UserId { get; init; }

    /// <summary>
    /// The role identifier to assign the task to (optional if UserId is provided).
    /// Maximum length: 100 characters.
    /// </summary>
    [MaxLength(100, ErrorMessage = "Role must not exceed 100 characters.")]
    public string? Role { get; init; }

    /// <summary>
    /// Optional timestamp for when the task was assigned. Defaults to current time if not provided.
    /// </summary>
    public DateTimeOffset? AssignedAt { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!UserId.HasValue && string.IsNullOrWhiteSpace(Role))
        {
            yield return new ValidationResult(
                "At least one of UserId or Role must be provided.",
                new[] { nameof(UserId), nameof(Role) });
        }

        if (!string.IsNullOrEmpty(Role) && string.IsNullOrWhiteSpace(Role))
        {
            yield return new ValidationResult(
                "Role cannot be only whitespace characters.",
                new[] { nameof(Role) });
        }
    }
}
