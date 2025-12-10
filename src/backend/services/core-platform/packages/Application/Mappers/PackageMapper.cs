using Harvestry.Packages.Application.DTOs;
using Harvestry.Packages.Domain.Entities;

namespace Harvestry.Packages.Application.Mappers;

/// <summary>
/// Extension methods for mapping Package entities to DTOs
/// </summary>
public static class PackageMapper
{
    public static PackageDto ToDto(this Package package)
    {
        return new PackageDto
        {
            Id = package.Id,
            SiteId = package.SiteId,
            PackageLabel = package.PackageLabel.Value,
            ItemId = package.ItemId,
            ItemName = package.ItemName,
            ItemCategory = package.ItemCategory,
            Quantity = package.Quantity,
            InitialQuantity = package.InitialQuantity,
            UnitOfMeasure = package.UnitOfMeasure,
            LocationId = package.LocationId,
            LocationName = package.LocationName,
            SublocationName = package.SublocationName,
            SourceHarvestId = package.SourceHarvestId,
            SourceHarvestName = package.SourceHarvestName,
            SourcePackageLabels = package.SourcePackageLabels.ToList(),
            ProductionBatchNumber = package.ProductionBatchNumber,
            IsProductionBatch = package.IsProductionBatch,
            IsTradeSample = package.IsTradeSample,
            IsDonation = package.IsDonation,
            ProductRequiresRemediation = package.ProductRequiresRemediation,
            PackagedDate = package.PackagedDate,
            ExpirationDate = package.ExpirationDate,
            UseByDate = package.UseByDate,
            FinishedDate = package.FinishedDate,
            LabTestingState = package.LabTestingState.ToString(),
            LabTestingStateRequired = package.LabTestingStateRequired,
            ThcPercent = package.ThcPercent,
            CbdPercent = package.CbdPercent,
            Status = package.Status.ToString(),
            PackageType = package.PackageType.ToString(),
            Notes = package.Notes,
            MetrcPackageId = package.MetrcPackageId,
            MetrcLastSyncAt = package.MetrcLastSyncAt,
            MetrcSyncStatus = package.MetrcSyncStatus,
            UnitCost = package.UnitCost,
            MaterialCost = package.MaterialCost,
            LaborCost = package.LaborCost,
            OverheadCost = package.OverheadCost,
            TotalCost = package.TotalCost,
            TotalValue = package.TotalValue,
            ReservedQuantity = package.ReservedQuantity,
            AvailableQuantity = package.AvailableQuantity,
            InventoryCategory = package.InventoryCategory.ToString(),
            HoldReasonCode = package.HoldReasonCode?.ToString(),
            HoldPlacedAt = package.HoldPlacedAt,
            HoldPlacedByUserId = package.HoldPlacedByUserId,
            HoldReleasedAt = package.HoldReleasedAt,
            RequiresTwoPersonRelease = package.RequiresTwoPersonRelease,
            VendorId = package.VendorId,
            VendorName = package.VendorName,
            VendorLotNumber = package.VendorLotNumber,
            PurchaseOrderId = package.PurchaseOrderId,
            PurchaseOrderNumber = package.PurchaseOrderNumber,
            ReceivedDate = package.ReceivedDate,
            Grade = package.Grade?.ToString(),
            QualityScore = package.QualityScore,
            QualityNotes = package.QualityNotes,
            GenerationDepth = package.GenerationDepth,
            RootAncestorId = package.RootAncestorId,
            CreatedAt = package.CreatedAt,
            CreatedByUserId = package.CreatedByUserId,
            UpdatedAt = package.UpdatedAt,
            UpdatedByUserId = package.UpdatedByUserId
        };
    }

    public static PackageSummaryDto ToSummaryDto(this Package package)
    {
        return new PackageSummaryDto
        {
            Id = package.Id,
            PackageLabel = package.PackageLabel.Value,
            ItemName = package.ItemName,
            ItemCategory = package.ItemCategory,
            Quantity = package.Quantity,
            AvailableQuantity = package.AvailableQuantity,
            UnitOfMeasure = package.UnitOfMeasure,
            LocationName = package.LocationName,
            Status = package.Status.ToString(),
            LabTestingState = package.LabTestingState.ToString(),
            PackagedDate = package.PackagedDate,
            ExpirationDate = package.ExpirationDate,
            UnitCost = package.UnitCost,
            TotalValue = package.TotalValue,
            InventoryCategory = package.InventoryCategory.ToString(),
            Grade = package.Grade?.ToString(),
            HoldReasonCode = package.HoldReasonCode?.ToString(),
            MetrcPackageId = package.MetrcPackageId,
            MetrcSyncStatus = package.MetrcSyncStatus
        };
    }

    public static MovementDto ToDto(this InventoryMovement movement)
    {
        return new MovementDto
        {
            Id = movement.Id,
            SiteId = movement.SiteId,
            MovementType = movement.MovementType.ToString(),
            Status = movement.Status.ToString(),
            PackageId = movement.PackageId,
            PackageLabel = movement.PackageLabel,
            ItemId = movement.ItemId,
            ItemName = movement.ItemName,
            FromLocationId = movement.FromLocationId,
            FromLocationPath = movement.FromLocationPath,
            ToLocationId = movement.ToLocationId,
            ToLocationPath = movement.ToLocationPath,
            Quantity = movement.Quantity,
            UnitOfMeasure = movement.UnitOfMeasure,
            QuantityBefore = movement.QuantityBefore,
            QuantityAfter = movement.QuantityAfter,
            UnitCost = movement.UnitCost,
            TotalCost = movement.TotalCost,
            ReasonCode = movement.ReasonCode,
            ReasonNotes = movement.ReasonNotes,
            ProcessingJobId = movement.ProcessingJobId,
            ProcessingJobNumber = movement.ProcessingJobNumber,
            SalesOrderId = movement.SalesOrderId,
            SalesOrderNumber = movement.SalesOrderNumber,
            MetrcManifestId = movement.MetrcManifestId,
            SyncStatus = movement.SyncStatus,
            VerifiedByUserId = movement.VerifiedByUserId,
            VerifiedAt = movement.VerifiedAt,
            BarcodeScanned = movement.BarcodeScanned,
            Notes = movement.Notes,
            PhotoUrls = movement.PhotoUrls.ToList(),
            RequiresApproval = movement.RequiresApproval,
            FirstApproverId = movement.FirstApproverId,
            FirstApprovedAt = movement.FirstApprovedAt,
            SecondApproverId = movement.SecondApproverId,
            SecondApprovedAt = movement.SecondApprovedAt,
            BatchMovementId = movement.BatchMovementId,
            BatchSequence = movement.BatchSequence,
            CreatedAt = movement.CreatedAt,
            CreatedByUserId = movement.CreatedByUserId,
            CompletedAt = movement.CompletedAt,
            CompletedByUserId = movement.CompletedByUserId
        };
    }

    public static MovementSummaryDto ToSummaryDto(this InventoryMovement movement)
    {
        return new MovementSummaryDto
        {
            Id = movement.Id,
            MovementType = movement.MovementType.ToString(),
            Status = movement.Status.ToString(),
            PackageLabel = movement.PackageLabel,
            ItemName = movement.ItemName,
            FromLocationPath = movement.FromLocationPath,
            ToLocationPath = movement.ToLocationPath,
            Quantity = movement.Quantity,
            UnitOfMeasure = movement.UnitOfMeasure,
            ReasonCode = movement.ReasonCode,
            CreatedAt = movement.CreatedAt,
            CreatedByUserId = movement.CreatedByUserId,
            SyncStatus = movement.SyncStatus
        };
    }
}



