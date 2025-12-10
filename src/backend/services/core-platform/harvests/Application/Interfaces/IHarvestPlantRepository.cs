using Harvestry.Harvests.Domain.Entities;

namespace Harvestry.Harvests.Application.Interfaces;

/// <summary>
/// Repository for harvest plant operations
/// </summary>
public interface IHarvestPlantRepository
{
    /// <summary>
    /// Get harvest plant by ID
    /// </summary>
    Task<HarvestPlant?> GetByIdAsync(Guid harvestPlantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all plants for a harvest
    /// </summary>
    Task<IReadOnlyList<HarvestPlant>> GetByHarvestIdAsync(Guid harvestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get plant by plant ID within a harvest
    /// </summary>
    Task<HarvestPlant?> GetByPlantIdAsync(Guid harvestId, Guid plantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new harvest plant
    /// </summary>
    Task<HarvestPlant> CreateAsync(HarvestPlant harvestPlant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing harvest plant
    /// </summary>
    Task<HarvestPlant> UpdateAsync(HarvestPlant harvestPlant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a harvest plant
    /// </summary>
    Task DeleteAsync(Guid harvestPlantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if plant is already in a harvest
    /// </summary>
    Task<bool> PlantExistsInHarvestAsync(Guid harvestId, Guid plantId, CancellationToken cancellationToken = default);
}




