using Harvestry.Harvests.Domain.Entities;

namespace Harvestry.Harvests.Application.Interfaces;

/// <summary>
/// Repository for weight adjustment operations
/// </summary>
public interface IWeightAdjustmentRepository
{
    /// <summary>
    /// Get all adjustments for a harvest
    /// </summary>
    Task<IReadOnlyList<WeightAdjustment>> GetByHarvestIdAsync(Guid harvestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get adjustments for a specific plant
    /// </summary>
    Task<IReadOnlyList<WeightAdjustment>> GetByHarvestPlantIdAsync(Guid harvestPlantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new weight adjustment
    /// </summary>
    Task<WeightAdjustment> CreateAsync(WeightAdjustment adjustment, CancellationToken cancellationToken = default);
}
