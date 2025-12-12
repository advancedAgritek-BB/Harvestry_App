using Harvestry.Sales.Application.DTOs;
using Harvestry.Sales.Domain.Entities;

namespace Harvestry.Sales.Application.Mappers;

public static class SalesMapper
{
    public static SalesOrderDto ToDto(this SalesOrder order, IReadOnlyList<SalesOrderLine>? lines = null)
    {
        return new SalesOrderDto
        {
            Id = order.Id,
            SiteId = order.SiteId,
            OrderNumber = order.OrderNumber,
            CustomerName = order.CustomerName,
            DestinationLicenseNumber = order.DestinationLicenseNumber,
            DestinationFacilityName = order.DestinationFacilityName,
            Status = order.Status.ToString(),
            RequestedShipDate = order.RequestedShipDate,
            SubmittedAt = order.SubmittedAt,
            CancelledAt = order.CancelledAt,
            Notes = order.Notes,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Lines = (lines ?? order.Lines).Select(ToDto).ToList()
        };
    }

    public static SalesOrderLineDto ToDto(this SalesOrderLine line)
    {
        return new SalesOrderLineDto
        {
            Id = line.Id,
            LineNumber = line.LineNumber,
            ItemId = line.ItemId,
            ItemName = line.ItemName,
            UnitOfMeasure = line.UnitOfMeasure,
            RequestedQuantity = line.RequestedQuantity,
            AllocatedQuantity = line.AllocatedQuantity,
            ShippedQuantity = line.ShippedQuantity,
            UnitPrice = line.UnitPrice,
            CurrencyCode = line.CurrencyCode
        };
    }

    public static SalesAllocationDto ToDto(this SalesAllocation allocation)
    {
        return new SalesAllocationDto
        {
            Id = allocation.Id,
            SalesOrderId = allocation.SalesOrderId,
            SalesOrderLineId = allocation.SalesOrderLineId,
            PackageId = allocation.PackageId,
            PackageLabel = allocation.PackageLabel,
            AllocatedQuantity = allocation.AllocatedQuantity,
            UnitOfMeasure = allocation.UnitOfMeasure,
            IsCancelled = allocation.IsCancelled,
            CreatedAt = allocation.CreatedAt
        };
    }

    public static ShipmentDto ToDto(this Shipment shipment, IReadOnlyList<ShipmentPackage>? packages = null)
    {
        return new ShipmentDto
        {
            Id = shipment.Id,
            SiteId = shipment.SiteId,
            ShipmentNumber = shipment.ShipmentNumber,
            SalesOrderId = shipment.SalesOrderId,
            Status = shipment.Status.ToString(),
            PickingStartedAt = shipment.PickingStartedAt,
            PackedAt = shipment.PackedAt,
            ShippedAt = shipment.ShippedAt,
            CancelledAt = shipment.CancelledAt,
            CarrierName = shipment.CarrierName,
            TrackingNumber = shipment.TrackingNumber,
            Packages = (packages ?? shipment.Packages).Select(ToDto).ToList()
        };
    }

    public static ShipmentPackageDto ToDto(this ShipmentPackage p)
    {
        return new ShipmentPackageDto
        {
            Id = p.Id,
            PackageId = p.PackageId,
            PackageLabel = p.PackageLabel,
            Quantity = p.Quantity,
            UnitOfMeasure = p.UnitOfMeasure,
            PackedAt = p.PackedAt
        };
    }
}

