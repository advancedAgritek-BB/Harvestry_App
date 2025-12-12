using Harvestry.Items.Application.DTOs;

namespace Harvestry.Items.Application.Interfaces;

/// <summary>
/// Service interface for Item operations
/// </summary>
public interface IItemService
{
    /// <summary>
    /// Get item by ID
    /// </summary>
    Task<ItemDto?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get item by SKU
    /// </summary>
    Task<ItemDto?> GetBySkuAsync(Guid siteId, string sku, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get item by barcode
    /// </summary>
    Task<ItemDto?> GetByBarcodeAsync(Guid siteId, string barcode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get items with pagination and filtering
    /// </summary>
    Task<ItemListResponse> GetItemsAsync(
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
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new item
    /// </summary>
    Task<ItemDto> CreateAsync(Guid siteId, CreateItemRequest request, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing item
    /// </summary>
    Task<ItemDto?> UpdateAsync(Guid siteId, Guid id, UpdateItemRequest request, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete an item
    /// </summary>
    Task<bool> DeleteAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activate an item
    /// </summary>
    Task<ItemDto?> ActivateAsync(Guid siteId, Guid id, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivate an item
    /// </summary>
    Task<ItemDto?> DeactivateAsync(Guid siteId, Guid id, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get low stock alerts
    /// </summary>
    Task<List<LowStockAlertDto>> GetLowStockAlertsAsync(Guid siteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get category summary
    /// </summary>
    Task<List<ItemCategorySummaryDto>> GetCategorySummaryAsync(Guid siteId, CancellationToken cancellationToken = default);
}




