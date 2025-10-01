using System;
using Harvestry.Spatial.Domain.Enums;

namespace Harvestry.Spatial.Domain.Entities;

public partial class Calibration
{
    public static Calibration FromPersistence(
        Guid id,
        Guid equipmentId,
        string? channelCode,
        CalibrationMethod method,
        decimal referenceValue,
        decimal measuredValue,
        CalibrationResult result,
        decimal deviation,
        decimal deviationPct,
        DateTime performedAt,
        Guid performedByUserId,
        DateTime? nextDueAt,
        string? notes,
        string? attachmentUrl,
        string? coefficientsJson,
        string? firmwareVersionAtCalibration)
    {
        var calibration = new Calibration(id)
        {
            EquipmentId = equipmentId,
            ChannelCode = channelCode,
            Method = method,
            ReferenceValue = referenceValue,
            MeasuredValue = measuredValue,
            Result = result,
            Deviation = deviation,
            DeviationPct = deviationPct,
            PerformedAt = performedAt,
            PerformedByUserId = performedByUserId,
            NextDueAt = nextDueAt,
            Notes = notes,
            AttachmentUrl = attachmentUrl,
            CoefficientsJson = coefficientsJson,
            FirmwareVersionAtCalibration = firmwareVersionAtCalibration
        };

        return calibration;
    }
}
