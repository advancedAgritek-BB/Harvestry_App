using Harvestry.Sales.Domain.Entities;

namespace Harvestry.Sales.Application.Interfaces;

public interface ISalesAllocationRepository
{
    Task<List<SalesAllocation>> GetBySalesOrderIdAsync(Guid siteId, Guid salesOrderId, CancellationToken cancellationToken = default);
    Task<List<SalesAllocation>> GetByShipmentIdAsync(Guid siteId, Guid shipmentId, CancellationToken cancellationToken = default);

    Task<SalesAllocation> AddAsync(SalesAllocation allocation, CancellationToken cancellationToken = default);
    Task<List<SalesAllocation>> AddRangeAsync(IEnumerable<SalesAllocation> allocations, CancellationToken cancellationToken = default);
    Task<SalesAllocation> UpdateAsync(SalesAllocation allocation, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

