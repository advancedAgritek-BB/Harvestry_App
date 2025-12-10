using Harvestry.Harvests.Domain.Enums;

namespace Harvestry.Harvests.Application.DTOs;

// ===== RESPONSE DTOs =====

/// <summary>
/// Harvest workflow state response
/// </summary>
public record HarvestWorkflowResponse
{
    public Guid Id { get; init; }
    public string HarvestName { get; init; } = string.Empty;
    public string StrainName { get; init; } = string.Empty;
    public HarvestPhase Phase { get; init; }
    
    // Weights
    public decimal TotalWetWeight { get; init; }
    public decimal TotalDryWeight { get; init; }
    public decimal CurrentWeight { get; init; }
    public decimal BuckedFlowerWeight { get; init; }
    public decimal TotalWasteWeight { get; init; }
    public string UnitOfWeight { get; init; } = "Grams";
    
    // Lock status
    public bool WetWeightLocked { get; init; }
    public bool DryWeightLocked { get; init; }
    
    // Drying
    public DateOnly? DryingStartDate { get; init; }
    public DateOnly? DryingEndDate { get; init; }
    public int? DryingDurationDays { get; init; }
    public string? DryingLocationName { get; init; }
    
    // Metrics
    public HarvestMetricsDto Metrics { get; init; } = new();
    
    // Batching
    public HarvestBatchingMode? BatchingMode { get; init; }
    public Guid? ParentHarvestId { get; init; }
    public List<Guid> ChildHarvestIds { get; init; } = new();
    
    // Plants
    public List<HarvestPlantResponse> Plants { get; init; } = new();
    public int TotalPlants => Plants.Count;
    public int PlantsWeighed => Plants.Count(p => p.WetWeight > 0);
}

/// <summary>
/// Harvest plant response
/// </summary>
public record HarvestPlantResponse
{
    public Guid Id { get; init; }
    public Guid PlantId { get; init; }
    public string PlantTag { get; init; } = string.Empty;
    public decimal WetWeight { get; init; }
    public string UnitOfWeight { get; init; } = "Grams";
    public DateTime HarvestedAt { get; init; }
    public Guid? ScaleReadingId { get; init; }
    public string WeightSource { get; init; } = "manual";
    public bool IsWeightLocked { get; init; }
    public DateTime? WeightLockedAt { get; init; }
}

/// <summary>
/// Harvest metrics DTO
/// </summary>
public record HarvestMetricsDto
{
    public decimal WetWeight { get; init; }
    public decimal DryWeight { get; init; }
    public decimal BuckedFlowerWeight { get; init; }
    public decimal TotalWasteWeight { get; init; }
    public decimal StemWaste { get; init; }
    public decimal LeafWaste { get; init; }
    public decimal OtherWaste { get; init; }
    public decimal? MoistureLossPercent { get; init; }
    public decimal? DryToWetRatio { get; init; }
    public decimal? UsableFlowerPercent { get; init; }
    public decimal? WastePercent { get; init; }
    public decimal? YieldPerPlant { get; init; }
}

