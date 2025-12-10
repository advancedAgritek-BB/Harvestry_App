using Harvestry.Harvests.Application.DTOs;

namespace Harvestry.Harvests.Application.Interfaces;

/// <summary>
/// Service for managing scale devices and calibrations
/// </summary>
public interface IScaleDeviceService
{
    // ===== SCALE DEVICE CRUD =====

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
    /// Get scale devices by location
    /// </summary>
    Task<IReadOnlyList<ScaleDeviceResponse>> GetScaleDevicesByLocationAsync(
        Guid locationId,
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update scale device
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

    // ===== CALIBRATION =====

    /// <summary>
    /// Record a new calibration for a scale
    /// </summary>
    Task<ScaleCalibrationResponse> RecordCalibrationAsync(
        Guid scaleDeviceId,
        RecordScaleCalibrationRequest request,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current calibration for a scale
    /// </summary>
    Task<ScaleCalibrationResponse?> GetCurrentCalibrationAsync(
        Guid scaleDeviceId,
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get calibration history for a scale
    /// </summary>
    Task<IReadOnlyList<ScaleCalibrationResponse>> GetCalibrationHistoryAsync(
        Guid scaleDeviceId,
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a scale's calibration is valid
    /// </summary>
    Task<bool> IsCalibrationValidAsync(
        Guid scaleDeviceId,
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get scales with expiring calibrations
    /// </summary>
    Task<IReadOnlyList<ScaleDeviceResponse>> GetScalesWithExpiringCalibrationsAsync(
        int daysUntilExpiration,
        Guid siteId,
        CancellationToken cancellationToken = default);

    // ===== SCALE READINGS =====

    /// <summary>
    /// Create a scale reading record
    /// </summary>
    Task<ScaleReadingResponse> CreateScaleReadingAsync(
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
    /// Get scale reading by ID
    /// </summary>
    Task<ScaleReadingResponse?> GetScaleReadingAsync(
        Guid scaleReadingId,
        Guid siteId,
        CancellationToken cancellationToken = default);
}




