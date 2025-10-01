using System;
using Harvestry.Spatial.Application.ViewModels;
using Harvestry.Spatial.Domain.Entities;

namespace Harvestry.Spatial.Application.Mappers;

public static class ValveZoneMappingMapper
{
    public static ValveZoneMappingResponse ToResponse(ValveZoneMapping mapping)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));

        return new ValveZoneMappingResponse
        {
            Id = mapping.Id,
            SiteId = mapping.SiteId,
            ValveEquipmentId = mapping.ValveEquipmentId,
            ValveChannelCode = mapping.ValveChannelCode,
            ZoneLocationId = mapping.ZoneLocationId,
            Priority = mapping.Priority,
            NormallyOpen = mapping.NormallyOpen,
            InterlockGroup = mapping.InterlockGroup,
            Enabled = mapping.Enabled,
            Notes = mapping.Notes,
            CreatedAt = mapping.CreatedAt,
            CreatedByUserId = mapping.CreatedByUserId,
            UpdatedAt = mapping.UpdatedAt,
            UpdatedByUserId = mapping.UpdatedByUserId
        };
    }
}

