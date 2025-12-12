using Harvestry.Items.Application.DTOs;
using Harvestry.Items.Domain.Entities;

namespace Harvestry.Items.Application.Mappers;

/// <summary>
/// Extension methods for mapping Item entities to DTOs
/// </summary>
public static class ItemMapper
{
    /// <summary>
    /// Map Item entity to full DTO
    /// </summary>
    public static ItemDto ToDto(this Item item)
    {
        return new ItemDto
        {
            Id = item.Id,
            SiteId = item.SiteId,
            Name = item.Name,
            Category = item.Category.ToString(),
            UnitOfMeasure = item.UnitOfMeasure.ToString(),
            Status = item.Status.ToString(),
            StrainId = item.StrainId,
            StrainName = item.StrainName,
            UnitWeight = item.UnitWeight,
            UnitWeightUnitOfMeasure = item.UnitWeightUnitOfMeasure,
            DefaultThcPercent = item.DefaultThcPercent,
            DefaultThcContent = item.DefaultThcContent,
            DefaultThcContentUnitOfMeasure = item.DefaultThcContentUnitOfMeasure,
            DefaultCbdPercent = item.DefaultCbdPercent,
            DefaultCbdContent = item.DefaultCbdContent,
            RequiresLabTesting = item.RequiresLabTesting,
            DefaultLabTestingState = item.DefaultLabTestingState,
            Description = item.Description,
            Sku = item.Sku,
            Barcode = item.Barcode,
            MetrcItemId = item.MetrcItemId,
            MetrcLastSyncAt = item.MetrcLastSyncAt,
            MetrcSyncStatus = item.MetrcSyncStatus,
            InventoryCategory = item.InventoryCategory,
            IsLotTracked = item.IsLotTracked,
            IsSerialTracked = item.IsSerialTracked,
            ReorderPoint = item.ReorderPoint,
            ReorderQuantity = item.ReorderQuantity,
            SafetyStock = item.SafetyStock,
            LeadTimeDays = item.LeadTimeDays,
            MinOrderQuantity = item.MinOrderQuantity,
            MaxOrderQuantity = item.MaxOrderQuantity,
            ListPrice = item.ListPrice,
            WholesalePrice = item.WholesalePrice,
            CostPrice = item.CostPrice,
            MarginPercent = item.MarginPercent,
            IsSellable = item.IsSellable,
            IsPurchasable = item.IsPurchasable,
            IsProducible = item.IsProducible,
            IsActiveForSale = item.IsActiveForSale,
            DefaultReceivingLocationId = item.DefaultReceivingLocationId,
            DefaultStorageLocationId = item.DefaultStorageLocationId,
            DefaultProductionLocationId = item.DefaultProductionLocationId,
            ShelfLifeDays = item.ShelfLifeDays,
            RequiresExpirationDate = item.RequiresExpirationDate,
            StandardWeight = item.StandardWeight,
            StandardWeightUom = item.StandardWeightUom,
            WeightTolerancePercent = item.WeightTolerancePercent,
            CreatedAt = item.CreatedAt,
            CreatedByUserId = item.CreatedByUserId,
            UpdatedAt = item.UpdatedAt,
            UpdatedByUserId = item.UpdatedByUserId
        };
    }

    /// <summary>
    /// Map Item entity to summary DTO
    /// </summary>
    public static ItemSummaryDto ToSummaryDto(this Item item)
    {
        return new ItemSummaryDto
        {
            Id = item.Id,
            Name = item.Name,
            Category = item.Category.ToString(),
            UnitOfMeasure = item.UnitOfMeasure.ToString(),
            Status = item.Status.ToString(),
            Sku = item.Sku,
            Barcode = item.Barcode,
            StrainName = item.StrainName,
            InventoryCategory = item.InventoryCategory,
            ListPrice = item.ListPrice,
            CostPrice = item.CostPrice,
            ReorderPoint = item.ReorderPoint,
            IsSellable = item.IsSellable,
            IsActiveForSale = item.IsActiveForSale,
            MetrcItemId = item.MetrcItemId,
            MetrcSyncStatus = item.MetrcSyncStatus
        };
    }
}




