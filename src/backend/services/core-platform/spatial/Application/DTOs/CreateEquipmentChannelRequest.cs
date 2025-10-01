using System;

namespace Harvestry.Spatial.Application.DTOs;

public sealed record CreateEquipmentChannelRequest
{
    public string ChannelCode { get; init; } = string.Empty;
    public string? Role { get; init; }
    public string? PortMetaJson { get; init; }
    public Guid? AssignedZoneId { get; init; }
}
