using System;
using Harvestry.Spatial.Domain.Enums;

namespace Harvestry.Spatial.Application.DTOs;

public sealed record UpdateEquipmentRequest
{
    public string? Manufacturer { get; init; }
    public string? Model { get; init; }
    public string? SerialNumber { get; init; }
    public string? FirmwareVersion { get; init; }
    public string? Notes { get; init; }
    public string? MetadataJson { get; init; }
    public Guid? LocationId { get; init; }
    public Guid RequestedByUserId { get; init; }
}
