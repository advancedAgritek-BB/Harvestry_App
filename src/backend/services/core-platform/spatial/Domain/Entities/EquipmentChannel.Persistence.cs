using System;

namespace Harvestry.Spatial.Domain.Entities;

public partial class EquipmentChannel
{
    public static EquipmentChannel FromPersistence(
        Guid id,
        Guid equipmentId,
        string channelCode,
        string? role,
        string? portMetaJson,
        bool enabled,
        Guid? assignedZoneId,
        string? notes,
        DateTime createdAt)
    {
        var channel = new EquipmentChannel(id)
        {
            EquipmentId = equipmentId,
            ChannelCode = channelCode,
            Role = role,
            PortMetaJson = portMetaJson,
            Enabled = enabled,
            AssignedZoneId = assignedZoneId,
            Notes = notes,
            CreatedAt = createdAt
        };

        return channel;
    }
}
