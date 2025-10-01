using System;
using Harvestry.Spatial.Domain.Enums;

namespace Harvestry.Spatial.Domain.Entities;

public partial class InventoryLocation
{
    public static InventoryLocation FromPersistence(
        Guid id,
        Guid siteId,
        Guid? roomId,
        Guid? parentId,
        LocationType locationType,
        string code,
        string name,
        string? barcode,
        string path,
        int depth,
        LocationStatus status,
        decimal? lengthFt,
        decimal? widthFt,
        decimal? heightFt,
        int? plantCapacity,
        int currentPlantCount,
        int? rowNumber,
        int? columnNumber,
        decimal? weightCapacityLbs,
        decimal currentWeightLbs,
        string? notes,
        string? metadataJson,
        DateTime createdAt,
        Guid createdByUserId,
        DateTime updatedAt,
        Guid updatedByUserId)
    {
        var location = new InventoryLocation(id)
        {
            SiteId = siteId,
            RoomId = roomId,
            ParentId = parentId,
            LocationType = locationType,
            Code = code,
            Name = name,
            Barcode = barcode,
            Path = path,
            Depth = depth,
            Status = status,
            LengthFt = lengthFt,
            WidthFt = widthFt,
            HeightFt = heightFt,
            PlantCapacity = plantCapacity,
            CurrentPlantCount = currentPlantCount,
            RowNumber = rowNumber,
            ColumnNumber = columnNumber,
            WeightCapacityLbs = weightCapacityLbs,
            CurrentWeightLbs = currentWeightLbs,
            Notes = notes,
            MetadataJson = metadataJson,
            CreatedAt = createdAt,
            CreatedByUserId = createdByUserId,
            UpdatedAt = updatedAt,
            UpdatedByUserId = updatedByUserId
        };

        return location;
    }

    [Obsolete("Use FromPersistence instead.", false)]
    public static InventoryLocation Restore(
        Guid id,
        Guid siteId,
        Guid? roomId,
        Guid? parentId,
        LocationType locationType,
        string code,
        string name,
        string? barcode,
        string path,
        int depth,
        LocationStatus status,
        decimal? lengthFt,
        decimal? widthFt,
        decimal? heightFt,
        int? plantCapacity,
        int currentPlantCount,
        int? rowNumber,
        int? columnNumber,
        decimal? weightCapacityLbs,
        decimal currentWeightLbs,
        string? notes,
        string? metadataJson,
        DateTime createdAt,
        Guid createdByUserId,
        DateTime updatedAt,
        Guid updatedByUserId)
        => FromPersistence(
            id,
            siteId,
            roomId,
            parentId,
            locationType,
            code,
            name,
            barcode,
            path,
            depth,
            status,
            lengthFt,
            widthFt,
            heightFt,
            plantCapacity,
            currentPlantCount,
            rowNumber,
            columnNumber,
            weightCapacityLbs,
            currentWeightLbs,
            notes,
            metadataJson,
            createdAt,
            createdByUserId,
            updatedAt,
            updatedByUserId);
}
