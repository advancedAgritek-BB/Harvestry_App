using Harvestry.Harvests.Application.DTOs;
using Harvestry.Harvests.Domain.Entities;

namespace Harvestry.Harvests.Application.Mappers;

/// <summary>
/// Mapper for harvest workflow entities to DTOs
/// </summary>
public static class HarvestWorkflowMapper
{
    /// <summary>
    /// Map Harvest entity to workflow response
    /// </summary>
    public static HarvestWorkflowResponse ToWorkflowResponse(Harvest harvest)
    {
        return new HarvestWorkflowResponse
        {
            Id = harvest.Id,
            HarvestName = harvest.HarvestName,
            StrainName = harvest.StrainName,
            Phase = harvest.Phase,
            TotalWetWeight = harvest.TotalWetWeight,
            TotalDryWeight = harvest.TotalDryWeight,
            CurrentWeight = harvest.CurrentWeight,
            BuckedFlowerWeight = harvest.BuckedFlowerWeight,
            TotalWasteWeight = harvest.TotalWasteWeight,
            UnitOfWeight = harvest.UnitOfWeight,
            WetWeightLocked = harvest.WetWeightLocked,
            DryWeightLocked = harvest.DryWeightLocked,
            DryingStartDate = harvest.DryingStartDate,
            DryingEndDate = harvest.DryingEndDate,
            DryingDurationDays = harvest.DryingDurationDays,
            DryingLocationName = harvest.DryingLocationName,
            BatchingMode = harvest.BatchingMode,
            ParentHarvestId = harvest.ParentHarvestId,
            ChildHarvestIds = harvest.ChildHarvestIds.ToList(),
            Metrics = ToMetricsDto(harvest),
            Plants = harvest.HarvestPlants.Select(ToPlantResponse).ToList()
        };
    }

    /// <summary>
    /// Map Harvest to metrics DTO
    /// </summary>
    public static HarvestMetricsDto ToMetricsDto(Harvest harvest)
    {
        var plantCount = harvest.HarvestPlants.Count;
        var yieldPerPlant = plantCount > 0 && harvest.BuckedFlowerWeight > 0
            ? harvest.BuckedFlowerWeight / plantCount
            : (decimal?)null;

        return new HarvestMetricsDto
        {
            WetWeight = harvest.TotalWetWeight,
            DryWeight = harvest.TotalDryWeight,
            BuckedFlowerWeight = harvest.BuckedFlowerWeight,
            TotalWasteWeight = harvest.TotalWasteWeight,
            StemWaste = harvest.TotalStemWaste,
            LeafWaste = harvest.TotalLeafWaste,
            OtherWaste = harvest.TotalOtherWaste,
            MoistureLossPercent = harvest.MoistureLossPercent,
            DryToWetRatio = harvest.DryToWetRatio,
            UsableFlowerPercent = harvest.UsableFlowerPercent,
            WastePercent = harvest.TotalWastePercent,
            YieldPerPlant = yieldPerPlant
        };
    }

    /// <summary>
    /// Map HarvestPlant entity to response
    /// </summary>
    public static HarvestPlantResponse ToPlantResponse(HarvestPlant plant)
    {
        return new HarvestPlantResponse
        {
            Id = plant.Id,
            PlantId = plant.PlantId,
            PlantTag = plant.PlantTag,
            WetWeight = plant.WetWeight,
            UnitOfWeight = plant.UnitOfWeight,
            HarvestedAt = plant.HarvestedAt,
            ScaleReadingId = plant.ScaleReadingId,
            WeightSource = plant.WeightSource,
            IsWeightLocked = plant.IsWeightLocked,
            WeightLockedAt = plant.WeightLockedAt
        };
    }

