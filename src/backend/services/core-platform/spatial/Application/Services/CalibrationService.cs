using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Spatial.Application.Common;
using Harvestry.Spatial.Application.DTOs;
using Harvestry.Spatial.Application.Exceptions;
using Harvestry.Spatial.Application.Interfaces;
using Harvestry.Spatial.Application.Mappers;
using Harvestry.Spatial.Application.ViewModels;
using Harvestry.Spatial.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Harvestry.Spatial.Application.Services;

/// <summary>
/// Provides calibration workflows for equipment, including recording events and retrieving history/overdue reports.
/// </summary>
public sealed class CalibrationService : ICalibrationService
{
    private readonly ICalibrationRepository _calibrationRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly ILogger<CalibrationService> _logger;

    private const int DefaultCalibrationIntervalDays = 30;

    public CalibrationService(
        ICalibrationRepository calibrationRepository,
        IEquipmentRepository equipmentRepository,
        ILogger<CalibrationService> logger)
    {
        _calibrationRepository = calibrationRepository ?? throw new ArgumentNullException(nameof(calibrationRepository));
        _equipmentRepository = equipmentRepository ?? throw new ArgumentNullException(nameof(equipmentRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CalibrationResponse> RecordAsync(Guid siteId, Guid equipmentId, CreateCalibrationRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        ValidationHelpers.EnsureNotEmpty(siteId, nameof(siteId));
        ValidationHelpers.EnsureNotEmpty(equipmentId, nameof(equipmentId));
        ValidationHelpers.EnsureNotEmpty(request.PerformedByUserId, nameof(request.PerformedByUserId));

        ValidationHelpers.EnsureRouteMatchesPayload(siteId, request.SiteId, nameof(request.SiteId));
        ValidationHelpers.EnsureRouteMatchesPayload(equipmentId, request.EquipmentId, nameof(request.EquipmentId));

        var equipment = await LoadEquipmentAsync(equipmentId, cancellationToken).ConfigureAwait(false)
                        ?? throw new KeyNotFoundException($"Equipment {equipmentId} was not found");

        ValidationHelpers.EnsureSameSite(siteId, equipment.SiteId, nameof(Equipment), equipment.Id);

        var intervalDays = DetermineIntervalDays(request.IntervalDaysOverride, equipment.CalibrationIntervalDays);
        var calibration = new Calibration(
            equipmentId,
            request.Method,
            request.ReferenceValue,
            request.MeasuredValue,
            request.Result,
            request.PerformedByUserId,
            request.ChannelCode,
            request.CoefficientsJson,
            request.FirmwareVersionAtCalibration);

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            calibration.AddNotes(request.Notes);
        }

        if (!string.IsNullOrWhiteSpace(request.AttachmentUrl))
        {
            calibration.AttachDocumentation(request.AttachmentUrl);
        }

        var nextDueAt = calibration.PerformedAt.AddDays(intervalDays);
        calibration.SetNextDueDate(nextDueAt);

        await _calibrationRepository.InsertAsync(calibration, cancellationToken).ConfigureAwait(false);

        equipment.RecordCalibration(calibration.PerformedAt, nextDueAt, intervalDays, request.PerformedByUserId);
        await _equipmentRepository.UpdateAsync(equipment, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Calibration recorded for equipment {EquipmentId} at site {SiteId}. Method={Method}, Result={Result}, NextDue={NextDueAt}",
            equipmentId,
            siteId,
            request.Method,
            request.Result,
            nextDueAt);

        return CalibrationMapper.ToResponse(calibration, intervalDays, DateTime.UtcNow);
    }

    public async Task<IReadOnlyList<CalibrationResponse>> GetHistoryAsync(Guid siteId, Guid equipmentId, CancellationToken cancellationToken = default)
    {
        ValidationHelpers.EnsureNotEmpty(siteId, nameof(siteId));
        ValidationHelpers.EnsureNotEmpty(equipmentId, nameof(equipmentId));

        var equipment = await LoadEquipmentAsync(equipmentId, cancellationToken).ConfigureAwait(false)
                        ?? throw new KeyNotFoundException($"Equipment {equipmentId} was not found");

        ValidationHelpers.EnsureSameSite(siteId, equipment.SiteId, nameof(Equipment), equipment.Id);

        var calibrations = await _calibrationRepository.GetByEquipmentIdAsync(equipmentId, cancellationToken).ConfigureAwait(false);
        var utcNow = DateTime.UtcNow;

        return calibrations
            .Select(cal => CalibrationMapper.ToResponse(cal, DetermineIntervalForResponse(cal, equipment), utcNow))
            .ToArray();
    }

    public async Task<CalibrationResponse?> GetLatestAsync(Guid siteId, Guid equipmentId, CancellationToken cancellationToken = default)
    {
        ValidationHelpers.EnsureNotEmpty(siteId, nameof(siteId));
        ValidationHelpers.EnsureNotEmpty(equipmentId, nameof(equipmentId));

        var equipment = await LoadEquipmentAsync(equipmentId, cancellationToken).ConfigureAwait(false)
                        ?? throw new KeyNotFoundException($"Equipment {equipmentId} was not found");

        ValidationHelpers.EnsureSameSite(siteId, equipment.SiteId, nameof(Equipment), equipment.Id);

        var calibration = await _calibrationRepository.GetLatestByEquipmentIdAsync(equipmentId, cancellationToken).ConfigureAwait(false);
        if (calibration is null)
        {
            return null;
        }

        return CalibrationMapper.ToResponse(calibration, DetermineIntervalForResponse(calibration, equipment), DateTime.UtcNow);
    }

    public async Task<IReadOnlyList<CalibrationResponse>> GetOverdueAsync(Guid siteId, DateTime? dueBeforeUtc, CancellationToken cancellationToken = default)
    {
        ValidationHelpers.EnsureNotEmpty(siteId, nameof(siteId));

        var effectiveDueDate = dueBeforeUtc ?? DateTime.UtcNow;
        var calibrations = await _calibrationRepository.GetOverdueAsync(siteId, effectiveDueDate, cancellationToken).ConfigureAwait(false);
        var utcNow = DateTime.UtcNow;

        var responses = calibrations
            .Select(cal => CalibrationMapper.ToResponse(cal, DetermineIntervalForResponse(cal, equipment: null), utcNow))
            .ToArray();

        foreach (var response in responses.Where(r => r.IsCritical))
        {
            _logger.LogWarning(
                "Calibration overdue >7 days for equipment {EquipmentId}. NextDue={NextDueAt}, Deviance={DeviationPercent}%",
                response.EquipmentId,
                response.NextDueAt,
                response.DeviationPct);
        }

        return responses;
    }


    private static int DetermineIntervalDays(int? overrideInterval, int? equipmentInterval)
    {
        var interval = overrideInterval ?? equipmentInterval ?? DefaultCalibrationIntervalDays;
        if (interval <= 0)
        {
            // Determine accurate parameter name for the exception
            string paramName = overrideInterval.HasValue
                ? nameof(overrideInterval)
                : equipmentInterval.HasValue
                    ? nameof(equipmentInterval)
                    : nameof(DefaultCalibrationIntervalDays);
            
            throw new ArgumentOutOfRangeException(paramName, "Calibration interval must be positive.");
        }

        return interval;
    }

    private static int? DetermineIntervalForResponse(Calibration calibration, Equipment? equipment)
    {
        if (calibration == null) throw new ArgumentNullException(nameof(calibration));

        if (calibration.NextDueAt.HasValue)
        {
            var totalDays = (int)Math.Round((calibration.NextDueAt.Value - calibration.PerformedAt).TotalDays);
            if (totalDays > 0)
            {
                return totalDays;
            }
        }

        if (equipment?.CalibrationIntervalDays is { } interval && interval > 0)
        {
            return interval;
        }

        return null;
    }

    private Task<Equipment?> LoadEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken)
    {
        return _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken);
    }
}
