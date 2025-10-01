using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Spatial.Application.DTOs;
using Harvestry.Spatial.Application.Exceptions;
using Harvestry.Spatial.Application.Interfaces;
using Harvestry.Spatial.Application.Mappers;
using Harvestry.Spatial.Application.ViewModels;
using Harvestry.Spatial.Domain.Entities;
using Harvestry.Spatial.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Harvestry.Spatial.Application.Services;

/// <summary>
/// Implements the orchestration logic for room and inventory location operations.
/// </summary>
public sealed class SpatialHierarchyService : ISpatialHierarchyService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IInventoryLocationRepository _locationRepository;
    private readonly ILogger<SpatialHierarchyService> _logger;

    private static readonly IReadOnlyDictionary<LocationType, LocationType> CultivationParentMap =
        new Dictionary<LocationType, LocationType>
        {
            { LocationType.Zone, LocationType.Room },
            { LocationType.SubZone, LocationType.Zone },
            { LocationType.Row, LocationType.SubZone },
            { LocationType.Position, LocationType.Row }
        };

    private static readonly IReadOnlyDictionary<LocationType, LocationType> WarehouseParentMap =
        new Dictionary<LocationType, LocationType>
        {
            { LocationType.Rack, LocationType.Room },
            { LocationType.Shelf, LocationType.Rack },
            { LocationType.Bin, LocationType.Shelf }
        };

    public SpatialHierarchyService(
        IRoomRepository roomRepository,
        IInventoryLocationRepository locationRepository,
        ILogger<SpatialHierarchyService> logger)
    {
        _roomRepository = roomRepository ?? throw new ArgumentNullException(nameof(roomRepository));
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RoomResponse> CreateRoomAsync(CreateRoomRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        ValidateIdentifier(request.SiteId, nameof(request.SiteId));
        ValidateIdentifier(request.RequestedByUserId, nameof(request.RequestedByUserId));

        var code = NormalizeCode(request.Code);
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Room name is required", nameof(request.Name));
        }

        var existing = await _roomRepository.GetByCodeAsync(request.SiteId, code, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            throw new InvalidOperationException($"Room code '{code}' already exists for site {request.SiteId}");
        }

        var room = new Room(
            request.SiteId,
            code,
            request.Name,
            request.RoomType,
            request.RequestedByUserId,
            request.CustomRoomType,
            request.Description);

        if (request.FloorLevel.HasValue || request.AreaSqft.HasValue || request.HeightFt.HasValue)
        {
            room.UpdateDimensions(request.FloorLevel, request.AreaSqft, request.HeightFt, request.RequestedByUserId);
        }

        if (request.TargetTempF.HasValue || request.TargetHumidityPct.HasValue || request.TargetCo2Ppm.HasValue)
        {
            room.UpdateEnvironmentTargets(request.TargetTempF, request.TargetHumidityPct, request.TargetCo2Ppm, request.RequestedByUserId);
        }

        await _roomRepository.InsertAsync(room, cancellationToken).ConfigureAwait(false);

        var rootLocation = new InventoryLocation(
            request.SiteId,
            code,
            request.Name,
            LocationType.Room,
            request.RequestedByUserId,
            room.Id,
            null,
            barcode: null);

        try
        {
            await _locationRepository.InsertAsync(rootLocation, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create root location for room {RoomId}. Rolling back room create.", room.Id);
            try
            {
                await _roomRepository.DeleteAsync(room.Id, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception rollbackEx)
            {
                _logger.LogWarning(rollbackEx, "Failed to roll back room {RoomId} after location create failure", room.Id);
            }

            throw;
        }

        return await BuildRoomResponseAsync(room.Id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<RoomResponse> UpdateRoomAsync(Guid siteId, Guid roomId, UpdateRoomRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        ValidateIdentifier(siteId, nameof(siteId));
        ValidateIdentifier(roomId, nameof(roomId));
        ValidateIdentifier(request.RequestedByUserId, nameof(request.RequestedByUserId));

        var room = await _roomRepository.GetByIdAsync(roomId, cancellationToken).ConfigureAwait(false)
                   ?? throw new KeyNotFoundException($"Room {roomId} was not found");

        EnsureSameSite(siteId, room.SiteId, nameof(Room), room.Id);

        room.UpdateInfo(request.Name, request.Description, request.RequestedByUserId);

        if (request.FloorLevel.HasValue || request.AreaSqft.HasValue || request.HeightFt.HasValue)
        {
            room.UpdateDimensions(request.FloorLevel, request.AreaSqft, request.HeightFt, request.RequestedByUserId);
        }

        if (request.TargetTempF.HasValue || request.TargetHumidityPct.HasValue || request.TargetCo2Ppm.HasValue)
        {
            room.UpdateEnvironmentTargets(request.TargetTempF, request.TargetHumidityPct, request.TargetCo2Ppm, request.RequestedByUserId);
        }

        await _roomRepository.UpdateAsync(room, cancellationToken).ConfigureAwait(false);

        var roomLocations = await _locationRepository.GetByRoomAsync(room.Id, cancellationToken).ConfigureAwait(false);
        var rootLocation = roomLocations.FirstOrDefault(l => l.LocationType == LocationType.Room && l.ParentId is null);
        if (rootLocation is not null)
        {
            rootLocation.UpdateInfo(request.Name, rootLocation.Barcode, rootLocation.Notes, request.RequestedByUserId);
            await _locationRepository.UpdateAsync(rootLocation, cancellationToken).ConfigureAwait(false);
        }

        return await BuildRoomResponseAsync(room.Id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<RoomResponse?> GetRoomWithHierarchyAsync(Guid siteId, Guid roomId, CancellationToken cancellationToken = default)
    {
        ValidateIdentifier(siteId, nameof(siteId));
        ValidateIdentifier(roomId, nameof(roomId));

        var room = await _roomRepository.GetByIdAsync(roomId, cancellationToken).ConfigureAwait(false);
        if (room is null)
        {
            return null;
        }

        EnsureSameSite(siteId, room.SiteId, nameof(Room), room.Id);

        return await BuildRoomResponseAsync(room, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<RoomResponse>> GetRoomsBySiteAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        ValidateIdentifier(siteId, nameof(siteId));

        var rooms = await _roomRepository.GetBySiteAsync(siteId, cancellationToken).ConfigureAwait(false);
        var responses = new List<RoomResponse>(rooms.Count);

        foreach (var room in rooms)
        {
            responses.Add(await BuildRoomResponseAsync(room, cancellationToken).ConfigureAwait(false));
        }

        return responses;
    }

    public async Task<RoomResponse> ChangeRoomStatusAsync(Guid siteId, Guid roomId, RoomStatus newStatus, Guid requestedByUserId, CancellationToken cancellationToken = default)
    {
        ValidateIdentifier(siteId, nameof(siteId));
        ValidateIdentifier(roomId, nameof(roomId));
        ValidateIdentifier(requestedByUserId, nameof(requestedByUserId));

        var room = await _roomRepository.GetByIdAsync(roomId, cancellationToken).ConfigureAwait(false)
                   ?? throw new KeyNotFoundException($"Room {roomId} was not found");

        EnsureSameSite(siteId, room.SiteId, nameof(Room), room.Id);

        room.ChangeStatus(newStatus, requestedByUserId);
        await _roomRepository.UpdateAsync(room, cancellationToken).ConfigureAwait(false);

        var locations = await _locationRepository.GetByRoomAsync(room.Id, cancellationToken).ConfigureAwait(false);
        var rootLocation = locations.FirstOrDefault(l => l.LocationType == LocationType.Room && l.ParentId is null);
        if (rootLocation is not null)
        {
            var mappedStatus = MapRoomStatusToLocationStatus(newStatus);
            rootLocation.ChangeStatus(mappedStatus, requestedByUserId);
            await _locationRepository.UpdateAsync(rootLocation, cancellationToken).ConfigureAwait(false);
        }

        return await BuildRoomResponseAsync(room, cancellationToken).ConfigureAwait(false);
    }

    public async Task<InventoryLocationNodeResponse> CreateLocationAsync(CreateLocationRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        ValidateIdentifier(request.SiteId, nameof(request.SiteId));
        ValidateIdentifier(request.RequestedByUserId, nameof(request.RequestedByUserId));

        if (request.LocationType == LocationType.Room)
        {
            throw new InvalidOperationException("Room locations are created automatically when a room is created.");
        }

        var code = NormalizeCode(request.Code);
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Location name is required", nameof(request.Name));
        }

        InventoryLocation? parent = null;
        if (!request.ParentLocationId.HasValue)
        {
            throw new InvalidOperationException("Parent location must be provided for non-room nodes.");
        }

        parent = await _locationRepository.GetByIdAsync(request.ParentLocationId.Value, cancellationToken).ConfigureAwait(false)
                 ?? throw new KeyNotFoundException($"Parent location {request.ParentLocationId} was not found");

        EnsureSameSite(request.SiteId, parent.SiteId, nameof(InventoryLocation), parent.Id);

        ValidateParentChildRelationship(request.LocationType, parent.LocationType);

        var roomId = parent.RoomId ?? throw new InvalidOperationException("Parent location is missing room association.");

        var location = new InventoryLocation(
            request.SiteId,
            code,
            request.Name,
            request.LocationType,
            request.RequestedByUserId,
            roomId,
            parent.Id,
            request.Barcode);

        if (request.LengthFt.HasValue || request.WidthFt.HasValue || request.HeightFt.HasValue)
        {
            location.UpdateDimensions(request.LengthFt, request.WidthFt, request.HeightFt, request.RequestedByUserId);
        }

        if (request.PlantCapacity.HasValue || request.RowNumber.HasValue || request.ColumnNumber.HasValue)
        {
            location.UpdateCultivationCapacity(request.PlantCapacity, request.RowNumber, request.ColumnNumber, request.RequestedByUserId);
        }

        if (request.WeightCapacityLbs.HasValue)
        {
            location.UpdateWarehouseCapacity(request.WeightCapacityLbs, request.RequestedByUserId);
        }

        if (!string.IsNullOrWhiteSpace(request.Notes) || request.MetadataJson is not null)
        {
            location.UpdateInfo(request.Name, request.Barcode, request.Notes, request.RequestedByUserId);
        }

        if (request.MetadataJson is not null)
        {
            if (string.IsNullOrWhiteSpace(request.MetadataJson))
            {
                location.UpdateMetadata(null, request.RequestedByUserId);
            }
            else
            {
                ValidateJson(request.MetadataJson);
                location.UpdateMetadata(request.MetadataJson, request.RequestedByUserId);
            }
        }

        await _locationRepository.InsertAsync(location, cancellationToken).ConfigureAwait(false);

        var created = await _locationRepository.GetByIdAsync(location.Id, cancellationToken).ConfigureAwait(false)
                      ?? location;

        return SpatialHierarchyMapper.ToLocationNode(created);
    }

    public async Task<InventoryLocationNodeResponse> UpdateLocationAsync(Guid siteId, Guid locationId, UpdateLocationRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        ValidateIdentifier(siteId, nameof(siteId));
        ValidateIdentifier(locationId, nameof(locationId));
        ValidateIdentifier(request.RequestedByUserId, nameof(request.RequestedByUserId));

        var location = await _locationRepository.GetByIdAsync(locationId, cancellationToken).ConfigureAwait(false)
                       ?? throw new KeyNotFoundException($"Location {locationId} was not found");

        EnsureSameSite(siteId, location.SiteId, nameof(InventoryLocation), location.Id);

        location.UpdateInfo(request.Name, request.Barcode, request.Notes, request.RequestedByUserId);

        if (request.LengthFt.HasValue || request.WidthFt.HasValue || request.HeightFt.HasValue)
        {
            location.UpdateDimensions(request.LengthFt, request.WidthFt, request.HeightFt, request.RequestedByUserId);
        }

        if (request.PlantCapacity.HasValue || request.RowNumber.HasValue || request.ColumnNumber.HasValue)
        {
            location.UpdateCultivationCapacity(request.PlantCapacity, request.RowNumber, request.ColumnNumber, request.RequestedByUserId);
        }

        if (request.WeightCapacityLbs.HasValue)
        {
            location.UpdateWarehouseCapacity(request.WeightCapacityLbs, request.RequestedByUserId);
        }

        if (request.MetadataJson is not null)
        {
            if (string.IsNullOrWhiteSpace(request.MetadataJson))
            {
                location.UpdateMetadata(null, request.RequestedByUserId);
            }
            else
            {
                ValidateJson(request.MetadataJson);
                location.UpdateMetadata(request.MetadataJson, request.RequestedByUserId);
            }
        }

        if (request.Status.HasValue)
        {
            location.ChangeStatus(request.Status.Value, request.RequestedByUserId);
        }

        await _locationRepository.UpdateAsync(location, cancellationToken).ConfigureAwait(false);

        var persisted = await _locationRepository.GetByIdAsync(locationId, cancellationToken).ConfigureAwait(false) ?? location;
        var children = await _locationRepository.GetChildrenAsync(locationId, cancellationToken).ConfigureAwait(false);
        var childResponses = children
            .Select(child => SpatialHierarchyMapper.ToLocationNode(child))
            .ToArray();

        return SpatialHierarchyMapper.ToLocationNode(persisted, childResponses);
    }

    public async Task<IReadOnlyList<InventoryLocationNodeResponse>> GetLocationChildrenAsync(Guid siteId, Guid locationId, CancellationToken cancellationToken = default)
    {
        ValidateIdentifier(siteId, nameof(siteId));
        ValidateIdentifier(locationId, nameof(locationId));

        var parent = await _locationRepository.GetByIdAsync(locationId, cancellationToken).ConfigureAwait(false)
                     ?? throw new KeyNotFoundException($"Location {locationId} was not found");

        EnsureSameSite(siteId, parent.SiteId, nameof(InventoryLocation), parent.Id);

        var children = await _locationRepository.GetChildrenAsync(locationId, cancellationToken).ConfigureAwait(false);
        return children
            .Select(child => SpatialHierarchyMapper.ToLocationNode(child))
            .ToArray();
    }

    public async Task<IReadOnlyList<LocationPathSegment>> GetLocationPathAsync(Guid siteId, Guid locationId, CancellationToken cancellationToken = default)
    {
        ValidateIdentifier(siteId, nameof(siteId));
        ValidateIdentifier(locationId, nameof(locationId));

        var location = await _locationRepository.GetByIdAsync(locationId, cancellationToken).ConfigureAwait(false)
                       ?? throw new KeyNotFoundException($"Location {locationId} was not found");

        EnsureSameSite(siteId, location.SiteId, nameof(InventoryLocation), location.Id);

        var pathNodes = await _locationRepository.GetPathAsync(locationId, cancellationToken).ConfigureAwait(false);
        return SpatialHierarchyMapper.ToPathSegments(pathNodes);
    }

    public async Task DeleteLocationAsync(Guid siteId, Guid locationId, Guid requestedByUserId, CancellationToken cancellationToken = default)
    {
        ValidateIdentifier(siteId, nameof(siteId));
        ValidateIdentifier(locationId, nameof(locationId));
        ValidateIdentifier(requestedByUserId, nameof(requestedByUserId));

        var location = await _locationRepository.GetByIdAsync(locationId, cancellationToken).ConfigureAwait(false)
                       ?? throw new KeyNotFoundException($"Location {locationId} was not found");

        EnsureSameSite(siteId, location.SiteId, nameof(InventoryLocation), location.Id);

        if (location.LocationType == LocationType.Room)
        {
            throw new InvalidOperationException("Cannot delete root room location. Delete the room instead.");
        }

        var descendants = await _locationRepository.GetDescendantsAsync(locationId, cancellationToken).ConfigureAwait(false);
        if (descendants.Count > 0)
        {
            throw new InvalidOperationException("Cannot delete a location that still has child nodes. Delete children first.");
        }

        await _locationRepository.DeleteAsync(locationId, cancellationToken).ConfigureAwait(false);
    }

    private async Task<RoomResponse> BuildRoomResponseAsync(Guid roomId, CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetByIdAsync(roomId, cancellationToken).ConfigureAwait(false)
                   ?? throw new KeyNotFoundException($"Room {roomId} was not found");

        return await BuildRoomResponseAsync(room, cancellationToken).ConfigureAwait(false);
    }

    private async Task<RoomResponse> BuildRoomResponseAsync(Room room, CancellationToken cancellationToken)
    {
        var locations = await _locationRepository.GetByRoomAsync(room.Id, cancellationToken).ConfigureAwait(false);
        return SpatialHierarchyMapper.ToRoomResponse(room, locations);
    }

    private static void EnsureSameSite(Guid expectedSiteId, Guid actualSiteId, string entityName, Guid entityId)
    {
        if (expectedSiteId != actualSiteId)
        {
            var message = $"{entityName} {entityId} belongs to site {actualSiteId}, but the request targeted site {expectedSiteId}.";
            throw new TenantMismatchException(expectedSiteId, actualSiteId, message);
        }
    }

    private static void ValidateIdentifier(Guid value, string argumentName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException($"{argumentName} cannot be empty", argumentName);
        }
    }

    private static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code is required", nameof(code));
        }

        return code.Trim().ToUpperInvariant();
    }

    private static void ValidateParentChildRelationship(LocationType child, LocationType parent)
    {
        if (CultivationParentMap.TryGetValue(child, out var expectedCultivationParent))
        {
            if (expectedCultivationParent != parent)
            {
                throw new InvalidOperationException($"{child} locations must be placed under a {expectedCultivationParent}.");
            }
            return;
        }

        if (WarehouseParentMap.TryGetValue(child, out var expectedWarehouseParent))
        {
            if (expectedWarehouseParent != parent)
            {
                throw new InvalidOperationException($"{child} locations must be placed under a {expectedWarehouseParent}.");
            }
            return;
        }

        throw new InvalidOperationException($"Unsupported location type {child}.");
    }

    private static LocationStatus MapRoomStatusToLocationStatus(RoomStatus status)
    {
        return status switch
        {
            RoomStatus.Active => LocationStatus.Active,
            RoomStatus.Inactive => LocationStatus.Inactive,
            RoomStatus.Maintenance => LocationStatus.Reserved,
            RoomStatus.Quarantine => LocationStatus.Quarantine,
            _ => LocationStatus.Active
        };
    }

    private static void ValidateJson(string json)
    {
        try
        {
            JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("MetadataJson must be valid JSON.", nameof(json), ex);
        }
    }
}
