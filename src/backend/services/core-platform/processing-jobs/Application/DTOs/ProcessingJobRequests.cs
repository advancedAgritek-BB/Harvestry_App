using System.ComponentModel.DataAnnotations;

namespace Harvestry.ProcessingJobs.Application.DTOs;

/// <summary>
/// Request to create a new processing job
/// </summary>
public record CreateProcessingJobRequest
{
    [Required]
    public Guid ProcessingJobTypeId { get; init; }

    [Required]
    public DateOnly StartDate { get; init; }

    public DateOnly? ExpectedEndDate { get; init; }

    public string? Notes { get; init; }
}

/// <summary>
/// Request to add an input to a processing job
/// </summary>
public record AddInputRequest
{
    [Required]
    public Guid PackageId { get; init; }

    [Required]
    [Range(0.0001, double.MaxValue)]
    public decimal Quantity { get; init; }
}

/// <summary>
/// Request to add an output to a processing job
/// </summary>
public record AddOutputRequest
{
    [Required]
    [StringLength(30)]
    public string PackageLabel { get; init; } = string.Empty;

    [Required]
    public Guid ItemId { get; init; }

    [Required]
    [StringLength(200)]
    public string ItemName { get; init; } = string.Empty;

    public string? ItemCategory { get; init; }

    [Required]
    [Range(0.0001, double.MaxValue)]
    public decimal Quantity { get; init; }

    [Required]
    public string UnitOfMeasure { get; init; } = string.Empty;

    public Guid? LocationId { get; init; }
    public string? LocationName { get; init; }

    public bool IsWaste { get; init; }
    public string? WasteType { get; init; }
}

/// <summary>
/// Request to finish a processing job
/// </summary>
public record FinishJobRequest
{
    [Required]
    public DateOnly EndDate { get; init; }

    public string? Notes { get; init; }
}



