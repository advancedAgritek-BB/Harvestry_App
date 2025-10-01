using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Spatial.Application.Common;
using Harvestry.Spatial.Application.DTOs;
using Harvestry.Spatial.Application.Exceptions;
using Harvestry.Spatial.Application.Interfaces;
using Harvestry.Spatial.Application.Mappers;
using Harvestry.Spatial.Application.ViewModels;
using Harvestry.Spatial.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Harvestry.Spatial.Application.Services;

/// <summary>
/// Manages valve to zone routing relationships with tenant isolation and interlock validation.
/// </summary>
public sealed class ValveZoneMappingService : IValveZoneMappingService
{
    private readonly IValveZoneMappingRepository _mappingRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IEquipmentChannelRepository _channelRepository;
    private readonly IInventoryLocationRepository _locationRepository;
    private readonly ILogger<ValveZoneMappingService> _logger;

    public ValveZoneMappingService(
        IValveZoneMappingRepository mappingRepository,
        IEquipmentRepository equipmentRepository,
        IEquipmentChannelRepository channelRepository,
        IInventoryLocationRepository locationRepository,
        ILogger<ValveZoneMappingService> logger)
    {
        _mappingRepository = mappingRepository ?? throw new ArgumentNullException(nameof(mappingRepository));
        _equipmentRepository = equipmentRepository ?? throw new ArgumentNullException(nameof(equipmentRepository));
        _channelRepository = channelRepository ?? throw new ArgumentNullException(nameof(channelRepository));
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ValveZoneMappingResponse> CreateAsync(Guid siteId, Guid equipmentId, CreateValveZoneMappingRequest request, CancellationToken cancellationToken = default)
    {
        ValidationHelpers.EnsureNotEmpty(siteId, nameof(siteId));
        ValidationHelpers.EnsureNotEmpty(equipmentId, nameof(equipmentId));
        if (request == null) throw new ArgumentNullException(nameof(request));
        ValidationHelpers.EnsureNotEmpty(request.ZoneLocationId, nameof(request.ZoneLocationId));
        ValidationHelpers.EnsureNotEmpty(request.RequestedByUserId, nameof(request.RequestedByUserId));

        ValidationHelpers.EnsureRouteMatchesPayload(siteId, request.SiteId, nameof(request.SiteId));
        ValidationHelpers.EnsureRouteMatchesPayload(equipmentId, request.EquipmentId, nameof(request.EquipmentId));

        var equipment = await LoadEquipmentAsync(equipmentId, cancellationToken).ConfigureAwait(false)
                        ?? throw new KeyNotFoundException($"Equipment {equipmentId} was not found");
        ValidationHelpers.EnsureSameSite(siteId, equipment.SiteId, nameof(Equipment), equipment.Id);

        var zone = await _locationRepository.GetByIdAsync(request.ZoneLocationId, cancellationToken).ConfigureAwait(false)
                   ?? throw new KeyNotFoundException($"Location {request.ZoneLocationId} was not found");
        ValidationHelpers.EnsureSameSite(siteId, zone.SiteId, nameof(InventoryLocation), zone.Id);

        if (!string.IsNullOrWhiteSpace(request.ValveChannelCode))
        {
            await EnsureChannelExistsAsync(equipmentId, request.ValveChannelCode!, cancellationToken).ConfigureAwait(false);
        }

        await WarnIfInterlockSharedAsync(siteId, request.InterlockGroup, cancellationToken).ConfigureAwait(false);

        var mapping = new ValveZoneMapping(
            siteId,
            equipmentId,
            request.ZoneLocationId,
            request.RequestedByUserId,
            request.ValveChannelCode,
            request.Priority,
            request.NormallyOpen,
            request.InterlockGroup,
            request.Notes);

        await _mappingRepository.InsertAsync(mapping, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Valve-zone mapping created. Mapping={MappingId} Equipment={EquipmentId} Zone={ZoneId} Site={SiteId}",
            mapping.Id,
            mapping.ValveEquipmentId,
            mapping.ZoneLocationId,
            mapping.SiteId);

        return ValveZoneMappingMapper.ToResponse(mapping);
    }

    public async Task<IReadOnlyList<ValveZoneMappingResponse>> GetByValveAsync(Guid siteId, Guid equipmentId, CancellationToken cancellationToken = default)
    {
        ValidationHelpers.EnsureNotEmpty(siteId, nameof(siteId));
        ValidationHelpers.EnsureNotEmpty(equipmentId, nameof(equipmentId));

        var equipment = await LoadEquipmentAsync(equipmentId, cancellationToken).ConfigureAwait(false)
                        ?? throw new KeyNotFoundException($"Equipment {equipmentId} was not found");
        ValidationHelpers.EnsureSameSite(siteId, equipment.SiteId, nameof(Equipment), equipment.Id);

        var mappings = await _mappingRepository.GetByValveAsync(equipmentId, cancellationToken).ConfigureAwait(false);
        return mappings.Select(ValveZoneMappingMapper.ToResponse).ToArray();
    }

    public async Task<IReadOnlyList<ValveZoneMappingResponse>> GetByZoneAsync(Guid siteId, Guid locationId, CancellationToken cancellationToken = default)
    {
        ValidationHelpers.EnsureNotEmpty(siteId, nameof(siteId));
        ValidationHelpers.EnsureNotEmpty(locationId, nameof(locationId));

        var location = await _locationRepository.GetByIdAsync(locationId, cancellationToken).ConfigureAwait(false)
                       ?? throw new KeyNotFoundException($"Location {locationId} was not found");
        ValidationHelpers.EnsureSameSite(siteId, location.SiteId, nameof(InventoryLocation), location.Id);

        var mappings = await _mappingRepository.GetByZoneAsync(locationId, cancellationToken).ConfigureAwait(false);
        return mappings.Select(ValveZoneMappingMapper.ToResponse).ToArray();
    }

    public async Task<ValveZoneMappingResponse> UpdateAsync(Guid siteId, Guid mappingId, UpdateValveZoneMappingRequest request, CancellationToken cancellationToken = default)
    {
        ValidationHelpers.EnsureNotEmpty(siteId, nameof(siteId));
        ValidationHelpers.EnsureNotEmpty(mappingId, nameof(mappingId));
        if (request == null) throw new ArgumentNullException(nameof(request));
        ValidationHelpers.EnsureNotEmpty(request.RequestedByUserId, nameof(request.RequestedByUserId));

        var mapping = await _mappingRepository.GetByIdAsync(mappingId, cancellationToken).ConfigureAwait(false)
                       ?? throw new KeyNotFoundException($"Valve mapping {mappingId} was not found");
        ValidationHelpers.EnsureSameSite(siteId, mapping.SiteId, nameof(ValveZoneMapping), mapping.Id);

        if (request.ZoneLocationId.HasValue && request.ZoneLocationId.Value != mapping.ZoneLocationId)
        {
            var zone = await _locationRepository.GetByIdAsync(request.ZoneLocationId.Value, cancellationToken).ConfigureAwait(false)
                       ?? throw new KeyNotFoundException($"Location {request.ZoneLocationId} was not found");
            ValidationHelpers.EnsureSameSite(siteId, zone.SiteId, nameof(InventoryLocation), zone.Id);
            mapping.ReassignZone(zone.Id, request.RequestedByUserId);
        }

        if (request.ValveChannelCode != mapping.ValveChannelCode)
        {
            if (!string.IsNullOrWhiteSpace(request.ValveChannelCode))
            {
                await EnsureChannelExistsAsync(mapping.ValveEquipmentId, request.ValveChannelCode!, cancellationToken).ConfigureAwait(false);
            }

            mapping.SetChannel(request.ValveChannelCode, request.RequestedByUserId);
        }

        await WarnIfInterlockSharedAsync(siteId, request.InterlockGroup, cancellationToken).ConfigureAwait(false);

        mapping.UpdateRouting(request.Priority, request.NormallyOpen, request.InterlockGroup, request.Notes, request.RequestedByUserId);
        mapping.SetEnabled(request.Enabled, request.RequestedByUserId);

        await _mappingRepository.UpdateAsync(mapping, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Valve-zone mapping updated. Mapping={MappingId} Enabled={Enabled} Priority={Priority}",
            mapping.Id,
            mapping.Enabled,
            mapping.Priority);

        return ValveZoneMappingMapper.ToResponse(mapping);
    }

    public async Task DeleteAsync(Guid siteId, Guid mappingId, Guid requestedByUserId, CancellationToken cancellationToken = default)
    {
        ValidationHelpers.EnsureNotEmpty(siteId, nameof(siteId));
        ValidationHelpers.EnsureNotEmpty(mappingId, nameof(mappingId));
        ValidationHelpers.EnsureNotEmpty(requestedByUserId, nameof(requestedByUserId));

        var mapping = await _mappingRepository.GetByIdAsync(mappingId, cancellationToken).ConfigureAwait(false)
                       ?? throw new KeyNotFoundException($"Valve mapping {mappingId} was not found");
        ValidationHelpers.EnsureSameSite(siteId, mapping.SiteId, nameof(ValveZoneMapping), mapping.Id);

        await _mappingRepository.DeleteAsync(mappingId, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Valve-zone mapping deleted. Mapping={MappingId} RequestedBy={UserId}", mappingId, requestedByUserId);
    }

    private async Task WarnIfInterlockSharedAsync(Guid siteId, string? interlockGroup, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(interlockGroup))
        {
            return;
        }

        var exists = await _mappingRepository.AnyWithInterlockAsync(siteId, interlockGroup.Trim(), cancellationToken).ConfigureAwait(false);
        if (exists)
        {
            _logger.LogWarning(
                "Valve interlock group {InterlockGroup} already has active mappings in site {SiteId}. Review safety constraints.",
                interlockGroup,
                siteId);
        }
    }

    private async Task EnsureChannelExistsAsync(Guid equipmentId, string channelCode, CancellationToken cancellationToken)
    {
        var channels = await _channelRepository.GetByEquipmentIdAsync(equipmentId, cancellationToken).ConfigureAwait(false);
        if (!channels.Any(c => string.Equals(c.ChannelCode, channelCode, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException($"Channel '{channelCode}' does not exist on equipment {equipmentId}.", nameof(channelCode));
        }
    }

    private async Task<Equipment?> LoadEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken)
    {
        return await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken).ConfigureAwait(false);
    }

}

