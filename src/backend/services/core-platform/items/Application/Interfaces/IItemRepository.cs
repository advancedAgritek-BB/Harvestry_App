using Harvestry.Items.Domain.Entities;
using Harvestry.Items.Domain.Enums;

namespace Harvestry.Items.Application.Interfaces;

/// <summary>
/// Repository interface for Item entities
/// </summary>
public interface IItemRepository
{
    /// <summary>
    /// Get item by ID
    /// </summary>
    Task<Item?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get item by ID within a specific site
    /// </summary>
    Task<Item?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get item by SKU
    /// </summary>
    Task<Item?> GetBySkuAsync(Guid siteId, string sku, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get item by barcode
    /// </summary>
    Task<Item?> GetByBarcodeAsync(Guid siteId, string barcode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get items by site with pagination and filtering
    /// </summary>
    Task<(List<Item> Items, int TotalCount)> GetBySiteAsync(
        Guid siteId,
        int page = 1,
        int pageSize = 50,
        ItemCategory? category = null,
        ItemStatus? status = null,
        string? inventoryCategory = null,
        string? search = null,
        bool? isSellable = null,
        bool? isPurchasable = null,
        bool? isProducible = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get items by IDs
    /// </summary>
    Task<List<Item>> GetByIdsAsync(Guid siteId, IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get items by category
    /// </summary>
    Task<List<Item>> GetByCategoryAsync(Guid siteId, ItemCategory category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get low stock items
    /// </summary>
    Task<List<LowStockItem>> GetLowStockItemsAsync(Guid siteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get category summary
    /// </summary>
    Task<List<CategorySummary>> GetCategorySummaryAsync(Guid siteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if SKU exists
    /// </summary>
    Task<bool> SkuExistsAsync(Guid siteId, string sku, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if barcode exists
    /// </summary>
    Task<bool> BarcodeExistsAsync(Guid siteId, string barcode, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new item
    /// </summary>
    Task<Item> AddAsync(Item item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing item
    /// </summary>
    Task<Item> UpdateAsync(Item item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete an item
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save changes
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Low stock item projection
/// </summary>
public record LowStockItem
{
    public Guid ItemId { get; init; }
    public string ItemName { get; init; } = string.Empty;
    public string? Sku { get; init; }
    public ItemCategory Category { get; init; }
    public string InventoryCategory { get; init; } = string.Empty;
    public decimal? ReorderPoint { get; init; }
    public decimal? ReorderQuantity { get; init; }
    public decimal? SafetyStock { get; init; }
    public int? LeadTimeDays { get; init; }
    public decimal CurrentQuantity { get; init; }
    public decimal ReservedQuantity { get; init; }
    public decimal AvailableQuantity { get; init; }
    public decimal? CostPrice { get; init; }
}

/// <summary>
/// Category summary projection
/// </summary>
public record CategorySummary
{
    public ItemCategory Category { get; init; }
    public int TotalItems { get; init; }
    public int ActiveItems { get; init; }
    public int InactiveItems { get; init; }
    public int SellableItems { get; init; }
    public int LowStockItems { get; init; }
}




