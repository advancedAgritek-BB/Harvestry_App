using Harvestry.Sales.Application.DTOs;
using Harvestry.Sales.Application.Interfaces;
using Harvestry.Sales.Application.Mappers;
using Harvestry.Sales.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Harvestry.Sales.Application.Services;

public sealed class SalesOrderService : ISalesOrderService
{
    private readonly ISalesOrderRepository _salesOrderRepository;
    private readonly ISalesOrderLineRepository _lineRepository;
    private readonly ILogger<SalesOrderService> _logger;

    public SalesOrderService(
        ISalesOrderRepository salesOrderRepository,
        ISalesOrderLineRepository lineRepository,
        ILogger<SalesOrderService> logger)
    {
        _salesOrderRepository = salesOrderRepository;
        _lineRepository = lineRepository;
        _logger = logger;
    }

    public async Task<SalesOrderDto?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _salesOrderRepository.GetByIdAsync(siteId, id, cancellationToken);
        if (order == null) return null;

        var lines = await _lineRepository.GetByOrderIdAsync(siteId, order.Id, cancellationToken);
        return order.ToDto(lines);
    }

    public async Task<SalesOrderListResponse> GetBySiteAsync(
        Guid siteId,
        int page = 1,
        int pageSize = 50,
        string? status = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var (orders, total) = await _salesOrderRepository.GetBySiteAsync(siteId, page, pageSize, status, search, cancellationToken);
        var dtos = orders.Select(o => o.ToDto(lines: Array.Empty<SalesOrderLine>())).ToList();

        return new SalesOrderListResponse
        {
            Orders = dtos,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<SalesOrderDto> CreateDraftAsync(Guid siteId, CreateSalesOrderRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var order = SalesOrder.CreateDraft(siteId, request.OrderNumber, request.CustomerName, userId);
        order.SetDestination(request.DestinationLicenseNumber, request.DestinationFacilityName, userId);
        order.SetRequestedShipDate(request.RequestedShipDate, userId);
        order.SetNotes(request.Notes, userId);

        await _salesOrderRepository.AddAsync(order, cancellationToken);
        await _salesOrderRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created sales order {OrderNumber} ({OrderId})", order.OrderNumber, order.Id);
        return order.ToDto(lines: new List<SalesOrderLine>());
    }

    public async Task<SalesOrderDto?> AddLineAsync(Guid siteId, Guid salesOrderId, AddSalesOrderLineRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var order = await _salesOrderRepository.GetByIdAsync(siteId, salesOrderId, cancellationToken);
        if (order == null) return null;

        var line = order.AddLine(
            request.LineNumber,
            request.ItemId,
            request.ItemName,
            request.RequestedQuantity,
            request.UnitOfMeasure,
            request.UnitPrice,
            request.CurrencyCode,
            userId);

        await _lineRepository.AddAsync(line, cancellationToken);
        await _salesOrderRepository.UpdateAsync(order, cancellationToken);
        await _salesOrderRepository.SaveChangesAsync(cancellationToken);

        var lines = await _lineRepository.GetByOrderIdAsync(siteId, salesOrderId, cancellationToken);
        return order.ToDto(lines);
    }

    public async Task<SalesOrderDto?> SubmitAsync(Guid siteId, Guid salesOrderId, Guid userId, CancellationToken cancellationToken = default)
    {
        var order = await _salesOrderRepository.GetByIdAsync(siteId, salesOrderId, cancellationToken);
        if (order == null) return null;

        // Validate lines exist
        var lines = await _lineRepository.GetByOrderIdAsync(siteId, salesOrderId, cancellationToken);
        if (lines.Count == 0) throw new InvalidOperationException("Cannot submit an order without lines.");

        order.Submit(userId);
        await _salesOrderRepository.UpdateAsync(order, cancellationToken);
        await _salesOrderRepository.SaveChangesAsync(cancellationToken);

        return order.ToDto(lines);
    }

    public async Task<SalesOrderDto?> CancelAsync(Guid siteId, Guid salesOrderId, CancelSalesOrderRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var order = await _salesOrderRepository.GetByIdAsync(siteId, salesOrderId, cancellationToken);
        if (order == null) return null;

        order.Cancel(request.Reason, userId);
        await _salesOrderRepository.UpdateAsync(order, cancellationToken);
        await _salesOrderRepository.SaveChangesAsync(cancellationToken);

        var lines = await _lineRepository.GetByOrderIdAsync(siteId, salesOrderId, cancellationToken);
        return order.ToDto(lines);
    }
}

