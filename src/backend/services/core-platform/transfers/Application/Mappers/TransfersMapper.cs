using Harvestry.Transfers.Application.DTOs;
using Harvestry.Transfers.Domain.Entities;

namespace Harvestry.Transfers.Application.Mappers;

public static class TransfersMapper
{
    public static OutboundTransferDto ToDto(this OutboundTransfer transfer, IReadOnlyList<OutboundTransferPackage>? packages = null)
    {
        return new OutboundTransferDto
        {
            Id = transfer.Id,
            SiteId = transfer.SiteId,
            ShipmentId = transfer.ShipmentId,
            SalesOrderId = transfer.SalesOrderId,
            DestinationLicenseNumber = transfer.DestinationLicenseNumber,
            DestinationFacilityName = transfer.DestinationFacilityName,
            Status = transfer.Status.ToString(),
            StatusReason = transfer.StatusReason,
            PlannedDepartureAt = transfer.PlannedDepartureAt,
            PlannedArrivalAt = transfer.PlannedArrivalAt,
            MetrcTransferTemplateId = transfer.MetrcTransferTemplateId,
            MetrcTransferNumber = transfer.MetrcTransferNumber,
            MetrcSyncStatus = transfer.MetrcSyncStatus,
            MetrcSyncError = transfer.MetrcSyncError,
            Packages = (packages ?? transfer.Packages).Select(ToDto).ToList()
        };
    }

    public static OutboundTransferPackageDto ToDto(this OutboundTransferPackage p)
    {
        return new OutboundTransferPackageDto
        {
            Id = p.Id,
            PackageId = p.PackageId,
            PackageLabel = p.PackageLabel,
            Quantity = p.Quantity,
            UnitOfMeasure = p.UnitOfMeasure
        };
    }

    public static TransportManifestDto ToDto(this TransportManifest manifest)
    {
        return new TransportManifestDto
        {
            Id = manifest.Id,
            SiteId = manifest.SiteId,
            OutboundTransferId = manifest.OutboundTransferId,
            Status = manifest.Status.ToString(),
            TransporterName = manifest.TransporterName,
            TransporterLicenseNumber = manifest.TransporterLicenseNumber,
            DriverName = manifest.DriverName,
            DriverLicenseNumber = manifest.DriverLicenseNumber,
            DriverPhone = manifest.DriverPhone,
            VehicleMake = manifest.VehicleMake,
            VehicleModel = manifest.VehicleModel,
            VehiclePlate = manifest.VehiclePlate,
            DepartureAt = manifest.DepartureAt,
            ArrivalAt = manifest.ArrivalAt,
            MetrcManifestNumber = manifest.MetrcManifestNumber
        };
    }

    public static InboundReceiptDto ToDto(this InboundTransferReceipt receipt, IReadOnlyList<InboundTransferReceiptLine>? lines = null)
    {
        return new InboundReceiptDto
        {
            Id = receipt.Id,
            SiteId = receipt.SiteId,
            OutboundTransferId = receipt.OutboundTransferId,
            MetrcTransferId = receipt.MetrcTransferId,
            MetrcTransferNumber = receipt.MetrcTransferNumber,
            Status = receipt.Status.ToString(),
            ReceivedAt = receipt.ReceivedAt,
            Notes = receipt.Notes,
            Lines = (lines ?? receipt.Lines).Select(ToDto).ToList()
        };
    }

    public static InboundReceiptLineDto ToDto(this InboundTransferReceiptLine line)
    {
        return new InboundReceiptLineDto
        {
            Id = line.Id,
            PackageLabel = line.PackageLabel,
            ReceivedQuantity = line.ReceivedQuantity,
            UnitOfMeasure = line.UnitOfMeasure,
            Accepted = line.Accepted,
            RejectionReason = line.RejectionReason
        };
    }
}

