using Harvestry.Sales.Application.Interfaces;
using Harvestry.Sales.Domain.Entities;
using Harvestry.Sales.Domain.Enums;
using Harvestry.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Sales.Infrastructure.Repositories;

public sealed class ShipmentRepository : IShipmentRepository
{
    private readonly SalesDbContext _db;

    public ShipmentRepository(SalesDbContext db)
    {
        _db = db;
    }

    public Task<Shipment?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default)
        => _db.Shipments.FirstOrDefaultAsync(s => s.SiteId == siteId && s.Id == id, cancellationToken);

    public async Task<(List<Shipment> Shipments, int TotalCount)> GetBySiteAsync(
        Guid siteId,
        int page = 1,
        int pageSize = 50,
        string? status = null,
        Guid? salesOrderId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Shipments.Where(s => s.SiteId == siteId);

        if (salesOrderId.HasValue) query = query.Where(s => s.SalesOrderId == salesOrderId.Value);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ShipmentStatus>(status, true, out var st))
        {
            query = query.Where(s => s.Status == st);
        }

        var total = await query.CountAsync(cancellationToken);
        var results = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (results, total);
    }

    public async Task<Shipment> AddAsync(Shipment shipment, CancellationToken cancellationToken = default)
    {
        await _db.Shipments.AddAsync(shipment, cancellationToken);
        return shipment;
    }

    public Task<Shipment> UpdateAsync(Shipment shipment, CancellationToken cancellationToken = default)
    {
        _db.Shipments.Update(shipment);
        return Task.FromResult(shipment);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}

