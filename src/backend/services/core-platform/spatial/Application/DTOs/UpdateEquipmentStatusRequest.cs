using Harvestry.Spatial.Domain.Enums;

namespace Harvestry.Spatial.Application.DTOs;

public sealed record UpdateEquipmentStatusRequest
{
    public EquipmentStatus Status { get; init; }
}

