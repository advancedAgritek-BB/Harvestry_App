using Harvestry.Sales.Infrastructure.Persistence;
using Harvestry.Transfers.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Transfers.Infrastructure.Repositories;

public sealed class ShipmentTransferSourceReader : IShipmentTransferSourceReader
{
    private readonly SalesDbContext _salesDb;

    public ShipmentTransferSourceReader(SalesDbContext salesDb)
    {
        _salesDb = salesDb;
    }

    public async Task<ShipmentTransferSource?> GetAsync(Guid siteId, Guid shipmentId, CancellationToken cancellationToken = default)
    {
        var shipment = await _salesDb.Shipments.FirstOrDefaultAsync(s => s.SiteId == siteId && s.Id == shipmentId, cancellationToken);
        if (shipment == null) return null;

        var order = await _salesDb.SalesOrders.FirstOrDefaultAsync(o => o.SiteId == siteId && o.Id == shipment.SalesOrderId, cancellationToken);
        if (order == null) return null;

        var packages = await _salesDb.ShipmentPackages
            .Where(p => p.SiteId == siteId && p.ShipmentId == shipmentId)
            .ToListAsync(cancellationToken);

        return new ShipmentTransferSource
        {
            ShipmentId = shipmentId,
            SalesOrderId = order.Id,
            DestinationLicenseNumber = order.DestinationLicenseNumber ?? string.Empty,
            DestinationFacilityName = order.DestinationFacilityName,
            Packages = packages.Select(p => new ShipmentTransferSourcePackage
            {
                PackageId = p.PackageId,
                PackageLabel = p.PackageLabel,
                Quantity = p.Quantity,
                UnitOfMeasure = p.UnitOfMeasure
            }).ToList()
        };
    }
}

