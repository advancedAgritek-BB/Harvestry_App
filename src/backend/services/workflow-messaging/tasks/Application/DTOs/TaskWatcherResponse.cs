using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Harvestry.Tasks.Application.DTOs;

/// <summary>
/// Represents a user watching a task for notifications.
/// </summary>
public sealed class TaskWatcherResponse : IValidatableObject
{
    /// <summary>
    /// The unique identifier of the user watching the task (required, non-empty).
    /// </summary>
    [Required]
    public required Guid UserId { get; init; }

    /// <summary>
    /// The timestamp when the user started watching the task (required).
    /// </summary>
    [Required]
    public required DateTimeOffset CreatedAt { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (UserId == Guid.Empty)
        {
            yield return new ValidationResult(
                "UserId must not be an empty GUID.",
                new[] { nameof(UserId) });
        }

        if (CreatedAt == DateTimeOffset.MinValue)
        {
            yield return new ValidationResult(
                "CreatedAt must be a valid timestamp.",
                new[] { nameof(CreatedAt) });
        }
    }
}
