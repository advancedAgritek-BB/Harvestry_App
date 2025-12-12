using Harvestry.Items.Application.DTOs;
using Harvestry.Items.Application.Interfaces;
using Harvestry.Items.Application.Mappers;
using Harvestry.Items.Domain.Entities;
using Harvestry.Items.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Harvestry.Items.Application.Services;

/// <summary>
/// Service implementation for Item operations
/// </summary>
public class ItemService : IItemService
{
    private readonly IItemRepository _repository;
    private readonly ILogger<ItemService> _logger;

    public ItemService(IItemRepository repository, ILogger<ItemService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ItemDto?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(siteId, id, cancellationToken);
        return item?.ToDto();
    }

    public async Task<ItemDto?> GetBySkuAsync(Guid siteId, string sku, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetBySkuAsync(siteId, sku, cancellationToken);
        return item?.ToDto();
    }

    public async Task<ItemDto?> GetByBarcodeAsync(Guid siteId, string barcode, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByBarcodeAsync(siteId, barcode, cancellationToken);
        return item?.ToDto();
    }

    public async Task<ItemListResponse> GetItemsAsync(
        Guid siteId,
        int page = 1,
        int pageSize = 50,
        string? category = null,
        string? status = null,
        string? inventoryCategory = null,
        string? search = null,
        bool? isSellable = null,
        bool? isPurchasable = null,
        bool? isProducible = null,
        CancellationToken cancellationToken = default)
    {
        ItemCategory? categoryEnum = null;
        if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<ItemCategory>(category, true, out var cat))
        {
            categoryEnum = cat;
        }

