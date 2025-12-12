using Harvestry.Items.Domain.Enums;

namespace Harvestry.Items.Domain.Entities;

public sealed partial class Item
{
    /// <summary>
    /// Restores an Item entity from persistence
    /// </summary>
    public static Item Restore(
        Guid id,
        Guid siteId,
        string name,
        ItemCategory category,
        UnitOfMeasure unitOfMeasure,
        ItemStatus status,
        Guid? strainId,
        string? strainName,
        decimal? unitWeight,
        string? unitWeightUom,
        decimal? defaultThcPercent,
        decimal? defaultThcContent,
        string? defaultThcContentUom,
        decimal? defaultCbdPercent,
        decimal? defaultCbdContent,
        bool requiresLabTesting,
        string? defaultLabTestingState,
        string? description,
        string? sku,
        string? barcode,
        long? metrcItemId,
        DateTime? metrcLastSyncAt,
        string? metrcSyncStatus,
        IDictionary<string, object>? metadata,
        DateTime createdAt,
        Guid createdByUserId,
        DateTime updatedAt,
        Guid updatedByUserId)
    {
        var item = new Item(id)
        {
            SiteId = siteId,
            Name = name,
            Category = category,
            UnitOfMeasure = unitOfMeasure,
            Status = status,
            StrainId = strainId,
            StrainName = strainName,
            UnitWeight = unitWeight,
            UnitWeightUnitOfMeasure = unitWeightUom,
            DefaultThcPercent = defaultThcPercent,
            DefaultThcContent = defaultThcContent,
            DefaultThcContentUnitOfMeasure = defaultThcContentUom,
            DefaultCbdPercent = defaultCbdPercent,
            DefaultCbdContent = defaultCbdContent,
            RequiresLabTesting = requiresLabTesting,
            DefaultLabTestingState = defaultLabTestingState,
            Description = description,
            Sku = sku,
            Barcode = barcode,
            MetrcItemId = metrcItemId,
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

        return item;
    }
}









