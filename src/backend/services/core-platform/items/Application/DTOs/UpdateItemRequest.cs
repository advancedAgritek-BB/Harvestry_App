using System.ComponentModel.DataAnnotations;

namespace Harvestry.Items.Application.DTOs;

/// <summary>
/// Request to update an existing item
/// </summary>
public record UpdateItemRequest
{
    [StringLength(200, MinimumLength = 1)]
    public string? Name { get; init; }

    public string? Category { get; init; }

    public string? UnitOfMeasure { get; init; }

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
    public bool? RequiresLabTesting { get; init; }
    public string? DefaultLabTestingState { get; init; }

    // Details
    [StringLength(1000)]
    public string? Description { get; init; }

    [StringLength(100)]
    public string? Sku { get; init; }

    [StringLength(100)]
    public string? Barcode { get; init; }

    // WMS - Classification
    public string? InventoryCategory { get; init; }
    public bool? IsLotTracked { get; init; }
    public bool? IsSerialTracked { get; init; }

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
    public bool? IsSellable { get; init; }
    public bool? IsPurchasable { get; init; }
    public bool? IsProducible { get; init; }
    public bool? IsActiveForSale { get; init; }

    // WMS - Default locations
    public Guid? DefaultReceivingLocationId { get; init; }
    public Guid? DefaultStorageLocationId { get; init; }
    public Guid? DefaultProductionLocationId { get; init; }

    // WMS - Shelf life
    public int? ShelfLifeDays { get; init; }
    public bool? RequiresExpirationDate { get; init; }

    // WMS - Weight
    public decimal? StandardWeight { get; init; }
    public string? StandardWeightUom { get; init; }
    public decimal? WeightTolerancePercent { get; init; }
}




