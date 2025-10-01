using System;
using Harvestry.Spatial.Domain.Enums;

namespace Harvestry.Spatial.Application.ViewModels;

public sealed record LocationPathSegment
{
    public Guid LocationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public LocationType LocationType { get; init; }
}
