using System;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Spatial.Application.DTOs;
using Harvestry.Spatial.Application.ViewModels;
using Harvestry.Spatial.Domain.Enums;

namespace Harvestry.Spatial.Application.Interfaces;

/// <summary>
/// Application service responsible for managing equipment lifecycle and telemetry metadata.
/// </summary>
public interface IEquipmentRegistryService
{
    Task<EquipmentResponse> CreateAsync(CreateEquipmentRequest request, CancellationToken cancellationToken = default);

    Task<EquipmentResponse?> GetByIdAsync(Guid siteId, Guid equipmentId, bool includeChannels, CancellationToken cancellationToken = default);

    Task<EquipmentListResponse> GetListAsync(Guid siteId, EquipmentListQuery query, CancellationToken cancellationToken = default);

    Task<EquipmentResponse> UpdateAsync(Guid siteId, Guid equipmentId, UpdateEquipmentRequest request, CancellationToken cancellationToken = default);

    Task RecordHeartbeatAsync(Guid siteId, Guid equipmentId, RecordHeartbeatRequest request, CancellationToken cancellationToken = default);

    Task UpdateNetworkAsync(Guid siteId, Guid equipmentId, UpdateNetworkRequest request, CancellationToken cancellationToken = default);

    Task<EquipmentResponse> ChangeStatusAsync(Guid siteId, Guid equipmentId, EquipmentStatus status, Guid requestedByUserId, CancellationToken cancellationToken = default);

    Task<EquipmentChannelResponse> AddChannelAsync(Guid siteId, Guid equipmentId, CreateEquipmentChannelRequest request, CancellationToken cancellationToken = default);

    Task<EquipmentChannelResponse[]> GetChannelsAsync(Guid siteId, Guid equipmentId, CancellationToken cancellationToken = default);

    Task RemoveChannelAsync(Guid siteId, Guid equipmentId, Guid channelId, Guid requestedByUserId, CancellationToken cancellationToken = default);
}