/// <summary>
/// Weight adjustment response
/// </summary>
public record WeightAdjustmentResponse
{
    public Guid Id { get; init; }
    public Guid HarvestId { get; init; }
    public Guid? HarvestPlantId { get; init; }
    public WeightType WeightType { get; init; }
    public decimal PreviousWeight { get; init; }
    public decimal NewWeight { get; init; }
    public decimal AdjustmentAmount { get; init; }
    public string ReasonCode { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public Guid AdjustedByUserId { get; init; }
    public bool PinOverrideUsed { get; init; }
    public DateTime AdjustedAt { get; init; }
}

/// <summary>
/// Scale device response
/// </summary>
public record ScaleDeviceResponse
{
    public Guid Id { get; init; }
    public string DeviceName { get; init; } = string.Empty;
    public string? DeviceSerialNumber { get; init; }
    public string? Manufacturer { get; init; }
    public string? Model { get; init; }
    public decimal? CapacityGrams { get; init; }
    public decimal? ReadabilityGrams { get; init; }
    public string ConnectionType { get; init; } = "usb";
    public string? LocationName { get; init; }
    public bool IsActive { get; init; }
    public bool IsCalibrationValid { get; init; }
    public ScaleCalibrationResponse? CurrentCalibration { get; init; }
}

/// <summary>
/// Scale calibration response
/// </summary>
public record ScaleCalibrationResponse
{
    public Guid Id { get; init; }
    public DateOnly CalibrationDate { get; init; }
    public DateOnly CalibrationDueDate { get; init; }
    public string CalibrationType { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public decimal? DeviationGrams { get; init; }
    public decimal? DeviationPercent { get; init; }
    public string? PerformedBy { get; init; }
    public string? CertificationNumber { get; init; }
    public bool IsValid { get; init; }
}

/// <summary>
/// Scale reading response
/// </summary>
public record ScaleReadingResponse
{
    public Guid Id { get; init; }
    public Guid? ScaleDeviceId { get; init; }
    public decimal GrossWeight { get; init; }
    public decimal TareWeight { get; init; }
    public decimal NetWeight { get; init; }
    public string UnitOfWeight { get; init; } = "Grams";
    public bool IsStable { get; init; }
    public int? StabilityDurationMs { get; init; }
    public DateTime ReadingTimestamp { get; init; }
    public bool CalibrationWasValid { get; init; }
}

// ===== REQUEST DTOs =====

/// <summary>
/// Record wet weight for a specific plant
/// </summary>
public record RecordPlantWetWeightRequest
{
    public Guid PlantId { get; init; }
    public string PlantTag { get; init; } = string.Empty;
    public decimal WetWeight { get; init; }
    public string UnitOfWeight { get; init; } = "Grams";
    public Guid? ScaleDeviceId { get; init; }
    public Guid? ScaleReadingId { get; init; }
}

/// <summary>
/// Record wet weight from scale reading
/// </summary>
public record RecordPlantWetWeightFromScaleRequest
{
    public Guid PlantId { get; init; }
    public string PlantTag { get; init; } = string.Empty;
    public Guid ScaleDeviceId { get; init; }
    public decimal GrossWeight { get; init; }
    public decimal TareWeight { get; init; }
    public decimal NetWeight { get; init; }
    public string UnitOfWeight { get; init; } = "Grams";
    public bool IsStable { get; init; }
    public int? StabilityDurationMs { get; init; }
    public string? RawScaleDataJson { get; init; }
}

/// <summary>
/// Start drying phase request
/// </summary>
public record StartDryingRequest
{
    public Guid? DryingLocationId { get; init; }
    public string? DryingLocationName { get; init; }
}

/// <summary>
/// Record bucking results request
/// </summary>
public record RecordBuckingResultsRequest
{
    public decimal BuckedFlowerWeight { get; init; }
    public decimal StemWaste { get; init; }
    public decimal LeafWaste { get; init; }
    public decimal OtherWaste { get; init; }
    public string UnitOfWeight { get; init; } = "Grams";
}

/// <summary>
/// Lock weight request
/// </summary>
public record LockWeightRequest
{
    public WeightType WeightType { get; init; }
    public Guid? HarvestPlantId { get; init; }
}

/// <summary>
/// Adjust weight with PIN override request
/// </summary>
public record AdjustWeightRequest
{
    public WeightType WeightType { get; init; }
    public Guid? HarvestPlantId { get; init; }
    public decimal NewWeight { get; init; }
    public string ReasonCode { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public string Pin { get; init; } = string.Empty;
}

/// <summary>
/// Set batching mode request
/// </summary>
public record SetBatchingModeRequest
{
    public HarvestBatchingMode BatchingMode { get; init; }
    public Guid? ParentHarvestId { get; init; }
}

/// <summary>
/// Create scale device request
/// </summary>
public record CreateScaleDeviceRequest
{
    public string DeviceName { get; init; } = string.Empty;
    public string? DeviceSerialNumber { get; init; }
    public string? Manufacturer { get; init; }
    public string? Model { get; init; }
    public decimal? CapacityGrams { get; init; }
    public decimal? ReadabilityGrams { get; init; }
    public string ConnectionType { get; init; } = "usb";
    public string? ConnectionConfigJson { get; init; }
    public Guid? LocationId { get; init; }
    public string? LocationName { get; init; }
    public bool RequiresCalibration { get; init; } = true;
    public int CalibrationIntervalDays { get; init; } = 365;
}

/// <summary>
/// Record scale calibration request
/// </summary>
public record RecordScaleCalibrationRequest
{
    public DateOnly CalibrationDate { get; init; }
    public string CalibrationType { get; init; } = string.Empty;
    public string? PerformedBy { get; init; }
    public string? CertifiedBy { get; init; }
    public string? CertificationNumber { get; init; }
    public string? CalibrationCompany { get; init; }
    public string? TestWeightsUsedJson { get; init; }
    public bool Passed { get; init; }
    public decimal? DeviationGrams { get; init; }
    public decimal? DeviationPercent { get; init; }
    public string? Notes { get; init; }
    public string? CertificateUrl { get; init; }
}




