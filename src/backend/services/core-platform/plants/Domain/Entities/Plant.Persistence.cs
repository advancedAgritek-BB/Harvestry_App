using Harvestry.Plants.Domain.Enums;
using Harvestry.Plants.Domain.ValueObjects;

namespace Harvestry.Plants.Domain.Entities;

public sealed partial class Plant
{
    /// <summary>
    /// Restores a Plant entity from persistence
    /// </summary>
    public static Plant Restore(
        Guid id,
        Guid siteId,
        string plantTagValue,
        Guid batchId,
        Guid strainId,
        string strainName,
        PlantGrowthPhase growthPhase,
        PlantStatus status,
        DateOnly plantedDate,
        DateOnly? vegetativeDate,
        DateOnly? floweringDate,
        Guid? locationId,
        string? sublocationName,
        string? patientLicenseNumber,
        Guid? harvestId,
        DateOnly? harvestDate,
        decimal? harvestWetWeight,
        string? harvestWeightUnit,
        DateOnly? destroyedDate,
        PlantDestroyReason? destroyReason,
        string? destroyReasonNote,
        decimal? wasteWeight,
        string? wasteWeightUnit,
        WasteMethod? wasteMethod,
        Guid? destroyedByUserId,
        Guid? destroyWitnessUserId,
        long? metrcPlantId,
        DateTime? metrcLastSyncAt,
        string? metrcSyncStatus,
        string? notes,
        IDictionary<string, object>? metadata,
        DateTime createdAt,
        Guid createdByUserId,
        DateTime updatedAt,
        Guid updatedByUserId)
    {
        var plantTag = PlantTag.Create(plantTagValue);

        var plant = new Plant(id)
        {
            SiteId = siteId,
            PlantTag = plantTag,
            BatchId = batchId,
            StrainId = strainId,
            StrainName = strainName,
            GrowthPhase = growthPhase,
            Status = status,
            PlantedDate = plantedDate,
            VegetativeDate = vegetativeDate,
            FloweringDate = floweringDate,
            LocationId = locationId,
            SublocationName = sublocationName,
            PatientLicenseNumber = patientLicenseNumber,
            HarvestId = harvestId,
            HarvestDate = harvestDate,
            HarvestWetWeight = harvestWetWeight,
            HarvestWeightUnit = harvestWeightUnit,
            DestroyedDate = destroyedDate,
            DestroyReason = destroyReason,
            DestroyReasonNote = destroyReasonNote,
            WasteWeight = wasteWeight,
            WasteWeightUnit = wasteWeightUnit,
            WasteMethod = wasteMethod,
            DestroyedByUserId = destroyedByUserId,
            DestroyWitnessUserId = destroyWitnessUserId,
            MetrcPlantId = metrcPlantId,
            MetrcLastSyncAt = metrcLastSyncAt,
            MetrcSyncStatus = metrcSyncStatus,
            Notes = notes,
            Metadata = metadata != null
                ? new Dictionary<string, object>(metadata)
                : new Dictionary<string, object>(),
            CreatedAt = createdAt,
            CreatedByUserId = createdByUserId,
            UpdatedAt = updatedAt,
            UpdatedByUserId = updatedByUserId
        };

        return plant;
    }
}









