using Harvestry.Harvests.Domain.Entities;
using Harvestry.Harvests.Domain.Enums;

namespace Harvestry.Harvests.Application.Interfaces;

/// <summary>
/// Repository for harvest persistence operations
/// </summary>
public interface IHarvestRepository
{
    /// <summary>
    /// Get harvest by ID with all related entities
    /// </summary>
    Task<Harvest?> GetByIdAsync(
        Guid harvestId,
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get harvest by ID with plants loaded
    /// </summary>
    Task<Harvest?> GetByIdWithPlantsAsync(
        Guid harvestId,
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get harvests by phase
    /// </summary>
    Task<IReadOnlyList<Harvest>> GetByPhaseAsync(
        HarvestPhase phase,
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active harvests
    /// </summary>
    Task<IReadOnlyList<Harvest>> GetActiveAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new harvest
    /// </summary>
    Task<Harvest> CreateAsync(
        Harvest harvest,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing harvest
    /// </summary>
    Task<Harvest> UpdateAsync(
        Harvest harvest,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if harvest exists
    /// </summary>
    Task<bool> ExistsAsync(
        Guid harvestId,
        Guid siteId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for harvest plant persistence operations
/// </summary>
public interface IHarvestPlantRepository
{
    /// <summary>
    /// Get harvest plant by ID
    /// </summary>
    Task<HarvestPlant?> GetByIdAsync(
        Guid harvestPlantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all plants for a harvest
    /// </summary>
    Task<IReadOnlyList<HarvestPlant>> GetByHarvestIdAsync(
        Guid harvestId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new harvest plant record
    /// </summary>
    Task<HarvestPlant> CreateAsync(
        HarvestPlant harvestPlant,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a harvest plant record
    /// </summary>
    Task<HarvestPlant> UpdateAsync(
        HarvestPlant harvestPlant,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for weight adjustment persistence operations
/// </summary>
public interface IWeightAdjustmentRepository
{
    /// <summary>
    /// Get adjustments for a harvest
    /// </summary>
    Task<IReadOnlyList<WeightAdjustment>> GetByHarvestIdAsync(
        Guid harvestId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get adjustments for a specific plant
    /// </summary>
    Task<IReadOnlyList<WeightAdjustment>> GetByHarvestPlantIdAsync(
        Guid harvestPlantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new weight adjustment record
    /// </summary>
    Task<WeightAdjustment> CreateAsync(
        WeightAdjustment adjustment,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for scale device persistence operations
/// </summary>
public interface IScaleDeviceRepository
{
    /// <summary>
    /// Get scale device by ID
    /// </summary>
    Task<ScaleDevice?> GetByIdAsync(
        Guid scaleDeviceId,
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active scale devices for a site
    /// </summary>
    Task<IReadOnlyList<ScaleDevice>> GetActiveAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get scale devices by location
    /// </summary>
    Task<IReadOnlyList<ScaleDevice>> GetByLocationAsync(
        Guid locationId,
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new scale device
    /// </summary>
    Task<ScaleDevice> CreateAsync(
        ScaleDevice scaleDevice,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a scale device
    /// </summary>
    Task<ScaleDevice> UpdateAsync(
        ScaleDevice scaleDevice,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for scale calibration persistence operations
/// </summary>
public interface IScaleCalibrationRepository
{
    /// <summary>
    /// Get calibrations for a scale device
    /// </summary>
    Task<IReadOnlyList<ScaleCalibration>> GetByScaleDeviceIdAsync(
        Guid scaleDeviceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the current valid calibration for a scale
    /// </summary>
    Task<ScaleCalibration?> GetCurrentValidAsync(
        Guid scaleDeviceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new calibration record
    /// </summary>
    Task<ScaleCalibration> CreateAsync(
        ScaleCalibration calibration,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for scale reading persistence operations
/// </summary>
public interface IScaleReadingRepository
{
    /// <summary>
    /// Get scale reading by ID
    /// </summary>
    Task<ScaleReading?> GetByIdAsync(
        Guid scaleReadingId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get scale readings for a harvest
    /// </summary>
    Task<IReadOnlyList<ScaleReading>> GetByHarvestIdAsync(
        Guid harvestId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get scale readings for a harvest plant
    /// </summary>
    Task<IReadOnlyList<ScaleReading>> GetByHarvestPlantIdAsync(
        Guid harvestPlantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new scale reading
    /// </summary>
    Task<ScaleReading> CreateAsync(
        ScaleReading scaleReading,
        CancellationToken cancellationToken = default);
}





