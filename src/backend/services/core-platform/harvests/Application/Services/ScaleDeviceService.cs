using Harvestry.Harvests.Application.DTOs;
using Harvestry.Harvests.Application.Interfaces;
using Harvestry.Harvests.Application.Mappers;
using Harvestry.Harvests.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Harvestry.Harvests.Application.Services;

/// <summary>
/// Service for managing scale devices and calibrations
/// </summary>
public class ScaleDeviceService : IScaleDeviceService
{
    private readonly IScaleDeviceRepository _scaleDeviceRepository;
    private readonly IScaleCalibrationRepository _scaleCalibrationRepository;
    private readonly IScaleReadingRepository _scaleReadingRepository;
    private readonly ILogger<ScaleDeviceService> _logger;

    public ScaleDeviceService(
        IScaleDeviceRepository scaleDeviceRepository,
        IScaleCalibrationRepository scaleCalibrationRepository,
        IScaleReadingRepository scaleReadingRepository,
        ILogger<ScaleDeviceService> logger)
    {
        _scaleDeviceRepository = scaleDeviceRepository;
        _scaleCalibrationRepository = scaleCalibrationRepository;
        _scaleReadingRepository = scaleReadingRepository;
        _logger = logger;
    }

    #region Scale Device CRUD

    public async Task<ScaleDeviceResponse> CreateScaleDeviceAsync(
        CreateScaleDeviceRequest request,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Creating scale device {DeviceName} for site {SiteId}",
            request.DeviceName, siteId);

        var scaleDevice = ScaleDevice.Create(
            siteId,
            request.DeviceName,
            request.DeviceSerialNumber,
            request.Manufacturer,
            request.Model,
            request.CapacityGrams,
            request.ReadabilityGrams,
            request.ConnectionType,
            userId);

        if (!string.IsNullOrEmpty(request.ConnectionConfigJson))
        {
            scaleDevice.UpdateConnectionConfig(request.ConnectionConfigJson);
        }

        if (request.LocationId.HasValue || !string.IsNullOrEmpty(request.LocationName))
        {
            scaleDevice.UpdateLocation(request.LocationId, request.LocationName);
        }

        scaleDevice.UpdateCalibrationSettings(request.RequiresCalibration, request.CalibrationIntervalDays);

        var savedDevice = await _scaleDeviceRepository.CreateAsync(scaleDevice, cancellationToken);

        _logger.LogInformation("Created scale device {DeviceId} ({DeviceName})", savedDevice.Id, savedDevice.DeviceName);

