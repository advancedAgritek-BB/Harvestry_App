using Harvestry.Harvests.Application.DTOs;
using Harvestry.Harvests.Application.Interfaces;
using Harvestry.Harvests.Application.Mappers;
using Harvestry.Harvests.Domain.Entities;
using Harvestry.Harvests.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Harvestry.Harvests.Application.Services;

/// <summary>
/// Service for managing the harvest workflow from wet weight through lot creation
/// </summary>
public class HarvestWorkflowService : IHarvestWorkflowService
{
    private readonly IHarvestRepository _harvestRepository;
    private readonly IHarvestPlantRepository _harvestPlantRepository;
    private readonly IWeightAdjustmentRepository _weightAdjustmentRepository;
    private readonly IScaleDeviceRepository _scaleDeviceRepository;
    private readonly IScaleCalibrationRepository _scaleCalibrationRepository;
    private readonly IScaleReadingRepository _scaleReadingRepository;
    private readonly ILogger<HarvestWorkflowService> _logger;

    public HarvestWorkflowService(
        IHarvestRepository harvestRepository,
        IHarvestPlantRepository harvestPlantRepository,
        IWeightAdjustmentRepository weightAdjustmentRepository,
        IScaleDeviceRepository scaleDeviceRepository,
        IScaleCalibrationRepository scaleCalibrationRepository,
        IScaleReadingRepository scaleReadingRepository,
        ILogger<HarvestWorkflowService> logger)
    {
        _harvestRepository = harvestRepository;
        _harvestPlantRepository = harvestPlantRepository;
        _weightAdjustmentRepository = weightAdjustmentRepository;
        _scaleDeviceRepository = scaleDeviceRepository;
        _scaleCalibrationRepository = scaleCalibrationRepository;
        _scaleReadingRepository = scaleReadingRepository;
        _logger = logger;
    }

    #region Query Operations

    public async Task<HarvestWorkflowResponse> GetHarvestWorkflowAsync(
        Guid harvestId,
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var harvest = await _harvestRepository.GetByIdWithPlantsAsync(harvestId, siteId, cancellationToken);
        if (harvest == null)
            throw new KeyNotFoundException($"Harvest {harvestId} not found");

        return HarvestWorkflowMapper.ToWorkflowResponse(harvest);
    }

    public async Task<IReadOnlyList<WeightAdjustmentResponse>> GetWeightAdjustmentsAsync(
        Guid harvestId,
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var harvest = await _harvestRepository.GetByIdAsync(harvestId, siteId, cancellationToken);
        if (harvest == null)
            throw new KeyNotFoundException($"Harvest {harvestId} not found");

        var adjustments = await _weightAdjustmentRepository.GetByHarvestIdAsync(harvestId, cancellationToken);
        return HarvestWorkflowMapper.ToAdjustmentResponseList(adjustments);
    }

    #endregion

    #region Wet Weight Operations

    public async Task<HarvestPlantResponse> RecordPlantWetWeightAsync(
        Guid harvestId,
        RecordPlantWetWeightRequest request,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Recording wet weight for plant {PlantTag} in harvest {HarvestId}: {Weight} {Unit}",
            request.PlantTag, harvestId, request.WetWeight, request.UnitOfWeight);

        var harvest = await _harvestRepository.GetByIdWithPlantsAsync(harvestId, siteId, cancellationToken);
        if (harvest == null)
            throw new KeyNotFoundException($"Harvest {harvestId} not found");

        // Add plant to harvest (the entity validates and updates totals)
        harvest.AddPlant(request.PlantId, request.PlantTag, request.WetWeight, userId);

        await _harvestRepository.UpdateAsync(harvest, cancellationToken);

        // Get the created plant
        var harvestPlant = harvest.HarvestPlants.First(p => p.PlantId == request.PlantId);

        _logger.LogInformation(
            "Recorded wet weight for plant {PlantTag}: {Weight}g, Total wet weight now {TotalWet}g",
            request.PlantTag, request.WetWeight, harvest.TotalWetWeight);

