using System;
using System.Collections.Generic;
using System.Linq;
using Harvestry.Spatial.Application.ViewModels;
using Harvestry.Spatial.Domain.Entities;
using Harvestry.Spatial.Domain.Enums;

namespace Harvestry.Spatial.Application.Mappers;

public static class SpatialHierarchyMapper
{
    public static RoomResponse ToRoomResponse(Room room, IReadOnlyCollection<InventoryLocation> locations)
    {
        if (room == null) throw new ArgumentNullException(nameof(room));
        if (locations == null) throw new ArgumentNullException(nameof(locations));

        return new RoomResponse
        {
            Id = room.Id,
            SiteId = room.SiteId,
            Code = room.Code,
            Name = room.Name,
            RoomType = room.RoomType,
            CustomRoomType = room.CustomRoomType,
            Status = room.Status,
            Description = room.Description,
            FloorLevel = room.FloorLevel,
            AreaSqft = room.AreaSqft,
            HeightFt = room.HeightFt,
            TargetTempF = room.TargetTempF,
            TargetHumidityPct = room.TargetHumidityPct,
            TargetCo2Ppm = room.TargetCo2Ppm,
            CreatedAt = room.CreatedAt,
            CreatedByUserId = room.CreatedByUserId,
            UpdatedAt = room.UpdatedAt,
            UpdatedByUserId = room.UpdatedByUserId,
            RootLocation = BuildLocationHierarchy(room, locations)
        };
    }

    public static InventoryLocationNodeResponse ToLocationNode(
        InventoryLocation location,
        IReadOnlyList<InventoryLocationNodeResponse>? children = null)
    {
        if (location == null) throw new ArgumentNullException(nameof(location));

        return new InventoryLocationNodeResponse
        {
            Id = location.Id,
            SiteId = location.SiteId,
            RoomId = location.RoomId,
            ParentId = location.ParentId,
            LocationType = location.LocationType,
            Code = location.Code,
            Name = location.Name,
            Barcode = location.Barcode,
            Path = location.Path,
            Depth = location.Depth,
            Status = location.Status,
            LengthFt = location.LengthFt,
            WidthFt = location.WidthFt,
            HeightFt = location.HeightFt,
            PlantCapacity = location.PlantCapacity,
            CurrentPlantCount = location.CurrentPlantCount,
            RowNumber = location.RowNumber,
            ColumnNumber = location.ColumnNumber,
            WeightCapacityLbs = location.WeightCapacityLbs,
            CurrentWeightLbs = location.CurrentWeightLbs,
            Notes = location.Notes,
            MetadataJson = location.MetadataJson,
            CreatedAt = location.CreatedAt,
            CreatedByUserId = location.CreatedByUserId,
            UpdatedAt = location.UpdatedAt,
            UpdatedByUserId = location.UpdatedByUserId,
            Children = children ?? Array.Empty<InventoryLocationNodeResponse>()
        };
    }

    public static InventoryLocationNodeResponse? BuildLocationHierarchy(
        Room room,
        IReadOnlyCollection<InventoryLocation> locations)
    {
        if (room == null) throw new ArgumentNullException(nameof(room));
        if (locations == null) throw new ArgumentNullException(nameof(locations));

        if (locations.Count == 0)
        {
            return null;
        }

        var nodesByParent = locations
            .GroupBy(location => location.ParentId ?? Guid.Empty)
            .ToDictionary(group => group.Key, group => group.ToArray());

        var root = locations.FirstOrDefault(l => l.LocationType == LocationType.Room && l.RoomId == room.Id && l.ParentId is null);
        if (root is null)
        {
            return null;
        }

        InventoryLocationNodeResponse BuildNode(InventoryLocation location)
        {
            nodesByParent.TryGetValue(location.Id, out var children);
            var childResponses = children?.Select(BuildNode).ToArray() ?? Array.Empty<InventoryLocationNodeResponse>();
            return ToLocationNode(location, childResponses);
        }

        return BuildNode(root);
    }

    public static IReadOnlyList<LocationPathSegment> ToPathSegments(IEnumerable<InventoryLocation> nodes)
    {
        if (nodes == null) throw new ArgumentNullException(nameof(nodes));

        return nodes
            .OrderBy(node => node.Depth)
            .Select(node => new LocationPathSegment
            {
                LocationId = node.Id,
                Name = node.Name,
                Code = node.Code,
                LocationType = node.LocationType
            })
            .ToArray();
    }
}
