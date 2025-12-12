namespace Harvestry.Items.Application.DTOs;

/// <summary>
/// Full item DTO with all fields
/// </summary>
public record ItemDto
{
    public Guid Id { get; init; }
    public Guid SiteId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string UnitOfMeasure { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;

    // Strain
    public Guid? StrainId { get; init; }
    public string? StrainName { get; init; }

    // Unit weight
    public decimal? UnitWeight { get; init; }
    public string? UnitWeightUnitOfMeasure { get; init; }

    // Potency
    public decimal? DefaultThcPercent { get; init; }
    public decimal? DefaultThcContent { get; init; }
    public string? DefaultThcContentUnitOfMeasure { get; init; }
    public decimal? DefaultCbdPercent { get; init; }
    public decimal? DefaultCbdContent { get; init; }

    // Lab testing
    public bool RequiresLabTesting { get; init; }
    public string? DefaultLabTestingState { get; init; }

    // Details
    public string? Description { get; init; }
    public string? Sku { get; init; }
    public string? Barcode { get; init; }

    // METRC
    public long? MetrcItemId { get; init; }
    public DateTime? MetrcLastSyncAt { get; init; }
    public string? MetrcSyncStatus { get; init; }

    // WMS - Classification
    public string InventoryCategory { get; init; } = "finished_good";
    public bool IsLotTracked { get; init; }
    public bool IsSerialTracked { get; init; }

    // WMS - Reorder
    public decimal? ReorderPoint { get; init; }
    public decimal? ReorderQuantity { get; init; }
    public decimal? SafetyStock { get; init; }
    public int? LeadTimeDays { get; init; }
    public decimal? MinOrderQuantity { get; init; }
    public decimal? MaxOrderQuantity { get; init; }

    // WMS - Pricing
    public decimal? ListPrice { get; init; }
    public decimal? WholesalePrice { get; init; }
    public decimal? CostPrice { get; init; }
    public decimal? MarginPercent { get; init; }

    // WMS - Flags
    public bool IsSellable { get; init; }
    public bool IsPurchasable { get; init; }
    public bool IsProducible { get; init; }
    public bool IsActiveForSale { get; init; }

    // WMS - Default locations
    public Guid? DefaultReceivingLocationId { get; init; }
    public Guid? DefaultStorageLocationId { get; init; }
    public Guid? DefaultProductionLocationId { get; init; }

    // WMS - Shelf life
    public int? ShelfLifeDays { get; init; }
    public bool RequiresExpirationDate { get; init; }

    // WMS - Weight
    public decimal? StandardWeight { get; init; }
    public string? StandardWeightUom { get; init; }
    public decimal WeightTolerancePercent { get; init; }

    // Audit
    public DateTime CreatedAt { get; init; }
    public Guid CreatedByUserId { get; init; }
    public DateTime UpdatedAt { get; init; }
    public Guid UpdatedByUserId { get; init; }
}

/// <summary>
/// Summary item DTO for lists
/// </summary>
public record ItemSummaryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string UnitOfMeasure { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? Sku { get; init; }
    public string? Barcode { get; init; }
    public string? StrainName { get; init; }
    public string InventoryCategory { get; init; } = "finished_good";
    public decimal? ListPrice { get; init; }
    public decimal? CostPrice { get; init; }
    public decimal? ReorderPoint { get; init; }
    public bool IsSellable { get; init; }
    public bool IsActiveForSale { get; init; }
    public long? MetrcItemId { get; init; }
    public string? MetrcSyncStatus { get; init; }
}

/// <summary>
/// Paginated list response
/// </summary>
public record ItemListResponse
{
    public List<ItemSummaryDto> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

/// <summary>
/// Low stock alert DTO
/// </summary>
public record LowStockAlertDto
{
    public Guid ItemId { get; init; }
    public string ItemName { get; init; } = string.Empty;
    public string? Sku { get; init; }
    public string Category { get; init; } = string.Empty;
    public string InventoryCategory { get; init; } = string.Empty;
    public decimal? ReorderPoint { get; init; }
    public decimal? ReorderQuantity { get; init; }
    public decimal? SafetyStock { get; init; }
    public int? LeadTimeDays { get; init; }
    public decimal CurrentQuantity { get; init; }
    public decimal ReservedQuantity { get; init; }
    public decimal AvailableQuantity { get; init; }
    public decimal Shortage { get; init; }
    public string StockStatus { get; init; } = string.Empty; // critical, low, ok
    public decimal? CostPrice { get; init; }
    public decimal ShortageValue { get; init; }
}

/// <summary>
/// Category summary DTO
/// </summary>
public record ItemCategorySummaryDto
{
    public string Category { get; init; } = string.Empty;
    public int TotalItems { get; init; }
    public int ActiveItems { get; init; }
    public int InactiveItems { get; init; }
    public int SellableItems { get; init; }
    public int LowStockItems { get; init; }
}




