using System;
using System.Collections.Generic;
using Harvestry.Spatial.Domain.Enums;

namespace Harvestry.Spatial.Application.ViewModels;

public sealed record InventoryLocationNodeResponse
{
    public Guid Id { get; init; }
    public Guid SiteId { get; init; }
    public Guid? RoomId { get; init; }
    public Guid? ParentId { get; init; }
    public LocationType LocationType { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Barcode { get; init; }
    public string Path { get; init; } = string.Empty;
    public int Depth { get; init; }
    public LocationStatus Status { get; init; }
    public decimal? LengthFt { get; init; }
    public decimal? WidthFt { get; init; }
    public decimal? HeightFt { get; init; }
    public int? PlantCapacity { get; init; }
    public int CurrentPlantCount { get; init; }
    public int? RowNumber { get; init; }
    public int? ColumnNumber { get; init; }
    public decimal? WeightCapacityLbs { get; init; }
    public decimal CurrentWeightLbs { get; init; }
    public string? Notes { get; init; }
    public string? MetadataJson { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid CreatedByUserId { get; init; }
    public DateTime UpdatedAt { get; init; }
    public Guid UpdatedByUserId { get; init; }
    public IReadOnlyList<InventoryLocationNodeResponse> Children { get; init; } = Array.Empty<InventoryLocationNodeResponse>();
}
