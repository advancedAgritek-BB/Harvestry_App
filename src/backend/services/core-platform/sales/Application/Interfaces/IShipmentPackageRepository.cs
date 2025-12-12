using Harvestry.Sales.Domain.Entities;

namespace Harvestry.Sales.Application.Interfaces;

public interface IShipmentPackageRepository
{
    Task<List<ShipmentPackage>> GetByShipmentIdAsync(Guid siteId, Guid shipmentId, CancellationToken cancellationToken = default);
    Task<List<ShipmentPackage>> AddRangeAsync(IEnumerable<ShipmentPackage> packages, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

