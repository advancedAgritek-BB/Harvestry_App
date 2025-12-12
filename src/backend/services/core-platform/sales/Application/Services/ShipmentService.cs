using System.Transactions;
using Harvestry.Packages.Application.Interfaces;
using Harvestry.Packages.Domain.Entities;
using Harvestry.Sales.Application.DTOs;
using Harvestry.Sales.Application.Interfaces;
using Harvestry.Sales.Application.Mappers;
using Harvestry.Sales.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Harvestry.Sales.Application.Services;

public sealed class ShipmentService : IShipmentService
{
    private readonly ISalesOrderRepository _salesOrderRepository;
    private readonly ISalesOrderLineRepository _lineRepository;
    private readonly ISalesAllocationRepository _allocationRepository;
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IShipmentPackageRepository _shipmentPackageRepository;
    private readonly IPackageRepository _packageRepository;
    private readonly IMovementRepository _movementRepository;
    private readonly ILogger<ShipmentService> _logger;

    public ShipmentService(
        ISalesOrderRepository salesOrderRepository,
        ISalesOrderLineRepository lineRepository,
        ISalesAllocationRepository allocationRepository,
        IShipmentRepository shipmentRepository,
        IShipmentPackageRepository shipmentPackageRepository,
        IPackageRepository packageRepository,
        IMovementRepository movementRepository,
        ILogger<ShipmentService> logger)
    {
        _salesOrderRepository = salesOrderRepository;
        _lineRepository = lineRepository;
        _allocationRepository = allocationRepository;
        _shipmentRepository = shipmentRepository;
        _shipmentPackageRepository = shipmentPackageRepository;
        _packageRepository = packageRepository;
        _movementRepository = movementRepository;
        _logger = logger;
    }

