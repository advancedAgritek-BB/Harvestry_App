using System;
using Harvestry.Spatial.Domain.Enums;

namespace Harvestry.Spatial.Application.DTOs;

public sealed record CreateRoomRequest
{
    public Guid SiteId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public RoomType RoomType { get; init; }
    public string? CustomRoomType { get; init; }
    public string? Description { get; init; }
    public int? FloorLevel { get; init; }
    public decimal? AreaSqft { get; init; }
    public decimal? HeightFt { get; init; }
    public decimal? TargetTempF { get; init; }
    public decimal? TargetHumidityPct { get; init; }
    public int? TargetCo2Ppm { get; init; }
    public Guid RequestedByUserId { get; init; }
}
