using Harvestry.Harvests.Application.DTOs;

namespace Harvestry.Harvests.Application.Interfaces;

/// <summary>
/// Service for managing scale devices and calibrations
/// </summary>
public interface IScaleService
{
    // ===== SCALE DEVICES =====

    /// <summary>
    /// Create a new scale device
    /// </summary>
    Task<ScaleDeviceResponse> CreateScaleDeviceAsync(
        CreateScaleDeviceRequest request,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a scale device by ID
    /// </summary>
    Task<ScaleDeviceResponse?> GetScaleDeviceAsync(
        Guid scaleDeviceId,
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active scale devices for a site
    /// </summary>
    Task<IReadOnlyList<ScaleDeviceResponse>> GetActiveScaleDevicesAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get scale devices at a specific location
    /// </summary>
    Task<IReadOnlyList<ScaleDeviceResponse>> GetScaleDevicesByLocationAsync(
        Guid locationId,
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update scale device details
    /// </summary>
    Task<ScaleDeviceResponse> UpdateScaleDeviceAsync(
        Guid scaleDeviceId,
        CreateScaleDeviceRequest request,
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivate a scale device
    /// </summary>
    Task DeactivateScaleDeviceAsync(
        Guid scaleDeviceId,
        Guid siteId,
        CancellationToken cancellationToken = default);

    // ===== CALIBRATIONS =====

    /// <summary>
    /// Record a calibration for a scale
    /// </summary>
    Task<ScaleCalibrationResponse> RecordCalibrationAsync(
        Guid scaleDeviceId,
        RecordScaleCalibrationRequest request,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get calibration history for a scale
    /// </summary>
    Task<IReadOnlyList<ScaleCalibrationResponse>> GetCalibrationHistoryAsync(
        Guid scaleDeviceId,
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current valid calibration for a scale
    /// </summary>
    Task<ScaleCalibrationResponse?> GetCurrentCalibrationAsync(
        Guid scaleDeviceId,
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a scale has valid calibration
    /// </summary>
    Task<bool> IsCalibrationValidAsync(
        Guid scaleDeviceId,
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get scales with calibration due within specified days
    /// </summary>
    Task<IReadOnlyList<ScaleDeviceResponse>> GetScalesWithCalibrationDueSoonAsync(
        int withinDays,
        Guid siteId,
        CancellationToken cancellationToken = default);

    // ===== SCALE READINGS =====

    /// <summary>
    /// Record a scale reading
    /// </summary>
    Task<ScaleReadingResponse> RecordScaleReadingAsync(
        Guid scaleDeviceId,
        Guid? harvestId,
        Guid? harvestPlantId,
        Guid? lotId,
        decimal grossWeight,
        decimal tareWeight,
        decimal netWeight,
        string unitOfWeight,
        bool isStable,
        int? stabilityDurationMs,
        string? rawScaleDataJson,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get scale readings for a harvest
    /// </summary>
    Task<IReadOnlyList<ScaleReadingResponse>> GetScaleReadingsForHarvestAsync(
        Guid harvestId,
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get scale readings for a specific plant
    /// </summary>
    Task<IReadOnlyList<ScaleReadingResponse>> GetScaleReadingsForPlantAsync(
        Guid harvestPlantId,
        Guid siteId,
        CancellationToken cancellationToken = default);
}





