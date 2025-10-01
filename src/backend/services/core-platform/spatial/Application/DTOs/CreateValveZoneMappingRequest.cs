using System;

namespace Harvestry.Spatial.Application.DTOs;

public sealed record CreateValveZoneMappingRequest
{
    public Guid SiteId { get; init; }
    public Guid EquipmentId { get; init; }
    public Guid ZoneLocationId { get; init; }
    public Guid RequestedByUserId { get; init; }
    public string? ValveChannelCode { get; init; }
    public int Priority { get; init; } = 1;
    public bool NormallyOpen { get; init; }
    public string? InterlockGroup { get; init; }
    public string? Notes { get; init; }
}

