using System;
using Harvestry.Spatial.Domain.Enums;

namespace Harvestry.Spatial.Domain.Entities;

public partial class Room
{
    public static Room FromPersistence(
        Guid id,
        Guid siteId,
        string code,
        string name,
        RoomType roomType,
        string? customRoomType,
        RoomStatus status,
        string? description,
        int? floorLevel,
        decimal? areaSqft,
        decimal? heightFt,
        decimal? targetTempF,
        decimal? targetHumidityPct,
        int? targetCo2Ppm,
        DateTime createdAt,
        Guid createdByUserId,
        DateTime updatedAt,
        Guid updatedByUserId)
    {
        var room = new Room(id)
        {
            SiteId = siteId,
            Code = code,
            Name = name,
            RoomType = roomType,
            CustomRoomType = customRoomType,
            Status = status,
            Description = description,
            FloorLevel = floorLevel,
            AreaSqft = areaSqft,
            HeightFt = heightFt,
            TargetTempF = targetTempF,
            TargetHumidityPct = targetHumidityPct,
            TargetCo2Ppm = targetCo2Ppm,
            CreatedAt = createdAt,
            CreatedByUserId = createdByUserId,
            UpdatedAt = updatedAt,
            UpdatedByUserId = updatedByUserId
        };

        return room;
    }

    [Obsolete("Use FromPersistence instead.", false)]
    public static Room Restore(
        Guid id,
        Guid siteId,
        string code,
        string name,
        RoomType roomType,
        string? customRoomType,
        RoomStatus status,
        string? description,
        int? floorLevel,
        decimal? areaSqft,
        decimal? heightFt,
        decimal? targetTempF,
        decimal? targetHumidityPct,
        int? targetCo2Ppm,
        DateTime createdAt,
        Guid createdByUserId,
        DateTime updatedAt,
        Guid updatedByUserId)
        => FromPersistence(
            id,
            siteId,
            code,
            name,
            roomType,
            customRoomType,
            status,
            description,
            floorLevel,
            areaSqft,
            heightFt,
            targetTempF,
            targetHumidityPct,
            targetCo2Ppm,
            createdAt,
            createdByUserId,
            updatedAt,
            updatedByUserId);
}
