using Harvestry.Items.Application.Interfaces;
using Harvestry.Items.Domain.Entities;
using Harvestry.Items.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Items.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for Item entities
/// </summary>
public class ItemRepository : IItemRepository
{
    private readonly ItemsDbContext _context;

    public ItemRepository(ItemsDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Item?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Items.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Item?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Items
            .FirstOrDefaultAsync(i => i.SiteId == siteId && i.Id == id, cancellationToken);
    }

    public async Task<Item?> GetBySkuAsync(Guid siteId, string sku, CancellationToken cancellationToken = default)
    {
        return await _context.Items
            .FirstOrDefaultAsync(i => i.SiteId == siteId && i.Sku == sku, cancellationToken);
    }

    public async Task<Item?> GetByBarcodeAsync(Guid siteId, string barcode, CancellationToken cancellationToken = default)
    {
        return await _context.Items
            .FirstOrDefaultAsync(i => i.SiteId == siteId && i.Barcode == barcode, cancellationToken);
    }

    public async Task<(List<Item> Items, int TotalCount)> GetBySiteAsync(
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
        CancellationToken cancellationToken = default)
    {
        var query = _context.Items.Where(i => i.SiteId == siteId);

        if (category.HasValue)
            query = query.Where(i => i.Category == category.Value);

        if (status.HasValue)
            query = query.Where(i => i.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(inventoryCategory))
            query = query.Where(i => i.InventoryCategory == inventoryCategory);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(i =>
                i.Name.ToLower().Contains(searchLower) ||
                (i.Sku != null && i.Sku.ToLower().Contains(searchLower)) ||
                (i.Barcode != null && i.Barcode.ToLower().Contains(searchLower)) ||
                (i.StrainName != null && i.StrainName.ToLower().Contains(searchLower)));
        }

        if (isSellable.HasValue)
            query = query.Where(i => i.IsSellable == isSellable.Value);

        if (isPurchasable.HasValue)
            query = query.Where(i => i.IsPurchasable == isPurchasable.Value);

        if (isProducible.HasValue)
            query = query.Where(i => i.IsProducible == isProducible.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(i => i.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<List<Item>> GetByIdsAsync(Guid siteId, IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await _context.Items
            .Where(i => i.SiteId == siteId && ids.Contains(i.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Item>> GetByCategoryAsync(Guid siteId, ItemCategory category, CancellationToken cancellationToken = default)
    {
        return await _context.Items
            .Where(i => i.SiteId == siteId && i.Category == category && i.Status == ItemStatus.Active)
            .OrderBy(i => i.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LowStockItem>> GetLowStockItemsAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        // This would typically join with packages table to get current quantities
        // For now, returning items that have reorder points set
        var itemsWithReorderPoints = await _context.Items
            .Where(i => i.SiteId == siteId && i.Status == ItemStatus.Active && i.ReorderPoint != null)
            .ToListAsync(cancellationToken);

        // In a real implementation, this would query the packages table to get actual stock levels
        // For now, return items with reorder points as placeholders
        return itemsWithReorderPoints.Select(i => new LowStockItem
        {
            ItemId = i.Id,
            ItemName = i.Name,
            Sku = i.Sku,
            Category = i.Category,
            InventoryCategory = i.InventoryCategory,
            ReorderPoint = i.ReorderPoint,
            ReorderQuantity = i.ReorderQuantity,
            SafetyStock = i.SafetyStock,
            LeadTimeDays = i.LeadTimeDays,
            CurrentQuantity = 0, // Would be calculated from packages
            ReservedQuantity = 0, // Would be calculated from packages
            AvailableQuantity = 0, // Would be calculated from packages
            CostPrice = i.CostPrice
        }).ToList();
    }

    public async Task<List<CategorySummary>> GetCategorySummaryAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        return await _context.Items
            .Where(i => i.SiteId == siteId)
            .GroupBy(i => i.Category)
            .Select(g => new CategorySummary
            {
                Category = g.Key,
                TotalItems = g.Count(),
                ActiveItems = g.Count(i => i.Status == ItemStatus.Active),
                InactiveItems = g.Count(i => i.Status == ItemStatus.Inactive),
                SellableItems = g.Count(i => i.IsSellable),
                LowStockItems = 0 // Would need to join with packages for accurate count
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SkuExistsAsync(Guid siteId, string sku, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Items.Where(i => i.SiteId == siteId && i.Sku == sku);
        if (excludeId.HasValue)
            query = query.Where(i => i.Id != excludeId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> BarcodeExistsAsync(Guid siteId, string barcode, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Items.Where(i => i.SiteId == siteId && i.Barcode == barcode);
        if (excludeId.HasValue)
            query = query.Where(i => i.Id != excludeId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<Item> AddAsync(Item item, CancellationToken cancellationToken = default)
    {
        await _context.Items.AddAsync(item, cancellationToken);
        return item;
    }

    public Task<Item> UpdateAsync(Item item, CancellationToken cancellationToken = default)
    {
        _context.Items.Update(item);
        return Task.FromResult(item);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await GetByIdAsync(id, cancellationToken);
        if (item != null)
        {
            _context.Items.Remove(item);
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}