        return HarvestWorkflowMapper.ToPlantResponse(harvestPlant);
    }

    public async Task<HarvestPlantResponse> RecordPlantWetWeightFromScaleAsync(
        Guid harvestId,
        RecordPlantWetWeightFromScaleRequest request,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Recording wet weight from scale for plant {PlantTag} in harvest {HarvestId}: {Weight} {Unit}",
            request.PlantTag, harvestId, request.NetWeight, request.UnitOfWeight);

        var harvest = await _harvestRepository.GetByIdWithPlantsAsync(harvestId, siteId, cancellationToken);
        if (harvest == null)
            throw new KeyNotFoundException($"Harvest {harvestId} not found");

        // Get scale device and calibration
        var scaleDevice = await _scaleDeviceRepository.GetByIdAsync(request.ScaleDeviceId, siteId, cancellationToken);
        if (scaleDevice == null)
            throw new KeyNotFoundException($"Scale device {request.ScaleDeviceId} not found");

        var currentCalibration = await _scaleCalibrationRepository.GetCurrentValidAsync(
            request.ScaleDeviceId, cancellationToken);

        // Create scale reading
        var scaleReading = ScaleReading.Create(
            harvestId: harvestId,
            harvestPlantId: null, // Will be set after plant is created
            lotId: null,
            scaleDeviceId: request.ScaleDeviceId,
            grossWeight: request.GrossWeight,
            tareWeight: request.TareWeight,
            netWeight: request.NetWeight,
            unitOfWeight: request.UnitOfWeight,
            isStable: request.IsStable,
            stabilityDurationMs: request.StabilityDurationMs,
            readingTimestamp: DateTime.UtcNow,
            recordedByUserId: userId);

        // Set calibration snapshot
        if (currentCalibration != null)
        {
            scaleReading.SetCalibrationSnapshot(
                currentCalibration.Id,
                currentCalibration.CalibrationDate,
                currentCalibration.CalibrationDueDate,
                currentCalibration.IsValid());
        }

        if (!string.IsNullOrEmpty(request.RawScaleDataJson))
        {
            scaleReading.SetRawScaleData(request.RawScaleDataJson);
        }

        var savedReading = await _scaleReadingRepository.CreateAsync(scaleReading, cancellationToken);

        // Create harvest plant from scale reading
        var harvestPlant = HarvestPlant.CreateFromScaleReading(
            harvestId,
            request.PlantId,
            request.PlantTag,
            request.NetWeight,
            request.UnitOfWeight,
            savedReading.Id);

        var savedPlant = await _harvestPlantRepository.CreateAsync(harvestPlant, cancellationToken);

        // Update harvest totals
        harvest.RecordWetWeight(harvest.TotalWetWeight + request.NetWeight, userId);
        await _harvestRepository.UpdateAsync(harvest, cancellationToken);

        _logger.LogInformation(
            "Recorded wet weight from scale for plant {PlantTag}: {Weight}g (scale reading {ReadingId})",
            request.PlantTag, request.NetWeight, savedReading.Id);

        return HarvestWorkflowMapper.ToPlantResponse(savedPlant);
    }

    public async Task LockPlantWeightAsync(
        Guid harvestId,
        Guid harvestPlantId,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var harvest = await _harvestRepository.GetByIdAsync(harvestId, siteId, cancellationToken);
        if (harvest == null)
            throw new KeyNotFoundException($"Harvest {harvestId} not found");

        var harvestPlant = await _harvestPlantRepository.GetByIdAsync(harvestPlantId, cancellationToken);
        if (harvestPlant == null || harvestPlant.HarvestId != harvestId)
            throw new KeyNotFoundException($"Harvest plant {harvestPlantId} not found in harvest {harvestId}");

        harvestPlant.LockWeight(userId);
        await _harvestPlantRepository.UpdateAsync(harvestPlant, cancellationToken);

        _logger.LogInformation("Locked weight for plant {PlantId} in harvest {HarvestId}", harvestPlantId, harvestId);
    }

    public async Task LockHarvestWetWeightAsync(
        Guid harvestId,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var harvest = await _harvestRepository.GetByIdWithPlantsAsync(harvestId, siteId, cancellationToken);
        if (harvest == null)
            throw new KeyNotFoundException($"Harvest {harvestId} not found");

        // Lock all individual plant weights
        foreach (var plant in harvest.HarvestPlants)
        {
            if (!plant.IsWeightLocked)
            {
                plant.LockWeight(userId);
                await _harvestPlantRepository.UpdateAsync(plant, cancellationToken);
            }
        }

        // Lock harvest total wet weight
        harvest.LockWetWeight(userId);
        await _harvestRepository.UpdateAsync(harvest, cancellationToken);

        _logger.LogInformation("Locked wet weight for harvest {HarvestId}", harvestId);
    }

    #endregion

    #region Drying Operations

    public async Task<HarvestWorkflowResponse> StartDryingAsync(
        Guid harvestId,
        StartDryingRequest request,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting drying phase for harvest {HarvestId}", harvestId);

        var harvest = await _harvestRepository.GetByIdWithPlantsAsync(harvestId, siteId, cancellationToken);
        if (harvest == null)
            throw new KeyNotFoundException($"Harvest {harvestId} not found");

        harvest.StartDrying(request.DryingLocationId, request.DryingLocationName, userId);
        await _harvestRepository.UpdateAsync(harvest, cancellationToken);

        _logger.LogInformation(
            "Started drying phase for harvest {HarvestId} at location {Location}",
            harvestId, request.DryingLocationName ?? "unspecified");

        return HarvestWorkflowMapper.ToWorkflowResponse(harvest);
    }

    public async Task<HarvestWorkflowResponse> CompleteDryingAsync(
        Guid harvestId,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Completing drying phase for harvest {HarvestId}", harvestId);

        var harvest = await _harvestRepository.GetByIdWithPlantsAsync(harvestId, siteId, cancellationToken);
        if (harvest == null)
            throw new KeyNotFoundException($"Harvest {harvestId} not found");

        harvest.CompleteDrying(userId);
        await _harvestRepository.UpdateAsync(harvest, cancellationToken);

        _logger.LogInformation(
            "Completed drying phase for harvest {HarvestId}, duration: {Days} days",
            harvestId, harvest.DryingDurationDays);

        return HarvestWorkflowMapper.ToWorkflowResponse(harvest);
    }

    #endregion

    #region Bucking Operations

    public async Task<HarvestWorkflowResponse> RecordBuckingResultsAsync(
        Guid harvestId,
        RecordBuckingResultsRequest request,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Recording bucking results for harvest {HarvestId}: flower={Flower}g, stem={Stem}g, leaf={Leaf}g, other={Other}g",
            harvestId, request.BuckedFlowerWeight, request.StemWaste, request.LeafWaste, request.OtherWaste);

        var harvest = await _harvestRepository.GetByIdWithPlantsAsync(harvestId, siteId, cancellationToken);
        if (harvest == null)
            throw new KeyNotFoundException($"Harvest {harvestId} not found");

        harvest.RecordBuckingResults(
            request.BuckedFlowerWeight,
            request.StemWaste,
            request.LeafWaste,
            request.OtherWaste,
            userId);

        await _harvestRepository.UpdateAsync(harvest, cancellationToken);

        _logger.LogInformation(
            "Recorded bucking results for harvest {HarvestId}: " +
            "flower={Flower}g, moisture loss={MoistureLoss}%, usable flower={Usable}%",
            harvestId, request.BuckedFlowerWeight,
            harvest.MoistureLossPercent, harvest.UsableFlowerPercent);

        return HarvestWorkflowMapper.ToWorkflowResponse(harvest);
    }

    public async Task LockHarvestDryWeightAsync(
        Guid harvestId,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var harvest = await _harvestRepository.GetByIdAsync(harvestId, siteId, cancellationToken);
        if (harvest == null)
            throw new KeyNotFoundException($"Harvest {harvestId} not found");

        harvest.LockDryWeight(userId);
        await _harvestRepository.UpdateAsync(harvest, cancellationToken);

        _logger.LogInformation("Locked dry weight for harvest {HarvestId}", harvestId);
    }

    #endregion

    #region Weight Adjustment

    public async Task<WeightAdjustmentResponse> AdjustWeightAsync(
        Guid harvestId,
        AdjustWeightRequest request,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Adjusting {WeightType} weight for harvest {HarvestId} to {NewWeight}",
            request.WeightType, harvestId, request.NewWeight);

        // Validate PIN
        var pinValid = await ValidatePinAsync(request.Pin, userId, cancellationToken);
        if (!pinValid)
            throw new UnauthorizedAccessException("Invalid PIN for weight adjustment");

        var harvest = await _harvestRepository.GetByIdWithPlantsAsync(harvestId, siteId, cancellationToken);
        if (harvest == null)
            throw new KeyNotFoundException($"Harvest {harvestId} not found");

        WeightAdjustment adjustment;

        if (request.HarvestPlantId.HasValue)
        {
            // Adjust individual plant weight
            var harvestPlant = await _harvestPlantRepository.GetByIdAsync(request.HarvestPlantId.Value, cancellationToken);
            if (harvestPlant == null || harvestPlant.HarvestId != harvestId)
                throw new KeyNotFoundException($"Harvest plant {request.HarvestPlantId} not found in harvest {harvestId}");

            var previousWeight = harvestPlant.WetWeight;
            harvestPlant.AdjustWeightWithOverride(request.NewWeight, null, "manual");
            await _harvestPlantRepository.UpdateAsync(harvestPlant, cancellationToken);

            // Create adjustment record
            adjustment = WeightAdjustment.CreateForPlant(
                harvestId,
                request.HarvestPlantId.Value,
                request.WeightType,
                previousWeight,
                request.NewWeight,
                request.ReasonCode,
                request.Notes,
                userId,
                pinOverrideUsed: true);

            // Update harvest total
            var totalWetWeight = harvest.HarvestPlants.Sum(p => p.WetWeight);
            harvest.RecordWetWeight(totalWetWeight, userId);
            await _harvestRepository.UpdateAsync(harvest, cancellationToken);
        }
        else
        {
            // Adjust harvest-level weight
            adjustment = harvest.AdjustWeight(
                request.WeightType,
                request.NewWeight,
                request.ReasonCode,
                request.Notes,
                userId,
                pinOverrideUsed: true);

            await _harvestRepository.UpdateAsync(harvest, cancellationToken);
        }

        await _weightAdjustmentRepository.CreateAsync(adjustment, cancellationToken);

        _logger.LogInformation(
            "Adjusted {WeightType} weight for harvest {HarvestId}: {PrevWeight} → {NewWeight} (reason: {Reason})",
            request.WeightType, harvestId, adjustment.PreviousWeight, adjustment.NewWeight, request.ReasonCode);

        return HarvestWorkflowMapper.ToAdjustmentResponse(adjustment);
    }

    public Task<bool> ValidatePinAsync(
        string pin,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement actual PIN validation against user's stored PIN
        // For now, accept any 4+ digit PIN
        var isValid = !string.IsNullOrEmpty(pin) && pin.Length >= 4 && pin.All(char.IsDigit);
        return Task.FromResult(isValid);
    }

    #endregion

    #region Batching Operations

    public async Task<HarvestWorkflowResponse> SetBatchingModeAsync(
        Guid harvestId,
        SetBatchingModeRequest request,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Setting batching mode for harvest {HarvestId} to {Mode}",
            harvestId, request.BatchingMode);

        var harvest = await _harvestRepository.GetByIdWithPlantsAsync(harvestId, siteId, cancellationToken);
        if (harvest == null)
            throw new KeyNotFoundException($"Harvest {harvestId} not found");

        harvest.SetBatchingMode(request.BatchingMode, request.ParentHarvestId, userId);
        await _harvestRepository.UpdateAsync(harvest, cancellationToken);

        return HarvestWorkflowMapper.ToWorkflowResponse(harvest);
    }

    public async Task<HarvestWorkflowResponse> MarkLotsCreatedAsync(
        Guid harvestId,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Marking lots created for harvest {HarvestId}", harvestId);

        var harvest = await _harvestRepository.GetByIdWithPlantsAsync(harvestId, siteId, cancellationToken);
        if (harvest == null)
            throw new KeyNotFoundException($"Harvest {harvestId} not found");

        harvest.MarkLotsCreated(userId);
        await _harvestRepository.UpdateAsync(harvest, cancellationToken);

        return HarvestWorkflowMapper.ToWorkflowResponse(harvest);
    }

    public async Task<HarvestWorkflowResponse> CompleteWorkflowAsync(
        Guid harvestId,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Completing workflow for harvest {HarvestId}", harvestId);

        var harvest = await _harvestRepository.GetByIdWithPlantsAsync(harvestId, siteId, cancellationToken);
        if (harvest == null)
            throw new KeyNotFoundException($"Harvest {harvestId} not found");

        harvest.CompleteWorkflow(userId);
        await _harvestRepository.UpdateAsync(harvest, cancellationToken);

        return HarvestWorkflowMapper.ToWorkflowResponse(harvest);
    }

    #endregion

    #region Metrics

    public async Task<HarvestMetricsDto> RecalculateMetricsAsync(
        Guid harvestId,
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var harvest = await _harvestRepository.GetByIdWithPlantsAsync(harvestId, siteId, cancellationToken);
        if (harvest == null)
            throw new KeyNotFoundException($"Harvest {harvestId} not found");

        harvest.CalculateMetrics();
        await _harvestRepository.UpdateAsync(harvest, cancellationToken);

        return HarvestWorkflowMapper.ToMetricsDto(harvest);
    }

    #endregion
}




