using System;
using System.Collections.Generic;
using System.Linq;
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
/// Implements equipment lifecycle operations (CRUD, telemetry, channels).
/// </summary>
public sealed class EquipmentRegistryService : IEquipmentRegistryService
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IEquipmentChannelRepository _channelRepository;
    private readonly IInventoryLocationRepository _locationRepository;
    private readonly ILogger<EquipmentRegistryService> _logger;

    public EquipmentRegistryService(
        IEquipmentRepository equipmentRepository,
        IEquipmentChannelRepository channelRepository,
        IInventoryLocationRepository locationRepository,
        ILogger<EquipmentRegistryService> logger)
    {
        _equipmentRepository = equipmentRepository ?? throw new ArgumentNullException(nameof(equipmentRepository));
        _channelRepository = channelRepository ?? throw new ArgumentNullException(nameof(channelRepository));
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<EquipmentResponse> CreateAsync(CreateEquipmentRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        ValidateIdentifier(request.SiteId, nameof(request.SiteId));
        ValidateIdentifier(request.LocationId, nameof(request.LocationId));
        ValidateIdentifier(request.RequestedByUserId, nameof(request.RequestedByUserId));

        var location = await _locationRepository.GetByIdAsync(request.LocationId, cancellationToken).ConfigureAwait(false)
                       ?? throw new KeyNotFoundException($"Location {request.LocationId} was not found");
        EnsureSameSite(request.SiteId, location.SiteId, nameof(InventoryLocation), location.Id);

        var equipment = new Equipment(
            request.SiteId,
            request.Code,
            request.TypeCode,
            request.CoreType,
            request.RequestedByUserId,
            request.Manufacturer,
            request.Model,
            request.SerialNumber);

        equipment.UpdateInfo(request.Manufacturer, request.Model, request.SerialNumber, request.Notes, request.RequestedByUserId);

        if (!string.IsNullOrWhiteSpace(request.FirmwareVersion))
        {
            equipment.UpdateFirmwareVersion(request.FirmwareVersion, request.RequestedByUserId);
        }

        equipment.AssignToLocation(request.LocationId, request.RequestedByUserId);
        equipment.UpdateDeviceTwin(request.MetadataJson ?? "{}", request.RequestedByUserId);

        await _equipmentRepository.InsertAsync(equipment, cancellationToken).ConfigureAwait(false);

        var response = EquipmentMapper.ToResponse(equipment);
        return response;
    }

    public async Task<EquipmentResponse?> GetByIdAsync(Guid siteId, Guid equipmentId, bool includeChannels, CancellationToken cancellationToken = default)
    {
        ValidateIdentifier(siteId, nameof(siteId));
        ValidateIdentifier(equipmentId, nameof(equipmentId));

        var equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken).ConfigureAwait(false);
        if (equipment is null)
        {
            return null;
        }

        EnsureSameSite(siteId, equipment.SiteId, nameof(Equipment), equipment.Id);

        IReadOnlyList<EquipmentChannel>? channels = null;
        if (includeChannels)
        {
            channels = await _channelRepository.GetByEquipmentIdAsync(equipmentId, cancellationToken).ConfigureAwait(false);
        }

        return EquipmentMapper.ToResponse(equipment, channels);
    }

    public async Task<EquipmentListResponse> GetListAsync(Guid siteId, EquipmentListQuery query, CancellationToken cancellationToken = default)
    {
        ValidateIdentifier(siteId, nameof(siteId));
        query ??= new EquipmentListQuery();

        if (query.Page <= 0)
        {
            query = query with { Page = 1 };
        }

        if (query.PageSize <= 0)
        {
            query = query with { PageSize = 50 };
        }

        var result = await _equipmentRepository.GetBySiteAsync(siteId, query, cancellationToken).ConfigureAwait(false);

        IReadOnlyList<EquipmentChannel> channelCache = Array.Empty<EquipmentChannel>();
        if (query.IncludeChannels && result.Items.Count > 0)
        {
            var channelTasks = result.Items.Select(e => _channelRepository.GetByEquipmentIdAsync(e.Id, cancellationToken));
            channelCache = (await Task.WhenAll(channelTasks).ConfigureAwait(false)).SelectMany(x => x).ToArray();
        }

        var responses = result.Items
            .Select(eq => EquipmentMapper.ToResponse(eq, query.IncludeChannels ? channelCache.Where(c => c.EquipmentId == eq.Id).ToArray() : null))
            .ToArray();

        return new EquipmentListResponse
        {
            Items = responses,
            TotalCount = result.TotalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<EquipmentResponse> UpdateAsync(Guid siteId, Guid equipmentId, UpdateEquipmentRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        ValidateIdentifier(siteId, nameof(siteId));
        ValidateIdentifier(equipmentId, nameof(equipmentId));
        ValidateIdentifier(request.RequestedByUserId, nameof(request.RequestedByUserId));

        var equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken).ConfigureAwait(false)
                       ?? throw new KeyNotFoundException($"Equipment {equipmentId} was not found");

        EnsureSameSite(siteId, equipment.SiteId, nameof(Equipment), equipment.Id);

        equipment.UpdateInfo(request.Manufacturer, request.Model, request.SerialNumber, request.Notes, request.RequestedByUserId);

        if (!string.IsNullOrWhiteSpace(request.FirmwareVersion))
        {
            equipment.UpdateFirmwareVersion(request.FirmwareVersion, request.RequestedByUserId);
        }

        if (!string.IsNullOrWhiteSpace(request.MetadataJson))
        {
            equipment.UpdateDeviceTwin(request.MetadataJson, request.RequestedByUserId);
        }

        if (request.LocationId.HasValue && request.LocationId.Value != equipment.LocationId)
        {
            var location = await _locationRepository.GetByIdAsync(request.LocationId.Value, cancellationToken).ConfigureAwait(false)
                           ?? throw new KeyNotFoundException($"Location {request.LocationId} was not found");
            EnsureSameSite(siteId, location.SiteId, nameof(InventoryLocation), location.Id);
            equipment.AssignToLocation(location.Id, request.RequestedByUserId);
        }
        else if (!request.LocationId.HasValue && equipment.LocationId.HasValue)
        {
            equipment.UnassignFromLocation(request.RequestedByUserId);
        }

        await _equipmentRepository.UpdateAsync(equipment, cancellationToken).ConfigureAwait(false);
        return EquipmentMapper.ToResponse(equipment);
    }

    public async Task RecordHeartbeatAsync(Guid siteId, Guid equipmentId, RecordHeartbeatRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        ValidateIdentifier(siteId, nameof(siteId));
        ValidateIdentifier(equipmentId, nameof(equipmentId));

        var equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken).ConfigureAwait(false)
                       ?? throw new KeyNotFoundException($"Equipment {equipmentId} was not found");

        EnsureSameSite(siteId, equipment.SiteId, nameof(Equipment), equipment.Id);

        equipment.RecordHeartbeat(request.HeartbeatAt, request.SignalStrengthDbm, request.BatteryPercent, request.UptimeSeconds);
        await _equipmentRepository.UpdateAsync(equipment, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateNetworkAsync(Guid siteId, Guid equipmentId, UpdateNetworkRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        ValidateIdentifier(siteId, nameof(siteId));
        ValidateIdentifier(equipmentId, nameof(equipmentId));
        ValidateIdentifier(request.RequestedByUserId, nameof(request.RequestedByUserId));

        var equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken).ConfigureAwait(false)
                       ?? throw new KeyNotFoundException($"Equipment {equipmentId} was not found");

        EnsureSameSite(siteId, equipment.SiteId, nameof(Equipment), equipment.Id);

        equipment.UpdateNetworkConfig(request.IpAddress, request.MacAddress, request.MqttTopic, request.RequestedByUserId);
        await _equipmentRepository.UpdateAsync(equipment, cancellationToken).ConfigureAwait(false);
    }

    public async Task<EquipmentResponse> ChangeStatusAsync(Guid siteId, Guid equipmentId, EquipmentStatus status, Guid requestedByUserId, CancellationToken cancellationToken = default)
    {
        ValidateIdentifier(siteId, nameof(siteId));
        ValidateIdentifier(equipmentId, nameof(equipmentId));
        ValidateIdentifier(requestedByUserId, nameof(requestedByUserId));

        var equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken).ConfigureAwait(false)
                       ?? throw new KeyNotFoundException($"Equipment {equipmentId} was not found");

        EnsureSameSite(siteId, equipment.SiteId, nameof(Equipment), equipment.Id);

        if (status == EquipmentStatus.Active && !equipment.LocationId.HasValue)
        {
            throw new InvalidOperationException("Active equipment must be assigned to a location.");
        }

        equipment.ChangeStatus(status, requestedByUserId);
        await _equipmentRepository.UpdateAsync(equipment, cancellationToken).ConfigureAwait(false);
        var channels = await _channelRepository.GetByEquipmentIdAsync(equipmentId, cancellationToken).ConfigureAwait(false);
        return EquipmentMapper.ToResponse(equipment, channels);
    }

    public async Task<EquipmentChannelResponse> AddChannelAsync(Guid siteId, Guid equipmentId, CreateEquipmentChannelRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        ValidateIdentifier(siteId, nameof(siteId));
        ValidateIdentifier(equipmentId, nameof(equipmentId));

        var equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken).ConfigureAwait(false)
                       ?? throw new KeyNotFoundException($"Equipment {equipmentId} was not found");
        EnsureSameSite(siteId, equipment.SiteId, nameof(Equipment), equipment.Id);

        var channel = new EquipmentChannel(equipmentId, request.ChannelCode, request.Role, request.PortMetaJson);
        if (request.AssignedZoneId.HasValue)
        {
            channel.AssignToZone(request.AssignedZoneId.Value);
        }

        await _channelRepository.InsertAsync(channel, cancellationToken).ConfigureAwait(false);
        return EquipmentMapper.ToChannelResponse(channel);
    }

    public async Task<EquipmentChannelResponse[]> GetChannelsAsync(Guid siteId, Guid equipmentId, CancellationToken cancellationToken = default)
    {
        ValidateIdentifier(siteId, nameof(siteId));
        ValidateIdentifier(equipmentId, nameof(equipmentId));

        var equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken).ConfigureAwait(false)
                       ?? throw new KeyNotFoundException($"Equipment {equipmentId} was not found");
        EnsureSameSite(siteId, equipment.SiteId, nameof(Equipment), equipment.Id);

        var channels = await _channelRepository.GetByEquipmentIdAsync(equipmentId, cancellationToken).ConfigureAwait(false);
        return channels.Select(EquipmentMapper.ToChannelResponse).ToArray();
    }

    public async Task RemoveChannelAsync(Guid siteId, Guid equipmentId, Guid channelId, Guid requestedByUserId, CancellationToken cancellationToken = default)
    {
        ValidateIdentifier(siteId, nameof(siteId));
        ValidateIdentifier(equipmentId, nameof(equipmentId));
        ValidateIdentifier(channelId, nameof(channelId));
        ValidateIdentifier(requestedByUserId, nameof(requestedByUserId));

        var equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken).ConfigureAwait(false)
                       ?? throw new KeyNotFoundException($"Equipment {equipmentId} was not found");
        EnsureSameSite(siteId, equipment.SiteId, nameof(Equipment), equipment.Id);

        var channel = await _channelRepository.GetByIdAsync(channelId, cancellationToken).ConfigureAwait(false)
                      ?? throw new KeyNotFoundException($"Equipment channel {channelId} was not found");

        if (channel.EquipmentId != equipmentId)
        {
            throw new InvalidOperationException("Channel does not belong to specified equipment.");
        }

        await _channelRepository.DeleteAsync(channelId, cancellationToken).ConfigureAwait(false);
    }

    private static void ValidateIdentifier(Guid value, string argumentName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException($"{argumentName} cannot be empty", argumentName);
        }
    }

    private static void EnsureSameSite(Guid expectedSiteId, Guid actualSiteId, string entityName, Guid entityId)
    {
        if (expectedSiteId != actualSiteId)
        {
            var message = $"{entityName} {entityId} belongs to site {actualSiteId}, but the request targeted site {expectedSiteId}.";
            throw new TenantMismatchException(expectedSiteId, actualSiteId, message);
        }
    }
}
