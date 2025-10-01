using System;

namespace Harvestry.Spatial.Application.ViewModels;

public sealed record ValveZoneMappingResponse
{
    public Guid Id { get; init; }
    public Guid SiteId { get; init; }
    public Guid ValveEquipmentId { get; init; }
    public string? ValveChannelCode { get; init; }
    public Guid ZoneLocationId { get; init; }
    public int Priority { get; init; }
    public bool NormallyOpen { get; init; }
    public string? InterlockGroup { get; init; }
    public bool Enabled { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid CreatedByUserId { get; init; }
    public DateTime UpdatedAt { get; init; }
    public Guid UpdatedByUserId { get; init; }
}

