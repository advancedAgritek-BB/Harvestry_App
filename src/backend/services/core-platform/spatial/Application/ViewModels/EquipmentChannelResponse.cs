using System;

namespace Harvestry.Spatial.Application.ViewModels;

public sealed record EquipmentChannelResponse
{
    public Guid Id { get; init; }
    public Guid EquipmentId { get; init; }
    public string ChannelCode { get; init; } = string.Empty;
    public string? Role { get; init; }
    public string? PortMetaJson { get; init; }
    public bool Enabled { get; init; }
    public Guid? AssignedZoneId { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
}
