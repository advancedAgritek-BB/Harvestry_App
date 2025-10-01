using System;
using Harvestry.Spatial.Application.ViewModels;
using Harvestry.Spatial.Domain.Entities;

namespace Harvestry.Spatial.Application.Mappers;

public static class CalibrationMapper
{
    public static CalibrationResponse ToResponse(Calibration calibration, int? intervalDays, DateTime utcNow)
    {
        if (calibration == null) throw new ArgumentNullException(nameof(calibration));

        var nextDueAt = calibration.NextDueAt;
        var isOverdue = nextDueAt.HasValue && nextDueAt.Value <= utcNow;
        var isCritical = nextDueAt.HasValue && nextDueAt.Value <= utcNow.AddDays(-7);

        return new CalibrationResponse
        {
            Id = calibration.Id,
            EquipmentId = calibration.EquipmentId,
            ChannelCode = calibration.ChannelCode,
            Method = calibration.Method,
            ReferenceValue = calibration.ReferenceValue,
            MeasuredValue = calibration.MeasuredValue,
            Result = calibration.Result,
            Deviation = calibration.Deviation,
            DeviationPct = calibration.DeviationPct,
            PerformedAt = calibration.PerformedAt,
            PerformedByUserId = calibration.PerformedByUserId,
            NextDueAt = nextDueAt,
            IntervalDays = intervalDays,
            Notes = calibration.Notes,
            AttachmentUrl = calibration.AttachmentUrl,
            FirmwareVersionAtCalibration = calibration.FirmwareVersionAtCalibration,
            IsOverdue = isOverdue,
            IsCritical = isCritical
        };
    }
}
