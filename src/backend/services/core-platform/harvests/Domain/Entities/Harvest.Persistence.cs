using Harvestry.Harvests.Domain.Enums;

namespace Harvestry.Harvests.Domain.Entities;

public sealed partial class Harvest
{
    /// <summary>
    /// Restores a Harvest entity from persistence
    /// </summary>
    public static Harvest Restore(
        Guid id,
        Guid siteId,
        string harvestName,
        HarvestType harvestType,
        Guid strainId,
        string strainName,
        Guid? locationId,
        string? locationName,
        string? sublocationName,
        DateOnly harvestStartDate,
        DateOnly? harvestEndDate,
        DateOnly? dryingDate,
        decimal totalWetWeight,
        decimal totalDryWeight,
        decimal currentWeight,
        decimal totalWasteWeight,
        string unitOfWeight,
        HarvestStatus status,
        string? notes,
        long? metrcHarvestId,
        DateTime? metrcLastSyncAt,
        string? metrcSyncStatus,
        IDictionary<string, object>? metadata,
        DateTime createdAt,
        Guid createdByUserId,
        DateTime updatedAt,
        Guid updatedByUserId)
    {
        var harvest = new Harvest(id)
        {
            SiteId = siteId,
            HarvestName = harvestName,
            HarvestType = harvestType,
            StrainId = strainId,
            StrainName = strainName,
            LocationId = locationId,
            LocationName = locationName,
            SublocationName = sublocationName,
            HarvestStartDate = harvestStartDate,
            HarvestEndDate = harvestEndDate,
            DryingDate = dryingDate,
            TotalWetWeight = totalWetWeight,
            TotalDryWeight = totalDryWeight,
            CurrentWeight = currentWeight,
            TotalWasteWeight = totalWasteWeight,
            UnitOfWeight = unitOfWeight,
            Status = status,
            Notes = notes,
            MetrcHarvestId = metrcHarvestId,
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

        return harvest;
    }
}








