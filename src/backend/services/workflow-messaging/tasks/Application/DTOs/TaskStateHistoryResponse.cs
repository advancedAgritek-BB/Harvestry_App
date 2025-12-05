using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TaskStatusEnum = Harvestry.Tasks.Domain.Enums.TaskStatus;

namespace Harvestry.Tasks.Application.DTOs;

/// <summary>
/// Represents a historical state transition for a task.
/// </summary>
public sealed class TaskStateHistoryResponse : IValidatableObject
{
    public TaskStatusEnum Status { get; init; }
    public TaskStatusEnum PreviousStatus { get; init; }
    public Guid ChangedBy { get; init; }
    public DateTimeOffset ChangedAt { get; init; }
    public string? Reason { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ChangedBy == Guid.Empty)
        {
            yield return new ValidationResult(
                "ChangedBy must not be an empty GUID.",
                new[] { nameof(ChangedBy) });
        }

        if (Status == PreviousStatus)
        {
            yield return new ValidationResult(
                "Status and PreviousStatus must be different (no self-transitions).",
                new[] { nameof(Status), nameof(PreviousStatus) });
        }

        if (ChangedAt > DateTimeOffset.UtcNow.AddMinutes(5))
        {
            yield return new ValidationResult(
                "ChangedAt must not be in the future.",
                new[] { nameof(ChangedAt) });
        }

        // Reject dates older than 1 year as potentially erroneous
        if (ChangedAt < DateTimeOffset.UtcNow.AddYears(-1))
        {
            yield return new ValidationResult(
                "ChangedAt is older than 1 year; this may indicate an error.",
                new[] { nameof(ChangedAt) });
        }

        // Enforce Reason for specific transitions
        if ((Status == TaskStatusEnum.Cancelled || Status == TaskStatusEnum.Blocked) && string.IsNullOrWhiteSpace(Reason))
        {
            yield return new ValidationResult(
                $"Reason is required when transitioning to {Status}.",
                new[] { nameof(Reason) });
        }
    }
}
