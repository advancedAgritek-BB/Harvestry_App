using System;

namespace Harvestry.Spatial.Application.DTOs;

public sealed record UpdateRoomRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int? FloorLevel { get; init; }
    public decimal? AreaSqft { get; init; }
    public decimal? HeightFt { get; init; }
    public decimal? TargetTempF { get; init; }
    public decimal? TargetHumidityPct { get; init; }
    public int? TargetCo2Ppm { get; init; }
    public Guid RequestedByUserId { get; init; }
}
