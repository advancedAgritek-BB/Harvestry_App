using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Spatial.Application.DTOs;
using Harvestry.Spatial.Application.ViewModels;
using Harvestry.Spatial.Domain.Enums;

namespace Harvestry.Spatial.Application.Interfaces;

/// <summary>
/// Application service for working with the spatial hierarchy (rooms and inventory locations).
/// </summary>
public interface ISpatialHierarchyService
{
    Task<RoomResponse> CreateRoomAsync(CreateRoomRequest request, CancellationToken cancellationToken = default);

    Task<RoomResponse> UpdateRoomAsync(Guid siteId, Guid roomId, UpdateRoomRequest request, CancellationToken cancellationToken = default);

    Task<RoomResponse?> GetRoomWithHierarchyAsync(Guid siteId, Guid roomId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RoomResponse>> GetRoomsBySiteAsync(Guid siteId, CancellationToken cancellationToken = default);

    Task<RoomResponse> ChangeRoomStatusAsync(Guid siteId, Guid roomId, RoomStatus newStatus, Guid requestedByUserId, CancellationToken cancellationToken = default);

    Task<InventoryLocationNodeResponse> CreateLocationAsync(CreateLocationRequest request, CancellationToken cancellationToken = default);

    Task<InventoryLocationNodeResponse> UpdateLocationAsync(Guid siteId, Guid locationId, UpdateLocationRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InventoryLocationNodeResponse>> GetLocationChildrenAsync(Guid siteId, Guid locationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LocationPathSegment>> GetLocationPathAsync(Guid siteId, Guid locationId, CancellationToken cancellationToken = default);

    Task DeleteLocationAsync(Guid siteId, Guid locationId, Guid requestedByUserId, CancellationToken cancellationToken = default);
}
