using Harvestry.Sales.Application.DTOs;

namespace Harvestry.Sales.Application.Interfaces;

public interface IAllocationService
{
    Task<List<SalesAllocationDto>> GetAllocationsAsync(Guid siteId, Guid salesOrderId, CancellationToken cancellationToken = default);
    Task<List<SalesAllocationDto>> AllocateAsync(Guid siteId, Guid salesOrderId, AllocateSalesOrderRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<List<SalesAllocationDto>> UnallocateAsync(Guid siteId, Guid salesOrderId, UnallocateSalesOrderRequest request, Guid userId, CancellationToken cancellationToken = default);
}