    public async Task<ShipmentDto?> GetByIdAsync(Guid siteId, Guid shipmentId, CancellationToken cancellationToken = default)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(siteId, shipmentId, cancellationToken);
        if (shipment == null) return null;
        var packages = await _shipmentPackageRepository.GetByShipmentIdAsync(siteId, shipmentId, cancellationToken);
        return shipment.ToDto(packages);
    }

    public async Task<ShipmentListResponse> GetBySiteAsync(Guid siteId, int page = 1, int pageSize = 50, string? status = null, Guid? salesOrderId = null, CancellationToken cancellationToken = default)
    {
        var (shipments, total) = await _shipmentRepository.GetBySiteAsync(siteId, page, pageSize, status, salesOrderId, cancellationToken);
        return new ShipmentListResponse
        {
            Shipments = shipments.Select(s => s.ToDto(packages: Array.Empty<ShipmentPackage>())).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ShipmentDto> CreateFromAllocationsAsync(Guid siteId, Guid salesOrderId, CreateShipmentRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var order = await _salesOrderRepository.GetByIdAsync(siteId, salesOrderId, cancellationToken);
        if (order == null) throw new InvalidOperationException("Sales order not found.");

        var allocations = await _allocationRepository.GetBySalesOrderIdAsync(siteId, salesOrderId, cancellationToken);
        var activeAllocations = allocations.Where(a => !a.IsCancelled).ToList();
        if (activeAllocations.Count == 0) throw new InvalidOperationException("No active allocations found for order.");

        var shipment = Shipment.CreateDraft(siteId, request.ShipmentNumber, salesOrderId, userId);
        shipment.SetNotes(request.Notes, userId);

        var shipmentPackages = activeAllocations.Select(a =>
            ShipmentPackage.Create(siteId, shipment.Id, a.PackageId, a.PackageLabel, a.AllocatedQuantity, a.UnitOfMeasure, a.Id)
        ).ToList();

        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        await _shipmentRepository.AddAsync(shipment, cancellationToken);
        await _shipmentPackageRepository.AddRangeAsync(shipmentPackages, cancellationToken);

        await _shipmentRepository.SaveChangesAsync(cancellationToken);
        await _shipmentPackageRepository.SaveChangesAsync(cancellationToken);

        tx.Complete();

        _logger.LogInformation("Created shipment {ShipmentNumber} ({ShipmentId}) for sales order {SalesOrderId}", shipment.ShipmentNumber, shipment.Id, salesOrderId);
        return shipment.ToDto(shipmentPackages);
    }

    public async Task<ShipmentDto?> StartPickingAsync(Guid siteId, Guid shipmentId, Guid userId, CancellationToken cancellationToken = default)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(siteId, shipmentId, cancellationToken);
        if (shipment == null) return null;

        shipment.StartPicking(userId);
        await _shipmentRepository.UpdateAsync(shipment, cancellationToken);
        await _shipmentRepository.SaveChangesAsync(cancellationToken);

        var packages = await _shipmentPackageRepository.GetByShipmentIdAsync(siteId, shipmentId, cancellationToken);
        return shipment.ToDto(packages);
    }

    public async Task<ShipmentDto?> MarkPackedAsync(Guid siteId, Guid shipmentId, Guid userId, CancellationToken cancellationToken = default)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(siteId, shipmentId, cancellationToken);
        if (shipment == null) return null;

        var packages = await _shipmentPackageRepository.GetByShipmentIdAsync(siteId, shipmentId, cancellationToken);
        foreach (var p in packages)
        {
            p.MarkPacked(userId);
        }

        shipment.MarkPacked(userId);
        await _shipmentRepository.UpdateAsync(shipment, cancellationToken);
        await _shipmentRepository.SaveChangesAsync(cancellationToken);
        await _shipmentPackageRepository.SaveChangesAsync(cancellationToken);

        return shipment.ToDto(packages);
    }

    public async Task<ShipmentDto?> MarkShippedAsync(Guid siteId, Guid shipmentId, MarkShipmentShippedRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(siteId, shipmentId, cancellationToken);
        if (shipment == null) return null;

        var order = await _salesOrderRepository.GetByIdAsync(siteId, shipment.SalesOrderId, cancellationToken);
        if (order == null) throw new InvalidOperationException("Sales order not found for shipment.");

        var orderLines = await _lineRepository.GetByOrderIdAsync(siteId, order.Id, cancellationToken);
        var allocations = await _allocationRepository.GetByShipmentIdAsync(siteId, shipmentId, cancellationToken);
        var allocationsById = allocations.ToDictionary(a => a.Id, a => a);

        var shipmentPackages = await _shipmentPackageRepository.GetByShipmentIdAsync(siteId, shipmentId, cancellationToken);
        var packageIds = shipmentPackages.Select(p => p.PackageId).Distinct().ToList();
        var packages = await _packageRepository.GetByIdsAsync(siteId, packageIds, cancellationToken);
        var packagesById = packages.ToDictionary(p => p.Id, p => p);

        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        foreach (var sp in shipmentPackages)
        {
            if (!packagesById.TryGetValue(sp.PackageId, out var package))
            {
                throw new InvalidOperationException($"Package {sp.PackageId} not found for shipment.");
            }

            // Consume reserved and record ship movement.
            package.ConsumeReserved(sp.Quantity, userId);
            await _packageRepository.UpdateAsync(package, cancellationToken);

            var fromLocationId = package.LocationId ?? Guid.Empty;
            var fromLocationPath = package.LocationName ?? string.Empty;

            var movement = InventoryMovement.CreateShip(
                siteId,
                package.Id,
                package.PackageLabel.Value,
                sp.Quantity,
                package.UnitOfMeasure,
                fromLocationId,
                fromLocationPath,
                userId,
                order.Id,
                order.OrderNumber,
                package.UnitCost);

            movement.SetItemInfo(package.ItemId, package.ItemName);
            if (request.OutboundTransferId.HasValue)
            {
                movement.LinkTransfer(request.OutboundTransferId.Value);
            }

            if (sp.SalesAllocationId.HasValue && allocationsById.TryGetValue(sp.SalesAllocationId.Value, out var allocation))
            {
                // tie shipment quantity back to line
                var line = orderLines.FirstOrDefault(l => l.Id == allocation.SalesOrderLineId);
                line?.AddShipment(sp.Quantity);
            }

            await _movementRepository.AddAsync(movement, cancellationToken);
        }

        shipment.MarkShipped(request.CarrierName, request.TrackingNumber, userId);
        await _shipmentRepository.UpdateAsync(shipment, cancellationToken);

        // Update sales order status (partial vs full) based on lines
        var isFullyShipped = orderLines.All(l => l.ShippedQuantity >= l.RequestedQuantity);
        order.MarkShipped(!isFullyShipped, userId);
        await _salesOrderRepository.UpdateAsync(order, cancellationToken);

        foreach (var line in orderLines)
        {
            await _lineRepository.UpdateAsync(line, cancellationToken);
        }

        await _movementRepository.SaveChangesAsync(cancellationToken);
        await _packageRepository.SaveChangesAsync(cancellationToken);
        await _shipmentRepository.SaveChangesAsync(cancellationToken);
        await _salesOrderRepository.SaveChangesAsync(cancellationToken);
        await _lineRepository.SaveChangesAsync(cancellationToken);

        tx.Complete();

        return shipment.ToDto(shipmentPackages);
    }

    public async Task<ShipmentDto?> CancelAsync(Guid siteId, Guid shipmentId, CancelShipmentRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(siteId, shipmentId, cancellationToken);
        if (shipment == null) return null;

        shipment.Cancel(request.Reason, userId);
        await _shipmentRepository.UpdateAsync(shipment, cancellationToken);
        await _shipmentRepository.SaveChangesAsync(cancellationToken);

        var packages = await _shipmentPackageRepository.GetByShipmentIdAsync(siteId, shipmentId, cancellationToken);
        return shipment.ToDto(packages);
    }
}

