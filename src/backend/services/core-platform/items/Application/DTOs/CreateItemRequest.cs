using System.ComponentModel.DataAnnotations;

namespace Harvestry.Items.Application.DTOs;

/// <summary>
/// Request to create a new item
/// </summary>
public record CreateItemRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; init; } = string.Empty;

    [Required]
    public string Category { get; init; } = string.Empty;

    [Required]
    public string UnitOfMeasure { get; init; } = string.Empty;

    // Strain
    public Guid? StrainId { get; init; }
    public string? StrainName { get; init; }

    // Unit weight
    public decimal? UnitWeight { get; init; }
    public string? UnitWeightUnitOfMeasure { get; init; }

    // Potency
    public decimal? DefaultThcPercent { get; init; }
    public decimal? DefaultCbdPercent { get; init; }

    // Lab testing
    public bool RequiresLabTesting { get; init; }
    public string? DefaultLabTestingState { get; init; }

    // Details
    [StringLength(1000)]
    public string? Description { get; init; }

    [StringLength(100)]
    public string? Sku { get; init; }

    [StringLength(100)]
    public string? Barcode { get; init; }

    // WMS - Classification
    public string InventoryCategory { get; init; } = "finished_good";
    public bool IsLotTracked { get; init; } = true;
    public bool IsSerialTracked { get; init; }

    // WMS - Reorder
    public decimal? ReorderPoint { get; init; }
    public decimal? ReorderQuantity { get; init; }
    public decimal? SafetyStock { get; init; }
    public int? LeadTimeDays { get; init; }

    // WMS - Pricing
    public decimal? ListPrice { get; init; }
    public decimal? WholesalePrice { get; init; }
    public decimal? CostPrice { get; init; }

    // WMS - Flags
    public bool IsSellable { get; init; } = true;
    public bool IsPurchasable { get; init; }
    public bool IsProducible { get; init; }

    // WMS - Default locations
    public Guid? DefaultReceivingLocationId { get; init; }
    public Guid? DefaultStorageLocationId { get; init; }
    public Guid? DefaultProductionLocationId { get; init; }

    // WMS - Shelf life
    public int? ShelfLifeDays { get; init; }
    public bool RequiresExpirationDate { get; init; }
}



