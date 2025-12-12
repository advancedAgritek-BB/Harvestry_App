namespace Harvestry.Packages.Application.DTOs;

/// <summary>
/// Full package DTO with all fields
/// </summary>
public record PackageDto
{
    public Guid Id { get; init; }
    public Guid SiteId { get; init; }
    public string PackageLabel { get; init; } = string.Empty;

    // Item
    public Guid ItemId { get; init; }
    public string ItemName { get; init; } = string.Empty;
    public string ItemCategory { get; init; } = string.Empty;

    // Quantity
    public decimal Quantity { get; init; }
    public decimal InitialQuantity { get; init; }
    public string UnitOfMeasure { get; init; } = string.Empty;

    // Location
    public Guid? LocationId { get; init; }
    public string? LocationName { get; init; }
    public string? SublocationName { get; init; }

    // Source
    public Guid? SourceHarvestId { get; init; }
    public string? SourceHarvestName { get; init; }
    public List<string> SourcePackageLabels { get; init; } = new();

    // Production
    public string? ProductionBatchNumber { get; init; }
    public bool IsProductionBatch { get; init; }

    // Flags
    public bool IsTradeSample { get; init; }
    public bool IsDonation { get; init; }
    public bool ProductRequiresRemediation { get; init; }

    // Dates
    public DateOnly PackagedDate { get; init; }
    public DateOnly? ExpirationDate { get; init; }
    public DateOnly? UseByDate { get; init; }
    public DateOnly? FinishedDate { get; init; }

    // Lab testing
    public string LabTestingState { get; init; } = string.Empty;
    public bool LabTestingStateRequired { get; init; }

    // Potency
    public decimal? ThcPercent { get; init; }
    public decimal? CbdPercent { get; init; }

    // Status
    public string Status { get; init; } = string.Empty;
    public string PackageType { get; init; } = string.Empty;
    public string? Notes { get; init; }

    // METRC
    public long? MetrcPackageId { get; init; }
    public DateTime? MetrcLastSyncAt { get; init; }
    public string? MetrcSyncStatus { get; init; }

    // WMS - Costing
    public decimal? UnitCost { get; init; }
    public decimal MaterialCost { get; init; }
    public decimal LaborCost { get; init; }
    public decimal OverheadCost { get; init; }
    public decimal TotalCost { get; init; }
    public decimal TotalValue { get; init; }

    // WMS - Reservation
    public decimal ReservedQuantity { get; init; }
    public decimal AvailableQuantity { get; init; }

    // WMS - Classification
    public string InventoryCategory { get; init; } = "finished_good";

    // WMS - Hold
    public string? HoldReasonCode { get; init; }
    public DateTime? HoldPlacedAt { get; init; }
    public Guid? HoldPlacedByUserId { get; init; }
    public DateTime? HoldReleasedAt { get; init; }
    public bool RequiresTwoPersonRelease { get; init; }

    // WMS - Vendor
    public Guid? VendorId { get; init; }
    public string? VendorName { get; init; }
    public string? VendorLotNumber { get; init; }
    public Guid? PurchaseOrderId { get; init; }
    public string? PurchaseOrderNumber { get; init; }
    public DateOnly? ReceivedDate { get; init; }

    // WMS - Quality
    public string? Grade { get; init; }
    public decimal? QualityScore { get; init; }
    public string? QualityNotes { get; init; }

    // WMS - Lineage
    public int GenerationDepth { get; init; }
    public Guid? RootAncestorId { get; init; }

    // Audit
    public DateTime CreatedAt { get; init; }
    public Guid CreatedByUserId { get; init; }
    public DateTime UpdatedAt { get; init; }
    public Guid UpdatedByUserId { get; init; }
}

/// <summary>
/// Summary package DTO for lists
/// </summary>
public record PackageSummaryDto
{
    public Guid Id { get; init; }
    public string PackageLabel { get; init; } = string.Empty;
    public string ItemName { get; init; } = string.Empty;
    public string ItemCategory { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal AvailableQuantity { get; init; }
    public string UnitOfMeasure { get; init; } = string.Empty;
    public string? LocationName { get; init; }
    public string Status { get; init; } = string.Empty;
    public string LabTestingState { get; init; } = string.Empty;
    public DateOnly PackagedDate { get; init; }
    public DateOnly? ExpirationDate { get; init; }
    public decimal? UnitCost { get; init; }
    public decimal TotalValue { get; init; }
    public string InventoryCategory { get; init; } = "finished_good";
    public string? Grade { get; init; }
    public string? HoldReasonCode { get; init; }
    public long? MetrcPackageId { get; init; }
    public string? MetrcSyncStatus { get; init; }
}

/// <summary>
/// Paginated list response
/// </summary>
public record PackageListResponse
{
    public List<PackageSummaryDto> Packages { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

/// <summary>
/// Package summary statistics
/// </summary>
public record PackageSummaryStatsDto
{
    public int TotalPackages { get; init; }
    public int ActivePackages { get; init; }
    public int OnHoldPackages { get; init; }
    public int FinishedPackages { get; init; }
    public decimal TotalQuantity { get; init; }
    public decimal TotalValue { get; init; }
    public int ExpiringInWeek { get; init; }
    public int ExpiringInMonth { get; init; }
    public int PendingLabTest { get; init; }
    public int FailedLabTest { get; init; }
}