        return HarvestWorkflowMapper.ToScaleDeviceResponse(savedDevice);
    }

    public async Task<ScaleDeviceResponse?> GetScaleDeviceAsync(
        Guid scaleDeviceId,
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var device = await _scaleDeviceRepository.GetByIdAsync(scaleDeviceId, siteId, cancellationToken);
        if (device == null)
            return null;

        // Load calibrations
        var calibrations = await _scaleCalibrationRepository.GetByScaleDeviceIdAsync(scaleDeviceId, cancellationToken);
        device.SetCalibrations(calibrations);

        return HarvestWorkflowMapper.ToScaleDeviceResponse(device);
    }

    public async Task<IReadOnlyList<ScaleDeviceResponse>> GetActiveScaleDevicesAsync(
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var devices = await _scaleDeviceRepository.GetActiveAsync(siteId, cancellationToken);

        // Load calibrations for each device
        foreach (var device in devices)
        {
            var calibrations = await _scaleCalibrationRepository.GetByScaleDeviceIdAsync(device.Id, cancellationToken);
            device.SetCalibrations(calibrations);
        }

        return HarvestWorkflowMapper.ToScaleDeviceResponseList(devices);
    }

    public async Task<IReadOnlyList<ScaleDeviceResponse>> GetScaleDevicesByLocationAsync(
        Guid locationId,
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var devices = await _scaleDeviceRepository.GetByLocationAsync(locationId, siteId, cancellationToken);

        foreach (var device in devices)
        {
            var calibrations = await _scaleCalibrationRepository.GetByScaleDeviceIdAsync(device.Id, cancellationToken);
            device.SetCalibrations(calibrations);
        }

        return HarvestWorkflowMapper.ToScaleDeviceResponseList(devices);
    }

    public async Task<ScaleDeviceResponse> UpdateScaleDeviceAsync(
        Guid scaleDeviceId,
        CreateScaleDeviceRequest request,
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var device = await _scaleDeviceRepository.GetByIdAsync(scaleDeviceId, siteId, cancellationToken);
        if (device == null)
            throw new KeyNotFoundException($"Scale device {scaleDeviceId} not found");

        device.Update(
            request.DeviceName,
            request.DeviceSerialNumber,
            request.Manufacturer,
            request.Model,
            request.CapacityGrams,
            request.ReadabilityGrams,
            request.ConnectionType);

        if (!string.IsNullOrEmpty(request.ConnectionConfigJson))
        {
            device.UpdateConnectionConfig(request.ConnectionConfigJson);
        }

        device.UpdateLocation(request.LocationId, request.LocationName);
        device.UpdateCalibrationSettings(request.RequiresCalibration, request.CalibrationIntervalDays);

        var updatedDevice = await _scaleDeviceRepository.UpdateAsync(device, cancellationToken);

        // Load calibrations
        var calibrations = await _scaleCalibrationRepository.GetByScaleDeviceIdAsync(scaleDeviceId, cancellationToken);
        updatedDevice.SetCalibrations(calibrations);

        _logger.LogInformation("Updated scale device {DeviceId}", scaleDeviceId);

        return HarvestWorkflowMapper.ToScaleDeviceResponse(updatedDevice);
    }

    public async Task DeactivateScaleDeviceAsync(
        Guid scaleDeviceId,
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var device = await _scaleDeviceRepository.GetByIdAsync(scaleDeviceId, siteId, cancellationToken);
        if (device == null)
            throw new KeyNotFoundException($"Scale device {scaleDeviceId} not found");

        device.Deactivate();
        await _scaleDeviceRepository.UpdateAsync(device, cancellationToken);

        _logger.LogInformation("Deactivated scale device {DeviceId}", scaleDeviceId);
    }

    #endregion

    #region Calibration

    public async Task<ScaleCalibrationResponse> RecordCalibrationAsync(
        Guid scaleDeviceId,
        RecordScaleCalibrationRequest request,
        Guid siteId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Recording {CalibrationType} calibration for scale {ScaleDeviceId}",
            request.CalibrationType, scaleDeviceId);

        var device = await _scaleDeviceRepository.GetByIdAsync(scaleDeviceId, siteId, cancellationToken);
        if (device == null)
            throw new KeyNotFoundException($"Scale device {scaleDeviceId} not found");

        // Calculate due date based on device's calibration interval
        var dueDate = request.CalibrationDate.AddDays(device.CalibrationIntervalDays);

        var calibration = ScaleCalibration.Create(
            scaleDeviceId,
            request.CalibrationDate,
            dueDate,
            request.CalibrationType,
            request.Passed,
            userId);

        calibration.SetCalibrationDetails(
            request.PerformedBy,
            request.CertifiedBy,
            request.CertificationNumber,
            request.CalibrationCompany,
            request.TestWeightsUsedJson,
            request.DeviationGrams,
            request.DeviationPercent,
            request.Notes);

        if (!string.IsNullOrEmpty(request.CertificateUrl))
        {
            calibration.SetCertificate(request.CertificateUrl, null);
        }

        var savedCalibration = await _scaleCalibrationRepository.CreateAsync(calibration, cancellationToken);

        _logger.LogInformation(
            "Recorded calibration {CalibrationId} for scale {ScaleDeviceId}, passed: {Passed}, due: {DueDate}",
            savedCalibration.Id, scaleDeviceId, request.Passed, dueDate);

        return HarvestWorkflowMapper.ToCalibrationResponse(savedCalibration);
    }

    public async Task<ScaleCalibrationResponse?> GetCurrentCalibrationAsync(
        Guid scaleDeviceId,
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var device = await _scaleDeviceRepository.GetByIdAsync(scaleDeviceId, siteId, cancellationToken);
        if (device == null)
            return null;

        var calibration = await _scaleCalibrationRepository.GetCurrentValidAsync(scaleDeviceId, cancellationToken);
        if (calibration == null)
            return null;

        return HarvestWorkflowMapper.ToCalibrationResponse(calibration);
    }

    public async Task<IReadOnlyList<ScaleCalibrationResponse>> GetCalibrationHistoryAsync(
        Guid scaleDeviceId,
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var device = await _scaleDeviceRepository.GetByIdAsync(scaleDeviceId, siteId, cancellationToken);
        if (device == null)
            throw new KeyNotFoundException($"Scale device {scaleDeviceId} not found");

        var calibrations = await _scaleCalibrationRepository.GetByScaleDeviceIdAsync(scaleDeviceId, cancellationToken);
        return HarvestWorkflowMapper.ToCalibrationResponseList(calibrations);
    }

    public async Task<bool> IsCalibrationValidAsync(
        Guid scaleDeviceId,
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var device = await _scaleDeviceRepository.GetByIdAsync(scaleDeviceId, siteId, cancellationToken);
        if (device == null)
            return false;

        if (!device.RequiresCalibration)
            return true;

        var calibration = await _scaleCalibrationRepository.GetCurrentValidAsync(scaleDeviceId, cancellationToken);
        return calibration != null && calibration.IsValid();
    }

    public async Task<IReadOnlyList<ScaleDeviceResponse>> GetScalesWithExpiringCalibrationsAsync(
        int daysUntilExpiration,
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var allDevices = await _scaleDeviceRepository.GetActiveAsync(siteId, cancellationToken);
        var expiringDevices = new List<ScaleDevice>();
        var expirationThreshold = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(daysUntilExpiration));

        foreach (var device in allDevices)
        {
            if (!device.RequiresCalibration)
                continue;

            var calibration = await _scaleCalibrationRepository.GetCurrentValidAsync(device.Id, cancellationToken);
            
            // No calibration or calibration expiring soon
            if (calibration == null || calibration.CalibrationDueDate <= expirationThreshold)
            {
                var calibrations = await _scaleCalibrationRepository.GetByScaleDeviceIdAsync(device.Id, cancellationToken);
                device.SetCalibrations(calibrations);
                expiringDevices.Add(device);
            }
        }

        return HarvestWorkflowMapper.ToScaleDeviceResponseList(expiringDevices);
    }

    #endregion

    #region Scale Readings

    public async Task<ScaleReadingResponse> CreateScaleReadingAsync(
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
        CancellationToken cancellationToken = default)
    {
        // Get current calibration
        var calibration = await _scaleCalibrationRepository.GetCurrentValidAsync(scaleDeviceId, cancellationToken);

        var scaleReading = ScaleReading.Create(
            harvestId,
            harvestPlantId,
            lotId,
            scaleDeviceId,
            grossWeight,
            tareWeight,
            netWeight,
            unitOfWeight,
            isStable,
            stabilityDurationMs,
            DateTime.UtcNow,
            userId);

        if (calibration != null)
        {
            scaleReading.SetCalibrationSnapshot(
                calibration.Id,
                calibration.CalibrationDate,
                calibration.CalibrationDueDate,
                calibration.IsValid());
        }

        if (!string.IsNullOrEmpty(rawScaleDataJson))
        {
            scaleReading.SetRawScaleData(rawScaleDataJson);
        }

        var savedReading = await _scaleReadingRepository.CreateAsync(scaleReading, cancellationToken);

        _logger.LogDebug(
            "Created scale reading {ReadingId}: {NetWeight}{Unit} (stable: {IsStable})",
            savedReading.Id, netWeight, unitOfWeight, isStable);

        return HarvestWorkflowMapper.ToScaleReadingResponse(savedReading);
    }

    public async Task<IReadOnlyList<ScaleReadingResponse>> GetScaleReadingsForHarvestAsync(
        Guid harvestId,
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var readings = await _scaleReadingRepository.GetByHarvestIdAsync(harvestId, cancellationToken);
        return HarvestWorkflowMapper.ToScaleReadingResponseList(readings);
    }

    public async Task<ScaleReadingResponse?> GetScaleReadingAsync(
        Guid scaleReadingId,
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var reading = await _scaleReadingRepository.GetByIdAsync(scaleReadingId, cancellationToken);
        if (reading == null)
            return null;

        return HarvestWorkflowMapper.ToScaleReadingResponse(reading);
    }

    #endregion
}




