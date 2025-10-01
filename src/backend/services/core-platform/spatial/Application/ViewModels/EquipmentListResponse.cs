using System.Collections.Generic;

namespace Harvestry.Spatial.Application.ViewModels;

public sealed record EquipmentListResponse
{
    public IReadOnlyList<EquipmentResponse> Items { get; init; } = new List<EquipmentResponse>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}
