using Harvestry.Harvests.Application.DTOs;
using Harvestry.Harvests.Domain.Enums;

namespace Harvestry.Harvests.Application.Interfaces;

/// <summary>
/// Service for managing the harvest workflow from wet weight through lot creation
/// </summary>
public interface IHarvestWorkflowService
{
    // ===== QUERY OPERATIONS =====

    /// <summary>
    /// Get harvest workflow state including plants, metrics, and adjustments
    /// </summary>
    Task<HarvestWorkflowResponse> GetHarvestWorkflowAsync(
        Guid harvestId,
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get weight adjustments for a harvest
    /// </summary>
    Task<IReadOnlyList<WeightAdjustmentResponse>> GetWeightAdjustmentsAsync(
        Guid harvestId,
        Guid siteId,
        CancellationToken cancellationToken = default);

    // ===== WET WEIGHT OPERATIONS =====

    /// <summary>
    /// Record wet weight for a specific plant (manual entry)
    /// </summary>
    Task<HarvestPlantResponse> RecordPlantWetWeightAsync(
        Guid harvestId,
        RecordPlantWetWeightRequest request,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Record wet weight for a plant from scale reading
    /// </summary>
    Task<HarvestPlantResponse> RecordPlantWetWeightFromScaleAsync(
        Guid harvestId,
        RecordPlantWetWeightFromScaleRequest request,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lock wet weight for a specific plant
    /// </summary>
    Task LockPlantWeightAsync(
        Guid harvestId,
        Guid harvestPlantId,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lock all wet weights for the harvest
    /// </summary>
    Task LockHarvestWetWeightAsync(
        Guid harvestId,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default);

    // ===== DRYING OPERATIONS =====

    /// <summary>
    /// Start the drying phase
    /// </summary>
    Task<HarvestWorkflowResponse> StartDryingAsync(
        Guid harvestId,
        StartDryingRequest request,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Complete the drying phase
    /// </summary>
    Task<HarvestWorkflowResponse> CompleteDryingAsync(
        Guid harvestId,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default);

    // ===== BUCKING OPERATIONS =====

    /// <summary>
    /// Record bucking results (flower weight and waste breakdown)
    /// </summary>
    Task<HarvestWorkflowResponse> RecordBuckingResultsAsync(
        Guid harvestId,
        RecordBuckingResultsRequest request,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lock dry weight for the harvest
    /// </summary>
    Task LockHarvestDryWeightAsync(
        Guid harvestId,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default);

    // ===== WEIGHT ADJUSTMENT =====

    /// <summary>
    /// Adjust a weight with PIN override (for locked weights)
    /// </summary>
    Task<WeightAdjustmentResponse> AdjustWeightAsync(
        Guid harvestId,
        AdjustWeightRequest request,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate PIN for weight adjustment
    /// </summary>
    Task<bool> ValidatePinAsync(
        string pin,
        Guid userId,
        CancellationToken cancellationToken = default);

    // ===== BATCHING OPERATIONS =====

    /// <summary>
    /// Set the batching mode for the harvest
    /// </summary>
    Task<HarvestWorkflowResponse> SetBatchingModeAsync(
        Guid harvestId,
        SetBatchingModeRequest request,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark harvest as having lots created
    /// </summary>
    Task<HarvestWorkflowResponse> MarkLotsCreatedAsync(
        Guid harvestId,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Complete the harvest workflow
    /// </summary>
    Task<HarvestWorkflowResponse> CompleteWorkflowAsync(
        Guid harvestId,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default);

    // ===== METRICS =====

    /// <summary>
    /// Recalculate metrics for a harvest
    /// </summary>
    Task<HarvestMetricsDto> RecalculateMetricsAsync(
        Guid harvestId,
        Guid siteId,
        CancellationToken cancellationToken = default);
}




