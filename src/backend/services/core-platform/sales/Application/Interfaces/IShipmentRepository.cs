using Harvestry.Sales.Domain.Entities;

namespace Harvestry.Sales.Application.Interfaces;

public interface IShipmentRepository
{
    Task<Shipment?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default);
    Task<(List<Shipment> Shipments, int TotalCount)> GetBySiteAsync(
        Guid siteId,
        int page = 1,
        int pageSize = 50,
        string? status = null,
        Guid? salesOrderId = null,
        CancellationToken cancellationToken = default);

    Task<Shipment> AddAsync(Shipment shipment, CancellationToken cancellationToken = default);
    Task<Shipment> UpdateAsync(Shipment shipment, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

