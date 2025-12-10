using Harvestry.Packages.Domain.Enums;
using Harvestry.Packages.Domain.ValueObjects;

namespace Harvestry.Packages.Domain.Entities;

public sealed partial class Package
{
    /// <summary>
    /// Restores a Package entity from persistence
    /// </summary>
    public static Package Restore(
        Guid id,
        Guid siteId,
        string packageLabelValue,
        Guid itemId,
        string itemName,
        string itemCategory,
        decimal quantity,
        decimal initialQuantity,
        string unitOfMeasure,
        Guid? locationId,
        string? locationName,
        string? sublocationName,
        Guid? sourceHarvestId,
        string? sourceHarvestName,
        string? productionBatchNumber,
        bool isProductionBatch,
        bool isTradeSample,
        bool isDonation,
        bool productRequiresRemediation,
        string? patientLicenseNumber,
        DateOnly packagedDate,
        DateOnly? expirationDate,
        DateOnly? useByDate,
        DateOnly? finishedDate,
        LabTestingState labTestingState,
        bool labTestingStateRequired,
        decimal? thcPercent,
        decimal? thcContent,
        string? thcContentUom,
        decimal? cbdPercent,
        decimal? cbdContent,
        PackageStatus status,
        PackageType packageType,
        string? notes,
        long? metrcPackageId,
        DateTime? metrcLastSyncAt,
        string? metrcSyncStatus,
        IDictionary<string, object>? metadata,
        DateTime createdAt,
        Guid createdByUserId,
        DateTime updatedAt,
        Guid updatedByUserId)
    {
        var packageLabel = PackageLabel.Create(packageLabelValue);

        var package = new Package(id)
        {
            SiteId = siteId,
            PackageLabel = packageLabel,
            ItemId = itemId,
            ItemName = itemName,
            ItemCategory = itemCategory,
            Quantity = quantity,
            InitialQuantity = initialQuantity,
            UnitOfMeasure = unitOfMeasure,
            LocationId = locationId,
            LocationName = locationName,
            SublocationName = sublocationName,
            SourceHarvestId = sourceHarvestId,
            SourceHarvestName = sourceHarvestName,
            ProductionBatchNumber = productionBatchNumber,
            IsProductionBatch = isProductionBatch,
            IsTradeSample = isTradeSample,
            IsDonation = isDonation,
            ProductRequiresRemediation = productRequiresRemediation,
            PatientLicenseNumber = patientLicenseNumber,
            PackagedDate = packagedDate,
            ExpirationDate = expirationDate,
            UseByDate = useByDate,
            FinishedDate = finishedDate,
            LabTestingState = labTestingState,
            LabTestingStateRequired = labTestingStateRequired,
            ThcPercent = thcPercent,
            ThcContent = thcContent,
            ThcContentUnitOfMeasure = thcContentUom,
            CbdPercent = cbdPercent,
            CbdContent = cbdContent,
            Status = status,
            PackageType = packageType,
            Notes = notes,
            MetrcPackageId = metrcPackageId,
            MetrcLastSyncAt = metrcLastSyncAt,
            MetrcSyncStatus = metrcSyncStatus,
            Metadata = metadata != null
                ? new Dictionary<string, object>(metadata)
                : new Dictionary<string, object>(),
            CreatedAt = createdAt,
            CreatedByUserId = createdByUserId,
            UpdatedAt = updatedAt,
            UpdatedByUserId = updatedByUserId
        };

        return package;
    }
}








