using System;

namespace Harvestry.Spatial.Application.DTOs;

public sealed record UpdateValveZoneMappingRequest
{
    public Guid RequestedByUserId { get; init; }
    public int Priority { get; init; } = 1;
    public bool NormallyOpen { get; init; }
    public string? InterlockGroup { get; init; }
    public string? Notes { get; init; }
    public bool Enabled { get; init; } = true;
    public Guid? ZoneLocationId { get; init; }
    public string? ValveChannelCode { get; init; }
}

