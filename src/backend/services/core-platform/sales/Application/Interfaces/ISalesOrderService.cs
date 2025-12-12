using Harvestry.Sales.Application.DTOs;

namespace Harvestry.Sales.Application.Interfaces;

public interface ISalesOrderService
{
    Task<SalesOrderDto?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default);
    Task<SalesOrderListResponse> GetBySiteAsync(Guid siteId, int page = 1, int pageSize = 50, string? status = null, string? search = null, CancellationToken cancellationToken = default);

    Task<SalesOrderDto> CreateDraftAsync(Guid siteId, CreateSalesOrderRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<SalesOrderDto?> AddLineAsync(Guid siteId, Guid salesOrderId, AddSalesOrderLineRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<SalesOrderDto?> SubmitAsync(Guid siteId, Guid salesOrderId, Guid userId, CancellationToken cancellationToken = default);
    Task<SalesOrderDto?> CancelAsync(Guid siteId, Guid salesOrderId, CancelSalesOrderRequest request, Guid userId, CancellationToken cancellationToken = default);
}

