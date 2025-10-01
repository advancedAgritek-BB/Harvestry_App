using System;
using Harvestry.Spatial.Domain.Enums;

namespace Harvestry.Spatial.Application.DTOs;

public sealed record CreateEquipmentRequest
{
    public Guid SiteId { get; init; }
    public Guid LocationId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string TypeCode { get; init; } = string.Empty;
    public CoreEquipmentType CoreType { get; init; }
    public string? Manufacturer { get; init; }
    public string? Model { get; init; }
    public string? SerialNumber { get; init; }
    public string? FirmwareVersion { get; init; }
    public string? Notes { get; init; }
    public string? MetadataJson { get; init; }
    public Guid RequestedByUserId { get; init; }
}
