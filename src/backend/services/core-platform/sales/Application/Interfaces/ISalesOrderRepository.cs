using Harvestry.Sales.Domain.Entities;

namespace Harvestry.Sales.Application.Interfaces;

public interface ISalesOrderRepository
{
    Task<SalesOrder?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default);
    Task<SalesOrder?> GetByOrderNumberAsync(Guid siteId, string orderNumber, CancellationToken cancellationToken = default);

    Task<(List<SalesOrder> Orders, int TotalCount)> GetBySiteAsync(
        Guid siteId,
        int page = 1,
        int pageSize = 50,
        string? status = null,
        string? search = null,
        CancellationToken cancellationToken = default);

    Task<SalesOrder> AddAsync(SalesOrder order, CancellationToken cancellationToken = default);
    Task<SalesOrder> UpdateAsync(SalesOrder order, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

