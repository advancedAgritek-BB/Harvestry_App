using System.ComponentModel.DataAnnotations;

namespace Harvestry.Tasks.Application.DTOs;

/// <summary>
/// Composite response containing a task and its gating status.
/// Both properties are required and must not be null.
/// </summary>
public sealed class TaskWithGatingResponse
{
    [Required]
    public required TaskResponse Task { get; init; }

    [Required]
    public required TaskGatingStatusResponse Gating { get; init; }
}
