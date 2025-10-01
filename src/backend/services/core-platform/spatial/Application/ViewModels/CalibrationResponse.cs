using System;
using Harvestry.Spatial.Domain.Enums;

namespace Harvestry.Spatial.Application.ViewModels;

public sealed record CalibrationResponse
{
    public Guid Id { get; init; }
    public Guid EquipmentId { get; init; }
    public string? ChannelCode { get; init; }
    public CalibrationMethod Method { get; init; }
    public decimal ReferenceValue { get; init; }
    public decimal MeasuredValue { get; init; }
    public CalibrationResult Result { get; init; }
    public decimal Deviation { get; init; }
    public decimal DeviationPct { get; init; }
    public DateTime PerformedAt { get; init; }
    public Guid PerformedByUserId { get; init; }
    public DateTime? NextDueAt { get; init; }
    public int? IntervalDays { get; init; }
    public string? Notes { get; init; }
    public string? AttachmentUrl { get; init; }
    public string? FirmwareVersionAtCalibration { get; init; }
    public bool IsOverdue { get; init; }
    public bool IsCritical { get; init; }
}