    /// <summary>
    /// Map WeightAdjustment entity to response
    /// </summary>
    public static WeightAdjustmentResponse ToAdjustmentResponse(WeightAdjustment adjustment)
    {
        return new WeightAdjustmentResponse
        {
            Id = adjustment.Id,
            HarvestId = adjustment.HarvestId,
            HarvestPlantId = adjustment.HarvestPlantId,
            WeightType = adjustment.WeightType,
            PreviousWeight = adjustment.PreviousWeight,
            NewWeight = adjustment.NewWeight,
            AdjustmentAmount = adjustment.AdjustmentAmount,
            ReasonCode = adjustment.ReasonCode,
            Notes = adjustment.Notes,
            AdjustedByUserId = adjustment.AdjustedByUserId,
            PinOverrideUsed = adjustment.PinOverrideUsed,
            AdjustedAt = adjustment.AdjustedAt
        };
    }

    /// <summary>
    /// Map list of adjustments to responses
    /// </summary>
    public static IReadOnlyList<WeightAdjustmentResponse> ToAdjustmentResponseList(
        IEnumerable<WeightAdjustment> adjustments)
    {
        return adjustments.Select(ToAdjustmentResponse).ToList();
    }

    /// <summary>
    /// Map ScaleDevice entity to response
    /// </summary>
    public static ScaleDeviceResponse ToScaleDeviceResponse(ScaleDevice device)
    {
        var currentCal = device.GetCurrentCalibration();

        return new ScaleDeviceResponse
        {
            Id = device.Id,
            DeviceName = device.DeviceName,
            DeviceSerialNumber = device.DeviceSerialNumber,
            Manufacturer = device.Manufacturer,
            Model = device.Model,
            CapacityGrams = device.CapacityGrams,
            ReadabilityGrams = device.ReadabilityGrams,
            ConnectionType = device.ConnectionType,
            LocationName = device.LocationName,
            IsActive = device.IsActive,
            IsCalibrationValid = device.IsCalibrationValid(),
            CurrentCalibration = currentCal != null ? ToCalibrationResponse(currentCal) : null
        };
    }

    /// <summary>
    /// Map list of scale devices to responses
    /// </summary>
    public static IReadOnlyList<ScaleDeviceResponse> ToScaleDeviceResponseList(
        IEnumerable<ScaleDevice> devices)
    {
        return devices.Select(ToScaleDeviceResponse).ToList();
    }

    /// <summary>
    /// Map ScaleCalibration entity to response
    /// </summary>
    public static ScaleCalibrationResponse ToCalibrationResponse(ScaleCalibration calibration)
    {
        return new ScaleCalibrationResponse
        {
            Id = calibration.Id,
            CalibrationDate = calibration.CalibrationDate,
            CalibrationDueDate = calibration.CalibrationDueDate,
            CalibrationType = calibration.CalibrationType,
            Passed = calibration.Passed,
            DeviationGrams = calibration.DeviationGrams,
            DeviationPercent = calibration.DeviationPercent,
            PerformedBy = calibration.PerformedBy,
            CertificationNumber = calibration.CertificationNumber,
            IsValid = calibration.IsValid()
        };
    }

    /// <summary>
    /// Map list of calibrations to responses
    /// </summary>
    public static IReadOnlyList<ScaleCalibrationResponse> ToCalibrationResponseList(
        IEnumerable<ScaleCalibration> calibrations)
    {
        return calibrations.Select(ToCalibrationResponse).ToList();
    }

    /// <summary>
    /// Map ScaleReading entity to response
    /// </summary>
    public static ScaleReadingResponse ToScaleReadingResponse(ScaleReading reading)
    {
        return new ScaleReadingResponse
        {
            Id = reading.Id,
            ScaleDeviceId = reading.ScaleDeviceId,
            GrossWeight = reading.GrossWeight,
            TareWeight = reading.TareWeight,
            NetWeight = reading.NetWeight,
            UnitOfWeight = reading.UnitOfWeight,
            IsStable = reading.IsStable,
            StabilityDurationMs = reading.StabilityDurationMs,
            ReadingTimestamp = reading.ReadingTimestamp,
            CalibrationWasValid = reading.CalibrationWasValid
        };
    }

    /// <summary>
    /// Map list of scale readings to responses
    /// </summary>
    public static IReadOnlyList<ScaleReadingResponse> ToScaleReadingResponseList(
        IEnumerable<ScaleReading> readings)
    {
        return readings.Select(ToScaleReadingResponse).ToList();
    }
}




