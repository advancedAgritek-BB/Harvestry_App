using System;
using Harvestry.Spatial.Domain.Enums;

namespace Harvestry.Spatial.Application.DTOs;

public sealed record EquipmentListQuery
{
    public EquipmentStatus? Status { get; init; }
    public CoreEquipmentType? CoreType { get; init; }
    public Guid? LocationId { get; init; }
    public DateTime? CalibrationDueBefore { get; init; }
    public bool IncludeChannels { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}