        ItemStatus? statusEnum = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ItemStatus>(status, true, out var stat))
        {
            statusEnum = stat;
        }

        var (items, totalCount) = await _repository.GetBySiteAsync(
            siteId, page, pageSize, categoryEnum, statusEnum, inventoryCategory, search,
            isSellable, isPurchasable, isProducible, cancellationToken);

        return new ItemListResponse
        {
            Items = items.Select(i => i.ToSummaryDto()).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ItemDto> CreateAsync(Guid siteId, CreateItemRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        // Validate SKU uniqueness
        if (!string.IsNullOrWhiteSpace(request.Sku))
        {
            if (await _repository.SkuExistsAsync(siteId, request.Sku, cancellationToken: cancellationToken))
            {
                throw new InvalidOperationException($"SKU '{request.Sku}' already exists");
            }
        }

        // Validate barcode uniqueness
        if (!string.IsNullOrWhiteSpace(request.Barcode))
        {
            if (await _repository.BarcodeExistsAsync(siteId, request.Barcode, cancellationToken: cancellationToken))
            {
                throw new InvalidOperationException($"Barcode '{request.Barcode}' already exists");
            }
        }

        var category = Enum.Parse<ItemCategory>(request.Category, true);
        var unitOfMeasure = Enum.Parse<UnitOfMeasure>(request.UnitOfMeasure, true);

        var item = Item.Create(
            siteId,
            request.Name,
            category,
            unitOfMeasure,
            userId,
            request.StrainId,
            request.StrainName);

        // Set additional properties
        item.UpdateDetails(
            request.Description,
            request.Sku,
            request.Barcode,
            userId);

        if (request.UnitWeight.HasValue)
        {
            item.SetUnitWeight(request.UnitWeight.Value, request.UnitWeightUnitOfMeasure ?? "g", userId);
        }

        if (request.DefaultThcPercent.HasValue || request.DefaultCbdPercent.HasValue)
        {
            item.SetDefaultPotency(
                request.DefaultThcPercent,
                null,
                null,
                request.DefaultCbdPercent,
                null,
                userId);
        }

        if (request.RequiresLabTesting)
        {
            item.SetLabTestingRequirements(true, request.DefaultLabTestingState, userId);
        }

        // WMS properties
        item.SetInventoryCategory(request.InventoryCategory, userId);
        item.SetTrackingFlags(request.IsLotTracked, request.IsSerialTracked, userId);

        if (request.ReorderPoint.HasValue || request.ReorderQuantity.HasValue)
        {
            item.SetReorderParameters(request.ReorderPoint, request.ReorderQuantity, request.SafetyStock, request.LeadTimeDays, userId);
        }

        if (request.ListPrice.HasValue || request.CostPrice.HasValue)
        {
            item.SetPricing(request.ListPrice, request.WholesalePrice, request.CostPrice, null, userId);
        }

        item.SetProductFlags(request.IsSellable, request.IsPurchasable, request.IsProducible, true, userId);

        if (request.DefaultReceivingLocationId.HasValue || request.DefaultStorageLocationId.HasValue)
        {
            item.SetDefaultLocations(request.DefaultReceivingLocationId, request.DefaultStorageLocationId, request.DefaultProductionLocationId, userId);
        }

        if (request.ShelfLifeDays.HasValue || request.RequiresExpirationDate)
        {
            item.SetShelfLife(request.ShelfLifeDays, request.RequiresExpirationDate, userId);
        }

        var created = await _repository.AddAsync(item, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created item {ItemId} for site {SiteId}", created.Id, siteId);

        return created.ToDto();
    }

    public async Task<ItemDto?> UpdateAsync(Guid siteId, Guid id, UpdateItemRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(siteId, id, cancellationToken);
        if (item == null) return null;

        // Validate SKU uniqueness
        if (!string.IsNullOrWhiteSpace(request.Sku))
        {
            if (await _repository.SkuExistsAsync(siteId, request.Sku, id, cancellationToken))
            {
                throw new InvalidOperationException($"SKU '{request.Sku}' already exists");
            }
        }

        // Validate barcode uniqueness
        if (!string.IsNullOrWhiteSpace(request.Barcode))
        {
            if (await _repository.BarcodeExistsAsync(siteId, request.Barcode, id, cancellationToken))
            {
                throw new InvalidOperationException($"Barcode '{request.Barcode}' already exists");
            }
        }

        // Update basic properties
        if (!string.IsNullOrWhiteSpace(request.Name) || 
            !string.IsNullOrWhiteSpace(request.Description) ||
            !string.IsNullOrWhiteSpace(request.Sku) ||
            !string.IsNullOrWhiteSpace(request.Barcode))
        {
            item.UpdateDetails(
                request.Description ?? item.Description,
                request.Sku ?? item.Sku,
                request.Barcode ?? item.Barcode,
                userId);
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            item.UpdateName(request.Name, userId);
        }

        // Update WMS properties
        if (!string.IsNullOrWhiteSpace(request.InventoryCategory))
        {
            item.SetInventoryCategory(request.InventoryCategory, userId);
        }

        if (request.IsLotTracked.HasValue || request.IsSerialTracked.HasValue)
        {
            item.SetTrackingFlags(
                request.IsLotTracked ?? item.IsLotTracked,
                request.IsSerialTracked ?? item.IsSerialTracked,
                userId);
        }

        item.SetReorderParameters(
            request.ReorderPoint ?? item.ReorderPoint,
            request.ReorderQuantity ?? item.ReorderQuantity,
            request.SafetyStock ?? item.SafetyStock,
            request.LeadTimeDays ?? item.LeadTimeDays,
            userId);

        if (request.MinOrderQuantity.HasValue || request.MaxOrderQuantity.HasValue)
        {
            item.SetOrderQuantityConstraints(
                request.MinOrderQuantity ?? item.MinOrderQuantity,
                request.MaxOrderQuantity ?? item.MaxOrderQuantity,
                userId);
        }

        item.SetPricing(
            request.ListPrice ?? item.ListPrice,
            request.WholesalePrice ?? item.WholesalePrice,
            request.CostPrice ?? item.CostPrice,
            request.MarginPercent ?? item.MarginPercent,
            userId);

        if (request.IsSellable.HasValue || request.IsPurchasable.HasValue || 
            request.IsProducible.HasValue || request.IsActiveForSale.HasValue)
        {
            item.SetProductFlags(
                request.IsSellable ?? item.IsSellable,
                request.IsPurchasable ?? item.IsPurchasable,
                request.IsProducible ?? item.IsProducible,
                request.IsActiveForSale ?? item.IsActiveForSale,
                userId);
        }

        item.SetDefaultLocations(
            request.DefaultReceivingLocationId ?? item.DefaultReceivingLocationId,
            request.DefaultStorageLocationId ?? item.DefaultStorageLocationId,
            request.DefaultProductionLocationId ?? item.DefaultProductionLocationId,
            userId);

        item.SetShelfLife(
            request.ShelfLifeDays ?? item.ShelfLifeDays,
            request.RequiresExpirationDate ?? item.RequiresExpirationDate,
            userId);

        if (request.StandardWeight.HasValue || !string.IsNullOrWhiteSpace(request.StandardWeightUom))
        {
            item.SetWeightParameters(
                request.StandardWeight ?? item.StandardWeight,
                request.StandardWeightUom ?? item.StandardWeightUom,
                request.WeightTolerancePercent ?? item.WeightTolerancePercent,
                userId);
        }

        await _repository.UpdateAsync(item, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated item {ItemId} for site {SiteId}", id, siteId);

        return item.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(siteId, id, cancellationToken);
        if (item == null) return false;

        await _repository.DeleteAsync(id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted item {ItemId} from site {SiteId}", id, siteId);

        return true;
    }

    public async Task<ItemDto?> ActivateAsync(Guid siteId, Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(siteId, id, cancellationToken);
        if (item == null) return null;

        item.Activate(userId);
        await _repository.UpdateAsync(item, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return item.ToDto();
    }

    public async Task<ItemDto?> DeactivateAsync(Guid siteId, Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(siteId, id, cancellationToken);
        if (item == null) return null;

        item.Deactivate(userId);
        await _repository.UpdateAsync(item, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return item.ToDto();
    }

    public async Task<List<LowStockAlertDto>> GetLowStockAlertsAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var lowStockItems = await _repository.GetLowStockItemsAsync(siteId, cancellationToken);

        return lowStockItems.Select(i => new LowStockAlertDto
        {
            ItemId = i.ItemId,
            ItemName = i.ItemName,
            Sku = i.Sku,
            Category = i.Category.ToString(),
            InventoryCategory = i.InventoryCategory,
            ReorderPoint = i.ReorderPoint,
            ReorderQuantity = i.ReorderQuantity,
            SafetyStock = i.SafetyStock,
            LeadTimeDays = i.LeadTimeDays,
            CurrentQuantity = i.CurrentQuantity,
            ReservedQuantity = i.ReservedQuantity,
            AvailableQuantity = i.AvailableQuantity,
            Shortage = (i.ReorderPoint ?? 0) - i.CurrentQuantity,
            StockStatus = DetermineStockStatus(i.CurrentQuantity, i.ReorderPoint, i.SafetyStock),
            CostPrice = i.CostPrice,
            ShortageValue = ((i.ReorderPoint ?? 0) - i.CurrentQuantity) * (i.CostPrice ?? 0)
        }).ToList();
    }

    public async Task<List<ItemCategorySummaryDto>> GetCategorySummaryAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var summaries = await _repository.GetCategorySummaryAsync(siteId, cancellationToken);

        return summaries.Select(s => new ItemCategorySummaryDto
        {
            Category = s.Category.ToString(),
            TotalItems = s.TotalItems,
            ActiveItems = s.ActiveItems,
            InactiveItems = s.InactiveItems,
            SellableItems = s.SellableItems,
            LowStockItems = s.LowStockItems
        }).ToList();
    }

    private static string DetermineStockStatus(decimal currentQuantity, decimal? reorderPoint, decimal? safetyStock)
    {
        if (currentQuantity <= (safetyStock ?? 0)) return "critical";
        if (currentQuantity <= (reorderPoint ?? 0)) return "low";
        return "ok";
    }
}




