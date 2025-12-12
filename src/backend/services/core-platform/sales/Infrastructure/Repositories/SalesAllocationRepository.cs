using Harvestry.Sales.Application.Interfaces;
using Harvestry.Sales.Domain.Entities;
using Harvestry.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Sales.Infrastructure.Repositories;

public sealed class SalesAllocationRepository : ISalesAllocationRepository
{
    private readonly SalesDbContext _db;

    public SalesAllocationRepository(SalesDbContext db)
    {
        _db = db;
    }

    public Task<List<SalesAllocation>> GetBySalesOrderIdAsync(Guid siteId, Guid salesOrderId, CancellationToken cancellationToken = default)
        => _db.SalesAllocations
            .Where(a => a.SiteId == siteId && a.SalesOrderId == salesOrderId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<List<SalesAllocation>> GetByShipmentIdAsync(Guid siteId, Guid shipmentId, CancellationToken cancellationToken = default)
    {
        // shipment_packages may reference sales_allocation_id; fetch allocations through those IDs
        var allocationIds = await _db.ShipmentPackages
            .Where(p => p.SiteId == siteId && p.ShipmentId == shipmentId && p.SalesAllocationId != null)
            .Select(p => p.SalesAllocationId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (allocationIds.Count == 0) return new List<SalesAllocation>();

        return await _db.SalesAllocations
            .Where(a => a.SiteId == siteId && allocationIds.Contains(a.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<SalesAllocation> AddAsync(SalesAllocation allocation, CancellationToken cancellationToken = default)
    {
        await _db.SalesAllocations.AddAsync(allocation, cancellationToken);
        return allocation;
    }

    public async Task<List<SalesAllocation>> AddRangeAsync(IEnumerable<SalesAllocation> allocations, CancellationToken cancellationToken = default)
    {
        var list = allocations.ToList();
        await _db.SalesAllocations.AddRangeAsync(list, cancellationToken);
        return list;
    }

    public Task<SalesAllocation> UpdateAsync(SalesAllocation allocation, CancellationToken cancellationToken = default)
    {
        _db.SalesAllocations.Update(allocation);
        return Task.FromResult(allocation);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}

