using System;
using Harvestry.Spatial.Domain.Enums;

namespace Harvestry.Spatial.Application.DTOs;

public sealed record CreateCalibrationRequest
{
    public Guid SiteId { get; init; }
    public Guid EquipmentId { get; init; }
    public Guid PerformedByUserId { get; init; }
    public CalibrationMethod Method { get; init; }
    public decimal ReferenceValue { get; init; }
    public decimal MeasuredValue { get; init; }
    public CalibrationResult Result { get; init; }
    public string? ChannelCode { get; init; }
    public string? CoefficientsJson { get; init; }
    public string? FirmwareVersionAtCalibration { get; init; }
    public string? Notes { get; init; }
    public string? AttachmentUrl { get; init; }
    public int? IntervalDaysOverride { get; init; }
}
