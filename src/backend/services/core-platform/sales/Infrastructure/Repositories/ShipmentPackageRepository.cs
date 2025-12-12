using Harvestry.Sales.Application.Interfaces;
using Harvestry.Sales.Domain.Entities;
using Harvestry.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Sales.Infrastructure.Repositories;

public sealed class ShipmentPackageRepository : IShipmentPackageRepository
{
    private readonly SalesDbContext _db;

    public ShipmentPackageRepository(SalesDbContext db)
    {
        _db = db;
    }

    public Task<List<ShipmentPackage>> GetByShipmentIdAsync(Guid siteId, Guid shipmentId, CancellationToken cancellationToken = default)
        => _db.ShipmentPackages
            .Where(p => p.SiteId == siteId && p.ShipmentId == shipmentId)
            .ToListAsync(cancellationToken);

    public async Task<List<ShipmentPackage>> AddRangeAsync(IEnumerable<ShipmentPackage> packages, CancellationToken cancellationToken = default)
    {
        var list = packages.ToList();
        await _db.ShipmentPackages.AddRangeAsync(list, cancellationToken);
        return list;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}

