using System;

namespace Harvestry.Spatial.Domain.Entities;

public partial class ValveZoneMapping
{
    public static ValveZoneMapping FromPersistence(
        Guid id,
        Guid siteId,
        Guid valveEquipmentId,
        string? valveChannelCode,
        Guid zoneLocationId,
        int priority,
        bool normallyOpen,
        string? interlockGroup,
        bool enabled,
        string? notes,
        DateTime createdAt,
        Guid createdByUserId,
        DateTime updatedAt,
        Guid updatedByUserId)
    {
        return new ValveZoneMapping(id, siteId, valveEquipmentId, zoneLocationId, createdByUserId,
            valveChannelCode, priority, normallyOpen, interlockGroup, notes)
        {
            Enabled = enabled,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            CreatedByUserId = createdByUserId,
            UpdatedByUserId = updatedByUserId
        };
    }
}

