using System;
using Harvestry.Spatial.Domain.Enums;

namespace Harvestry.Spatial.Application.DTOs;

public sealed record UpdateLocationRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Barcode { get; init; }
    public string? Notes { get; init; }
    public string? MetadataJson { get; init; }
    public decimal? LengthFt { get; init; }
    public decimal? WidthFt { get; init; }
    public decimal? HeightFt { get; init; }
    public int? PlantCapacity { get; init; }
    public int? RowNumber { get; init; }
    public int? ColumnNumber { get; init; }
    public decimal? WeightCapacityLbs { get; init; }
    public Guid RequestedByUserId { get; init; }
    public LocationStatus? Status { get; init; }
}
