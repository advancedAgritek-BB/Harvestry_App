namespace Harvestry.ProcessingJobs.Application.DTOs;

/// <summary>
/// Full processing job DTO
/// </summary>
public record ProcessingJobDto
{
    public Guid Id { get; init; }
    public Guid SiteId { get; init; }
    public string JobNumber { get; init; } = string.Empty;
    public Guid ProcessingJobTypeId { get; init; }
    public string ProcessingJobTypeName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public DateOnly? ExpectedEndDate { get; init; }

    public List<ProcessingJobInputDto> Inputs { get; init; } = new();
    public List<ProcessingJobOutputDto> Outputs { get; init; } = new();

    public decimal TotalInputQuantity { get; init; }
    public decimal TotalOutputQuantity { get; init; }
    public decimal YieldPercent { get; init; }

    public string? Notes { get; init; }
    public long? MetrcJobId { get; init; }
    public string? MetrcSyncStatus { get; init; }

    public DateTime CreatedAt { get; init; }
    public Guid CreatedByUserId { get; init; }
    public DateTime UpdatedAt { get; init; }
    public Guid UpdatedByUserId { get; init; }
}

/// <summary>
/// Processing job input DTO
/// </summary>
public record ProcessingJobInputDto
{
    public Guid Id { get; init; }
    public Guid PackageId { get; init; }
    public string PackageLabel { get; init; } = string.Empty;
    public string ItemName { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public string UnitOfMeasure { get; init; } = string.Empty;
    public decimal? UnitCost { get; init; }
    public decimal TotalCost { get; init; }
}

/// <summary>
/// Processing job output DTO
/// </summary>
public record ProcessingJobOutputDto
{
    public Guid Id { get; init; }
    public Guid PackageId { get; init; }
    public string PackageLabel { get; init; } = string.Empty;
    public string ItemName { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public string UnitOfMeasure { get; init; } = string.Empty;
    public bool IsWaste { get; init; }
    public string? WasteType { get; init; }
}

/// <summary>
/// Summary DTO for lists
/// </summary>
public record ProcessingJobSummaryDto
{
    public Guid Id { get; init; }
    public string JobNumber { get; init; } = string.Empty;
    public string ProcessingJobTypeName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public int InputCount { get; init; }
    public int OutputCount { get; init; }
    public decimal TotalInputQuantity { get; init; }
    public decimal TotalOutputQuantity { get; init; }
    public decimal YieldPercent { get; init; }
    public string? MetrcSyncStatus { get; init; }
}

/// <summary>
/// Paginated list response
/// </summary>
public record ProcessingJobListResponse
{
    public List<ProcessingJobSummaryDto> Jobs { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

/// <summary>
/// Processing job type DTO
/// </summary>
public record ProcessingJobTypeDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public string? DefaultOutputCategory { get; init; }
    public decimal? ExpectedYieldPercent { get; init; }
    public int? EstimatedDurationHours { get; init; }
}



