using Harvestry.Sales.Domain.Entities;

namespace Harvestry.Sales.Application.Interfaces;

public interface ISalesOrderLineRepository
{
    Task<List<SalesOrderLine>> GetByOrderIdAsync(Guid siteId, Guid salesOrderId, CancellationToken cancellationToken = default);
    Task<SalesOrderLine?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default);
    Task<SalesOrderLine> AddAsync(SalesOrderLine line, CancellationToken cancellationToken = default);
    Task<SalesOrderLine> UpdateAsync(SalesOrderLine line, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

