using Harvestry.Sales.Application.DTOs;

namespace Harvestry.Sales.Application.Interfaces;

public interface IShipmentService
{
    Task<ShipmentDto?> GetByIdAsync(Guid siteId, Guid shipmentId, CancellationToken cancellationToken = default);
    Task<ShipmentListResponse> GetBySiteAsync(Guid siteId, int page = 1, int pageSize = 50, string? status = null, Guid? salesOrderId = null, CancellationToken cancellationToken = default);

    Task<ShipmentDto> CreateFromAllocationsAsync(Guid siteId, Guid salesOrderId, CreateShipmentRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<ShipmentDto?> StartPickingAsync(Guid siteId, Guid shipmentId, Guid userId, CancellationToken cancellationToken = default);
    Task<ShipmentDto?> MarkPackedAsync(Guid siteId, Guid shipmentId, Guid userId, CancellationToken cancellationToken = default);
    Task<ShipmentDto?> MarkShippedAsync(Guid siteId, Guid shipmentId, MarkShipmentShippedRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<ShipmentDto?> CancelAsync(Guid siteId, Guid shipmentId, CancelShipmentRequest request, Guid userId, CancellationToken cancellationToken = default);
}

