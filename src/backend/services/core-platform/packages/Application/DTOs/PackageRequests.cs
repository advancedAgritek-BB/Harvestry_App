using System.ComponentModel.DataAnnotations;

namespace Harvestry.Packages.Application.DTOs;

/// <summary>
/// Request to create a new package
/// </summary>
public record CreatePackageRequest
{
    [Required]
    [StringLength(30, MinimumLength = 1)]
    public string PackageLabel { get; init; } = string.Empty;

    [Required]
    public Guid ItemId { get; init; }

    [Required]
    [StringLength(200)]
    public string ItemName { get; init; } = string.Empty;

    public string? ItemCategory { get; init; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Quantity { get; init; }

    [Required]
    public string UnitOfMeasure { get; init; } = string.Empty;

    public Guid? LocationId { get; init; }
    public string? LocationName { get; init; }
    public string? SublocationName { get; init; }

    public Guid? SourceHarvestId { get; init; }
    public string? SourceHarvestName { get; init; }
    public List<string>? SourcePackageLabels { get; init; }

    public DateOnly? PackagedDate { get; init; }
    public DateOnly? ExpirationDate { get; init; }

    // WMS fields
    public string? InventoryCategory { get; init; }
    public decimal? UnitCost { get; init; }
    public decimal? MaterialCost { get; init; }
    public decimal? LaborCost { get; init; }
    public decimal? OverheadCost { get; init; }

    // Vendor info
    public Guid? VendorId { get; init; }
    public string? VendorName { get; init; }
    public string? VendorLotNumber { get; init; }
    public DateOnly? ReceivedDate { get; init; }

    // Quality
    public string? Grade { get; init; }
    public decimal? QualityScore { get; init; }

    public string? Notes { get; init; }
}

/// <summary>
/// Request to update a package
/// </summary>
public record UpdatePackageRequest
{
    public Guid? LocationId { get; init; }
    public string? LocationName { get; init; }
    public string? SublocationName { get; init; }
    public DateOnly? ExpirationDate { get; init; }
    public DateOnly? UseByDate { get; init; }
    public string? Notes { get; init; }

    // WMS fields
    public string? InventoryCategory { get; init; }
    public decimal? UnitCost { get; init; }
    public string? Grade { get; init; }
    public decimal? QualityScore { get; init; }
    public string? QualityNotes { get; init; }
}

/// <summary>
/// Request to adjust package quantity
/// </summary>
public record AdjustPackageRequest
{
    [Required]
    public decimal AdjustmentQuantity { get; init; }

    [Required]
    public string Reason { get; init; } = string.Empty;

    public DateOnly? AdjustmentDate { get; init; }
    public string? ReasonNote { get; init; }
    public bool RequiresApproval { get; init; }
}

/// <summary>
/// Request to move package to new location
/// </summary>
public record MovePackageRequest
{
    [Required]
    public Guid ToLocationId { get; init; }

    public string? ToLocationPath { get; init; }
    public string? SublocationName { get; init; }
    public string? Notes { get; init; }
    public string? BarcodeScanned { get; init; }
}

/// <summary>
/// Request to split a package
/// </summary>
public record SplitPackageRequest
{
    [Required]
    public Guid SourcePackageId { get; init; }

    [Required]
    public List<SplitTargetDto> Targets { get; init; } = new();

    public string? Notes { get; init; }
}

public record SplitTargetDto
{
    [Required]
    public string PackageLabel { get; init; } = string.Empty;

    [Required]
    [Range(0.0001, double.MaxValue)]
    public decimal Quantity { get; init; }

    public Guid? LocationId { get; init; }
}

/// <summary>
/// Request to merge packages
/// </summary>
public record MergePackagesRequest
{
    [Required]
    [MinLength(2)]
    public List<Guid> SourcePackageIds { get; init; } = new();

    [Required]
    public string TargetPackageLabel { get; init; } = string.Empty;

    public Guid? LocationId { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Request to place package on hold
/// </summary>
public record PlaceHoldRequest
{
    [Required]
    public string ReasonCode { get; init; } = string.Empty;

    public bool RequiresTwoPersonRelease { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Request to release hold
/// </summary>
public record ReleaseHoldRequest
{
    public string? Notes { get; init; }
}

/// <summary>
/// Request to reserve quantity
/// </summary>
public record ReserveQuantityRequest
{
    [Required]
    [Range(0.0001, double.MaxValue)]
    public decimal Quantity { get; init; }

    public Guid? OrderId { get; init; }
    public string? OrderNumber { get; init; }
}



