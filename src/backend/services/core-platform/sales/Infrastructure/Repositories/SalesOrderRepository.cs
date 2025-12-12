using Harvestry.Sales.Application.Interfaces;
using Harvestry.Sales.Domain.Entities;
using Harvestry.Sales.Domain.Enums;
using Harvestry.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Sales.Infrastructure.Repositories;

public sealed class SalesOrderRepository : ISalesOrderRepository
{
    private readonly SalesDbContext _db;

    public SalesOrderRepository(SalesDbContext db)
    {
        _db = db;
    }

    public Task<SalesOrder?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default)
        => _db.SalesOrders.FirstOrDefaultAsync(o => o.SiteId == siteId && o.Id == id, cancellationToken);

    public Task<SalesOrder?> GetByOrderNumberAsync(Guid siteId, string orderNumber, CancellationToken cancellationToken = default)
        => _db.SalesOrders.FirstOrDefaultAsync(o => o.SiteId == siteId && o.OrderNumber == orderNumber, cancellationToken);

    public async Task<(List<SalesOrder> Orders, int TotalCount)> GetBySiteAsync(
        Guid siteId,
        int page = 1,
        int pageSize = 50,
        string? status = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.SalesOrders.Where(o => o.SiteId == siteId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<SalesOrderStatus>(status, true, out var s))
        {
            query = query.Where(o => o.Status == s);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLowerInvariant();
            query = query.Where(o =>
                o.OrderNumber.ToLower().Contains(searchLower) ||
                o.CustomerName.ToLower().Contains(searchLower) ||
                (o.DestinationFacilityName != null && o.DestinationFacilityName.ToLower().Contains(searchLower)));
        }

        var total = await query.CountAsync(cancellationToken);
        var results = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (results, total);
    }

    public async Task<SalesOrder> AddAsync(SalesOrder order, CancellationToken cancellationToken = default)
    {
        await _db.SalesOrders.AddAsync(order, cancellationToken);
        return order;
    }

    public Task<SalesOrder> UpdateAsync(SalesOrder order, CancellationToken cancellationToken = default)
    {
        _db.SalesOrders.Update(order);
        return Task.FromResult(order);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}

