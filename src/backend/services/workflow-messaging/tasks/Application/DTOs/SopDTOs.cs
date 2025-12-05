using System;
using System.ComponentModel.DataAnnotations;

namespace Harvestry.Tasks.Application.DTOs;

/// <summary>
/// Request payload to create a new SOP.
/// </summary>
public sealed class CreateSopRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Title is required.")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 500 characters.")]
    public string Title { get; init; } = string.Empty;

    public string? Content { get; init; }

    [StringLength(100, ErrorMessage = "Category must not exceed 100 characters.")]
    public string? Category { get; init; }
}

/// <summary>
/// Request payload to update an existing SOP.
/// </summary>
public sealed class UpdateSopRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Title is required.")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 500 characters.")]
    public string Title { get; init; } = string.Empty;

    public string? Content { get; init; }

    [StringLength(100, ErrorMessage = "Category must not exceed 100 characters.")]
    public string? Category { get; init; }
}

/// <summary>
/// Response payload for an SOP.
/// </summary>
public sealed class SopResponse
{
    public Guid Id { get; init; }
    public Guid OrgId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Content { get; init; }
    public string? Category { get; init; }
    public int Version { get; init; }
    public bool IsActive { get; init; }
    public Guid CreatedByUserId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>
/// Summary response for SOP list views.
/// </summary>
public sealed class SopSummaryResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Category { get; init; }
    public int Version { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

