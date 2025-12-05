using System.Collections.Generic;
using System.Collections.ObjectModel;
using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Application.Mappers;
using Harvestry.Genetics.Domain.Entities;
using Harvestry.Genetics.Domain.Enums;
using Harvestry.Genetics.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Harvestry.Genetics.Application.Services;

/// <summary>
/// Application service that orchestrates mother plant health workflows and propagation controls.
/// </summary>
public sealed class MotherHealthService : IMotherHealthService
{
    private static readonly TimeSpan DefaultHealthCheckThreshold = TimeSpan.FromDays(14);

    private readonly IMotherPlantRepository _motherPlantRepository;
    private readonly IMotherHealthLogRepository _healthLogRepository;
    private readonly IPropagationSettingsRepository _propagationSettingsRepository;
    private readonly IPropagationOverrideRequestRepository _overrideRepository;
    private readonly IBatchRepository _batchRepository;
    private readonly IStrainRepository _strainRepository;
    private readonly ILogger<MotherHealthService> _logger;

    public MotherHealthService(
        IMotherPlantRepository motherPlantRepository,
        IMotherHealthLogRepository healthLogRepository,
        IPropagationSettingsRepository propagationSettingsRepository,
        IPropagationOverrideRequestRepository overrideRepository,
        IBatchRepository batchRepository,
        IStrainRepository strainRepository,
        ILogger<MotherHealthService> logger)
    {
        _motherPlantRepository = motherPlantRepository;
        _healthLogRepository = healthLogRepository;
        _propagationSettingsRepository = propagationSettingsRepository;
        _overrideRepository = overrideRepository;
        _batchRepository = batchRepository;
        _strainRepository = strainRepository;
        _logger = logger;
    }

    public async Task<MotherPlantResponse> CreateMotherPlantAsync(Guid siteId, CreateMotherPlantRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureUserId(userId);

        // Validate batch and strain belong to the site
        var batch = await _batchRepository.GetByIdAsync(request.BatchId, siteId, cancellationToken).ConfigureAwait(false);
        if (batch is null)
        {
            throw new InvalidOperationException($"Batch {request.BatchId} not found for site {siteId}.");
        }

        var strainExists = await _strainRepository.ExistsAsync(request.StrainId, siteId, cancellationToken).ConfigureAwait(false);
        if (!strainExists)
        {
            throw new InvalidOperationException($"Strain {request.StrainId} not found for site {siteId}.");
        }

        var existing = await _motherPlantRepository.GetByPlantTagAsync(siteId, request.PlantTag, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            throw new InvalidOperationException($"Mother plant with tag {request.PlantTag} already exists for this site.");
        }

        var plantId = PlantId.Create(request.PlantTag);
        var mother = MotherPlant.Create(
            siteId,
            request.BatchId,
            plantId,
            request.StrainId,
            request.DateEstablished,
            userId,
            request.LocationId,
            request.RoomId,
            request.MaxPropagationCount);

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            mother.UpdateNotes(request.Notes, userId);
        }

        if (request.Metadata is not null)
        {
            mother.UpdateMetadata(request.Metadata, userId);
        }

        await _motherPlantRepository.AddAsync(mother, cancellationToken).ConfigureAwait(false);
        return MotherPlantMapper.ToResponse(mother);
    }

    public async Task<MotherPlantResponse?> GetMotherPlantByIdAsync(Guid siteId, Guid motherPlantId, CancellationToken cancellationToken = default)
    {
        var mother = await _motherPlantRepository.GetByIdAsync(siteId, motherPlantId, cancellationToken).ConfigureAwait(false);
        return mother is null ? null : MotherPlantMapper.ToResponse(mother);
    }

    public async Task<IReadOnlyList<MotherPlantResponse>> GetMotherPlantsAsync(Guid siteId, MotherPlantStatus? status, CancellationToken cancellationToken = default)
    {
        var mothers = await _motherPlantRepository.GetBySiteAsync(siteId, status, cancellationToken).ConfigureAwait(false);
        return MotherPlantMapper.ToResponseList(mothers);
    }

    public async Task<MotherPlantResponse> UpdateMotherPlantAsync(Guid siteId, Guid motherPlantId, UpdateMotherPlantRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureUserId(userId);

        var mother = await GetMotherForUpdate(siteId, motherPlantId, cancellationToken).ConfigureAwait(false);

        if (request.LocationId.HasValue || request.RoomId.HasValue)
        {
            mother.UpdateLocation(request.LocationId, request.RoomId, userId);
        }

        if (request.MaxPropagationCount.HasValue)
        {
            mother.UpdatePropagationLimit(request.MaxPropagationCount, userId);
        }

        if (request.Metadata is not null)
        {
            mother.UpdateMetadata(request.Metadata, userId);
        }

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            mother.UpdateNotes(request.Notes, userId);
        }

        if (request.StatusUpdate is not null)
        {
            ApplyStatusUpdate(mother, request.StatusUpdate, userId);
        }

        await _motherPlantRepository.UpdateAsync(mother, cancellationToken).ConfigureAwait(false);
        return MotherPlantMapper.ToResponse(mother);
    }

    public async Task<MotherPlantResponse> RecordHealthLogAsync(Guid siteId, Guid motherPlantId, MotherPlantHealthLogRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureUserId(userId);

        var mother = await GetMotherForUpdate(siteId, motherPlantId, cancellationToken).ConfigureAwait(false);

        var logDate = request.LogDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var assessment = new HealthAssessment(
            request.Status,
            request.PestPressure,
            request.DiseasePressure,
            request.NutrientDeficiencies ?? Array.Empty<string>(),
            request.Observations,
            request.TreatmentsApplied,
            request.EnvironmentalNotes,
            ConvertPhotoUrls(request.PhotoUrls));

        mother.RecordHealthLog(logDate, assessment, userId);
        var recordedLog = mother.HealthLogs.Last();

        await _healthLogRepository.AddAsync(recordedLog, cancellationToken).ConfigureAwait(false);
        await _motherPlantRepository.UpdateAsync(mother, cancellationToken).ConfigureAwait(false);

        return MotherPlantMapper.ToResponse(mother);
    }

    public async Task<MotherPlantResponse> RegisterPropagationAsync(Guid siteId, Guid motherPlantId, RegisterPropagationRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureUserId(userId);

        if (request.PropagatedCount <= 0)
        {
            throw new ArgumentException("Propagation count must be greater than zero.", nameof(request));
        }

        var mother = await GetMotherForUpdate(siteId, motherPlantId, cancellationToken).ConfigureAwait(false);
        var settings = await EnsureSettings(siteId, userId, cancellationToken).ConfigureAwait(false);

        await ValidatePropagationLimits(siteId, mother, settings, request.PropagatedCount, userId, request.Notes, cancellationToken).ConfigureAwait(false);

        mother.RegisterPropagation(request.PropagatedCount, userId);

        if (request.PropagatedCount > 0)
        {
            await _motherPlantRepository.UpdatePropagationAsync(
                siteId,
                motherPlantId,
                mother.PropagationCount,
                mother.LastPropagationDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                request.PropagatedCount,
                userId,
                request.Notes,
                cancellationToken).ConfigureAwait(false);
        }

        await _motherPlantRepository.UpdateAsync(mother, cancellationToken).ConfigureAwait(false);
        return MotherPlantMapper.ToResponse(mother);
    }

    public async Task<MotherPlantHealthSummaryResponse> GetHealthSummaryAsync(Guid siteId, Guid motherPlantId, CancellationToken cancellationToken = default)
    {
        var mother = await GetMotherForUpdate(siteId, motherPlantId, cancellationToken).ConfigureAwait(false);
        var logs = await _healthLogRepository.GetByMotherPlantAsync(siteId, motherPlantId, cancellationToken).ConfigureAwait(false);

        MotherHealthLog? latestLog = logs.FirstOrDefault();
        var summaryLogs = MotherPlantMapper.ToHealthLogResponseList(logs.Take(5));

        var threshold = DefaultHealthCheckThreshold;
        var isOverdue = latestLog is null
            ? DateTime.UtcNow - mother.DateEstablished.ToDateTime(TimeOnly.MinValue) > threshold
            : DateTime.UtcNow - latestLog.LogDate.ToDateTime(TimeOnly.MinValue) > threshold;

        DateOnly? nextCheck = latestLog is null
            ? mother.DateEstablished.AddDays((int)threshold.TotalDays)
            : latestLog.LogDate.AddDays((int)threshold.TotalDays);

        return MotherPlantMapper.ToHealthSummary(
            mother,
            latestLog,
            isOverdue,
            nextCheck,
            summaryLogs);
    }

    public async Task<IReadOnlyList<MotherHealthLogResponse>> GetHealthLogsAsync(Guid siteId, Guid motherPlantId, CancellationToken cancellationToken = default)
    {
        var logs = await _healthLogRepository.GetByMotherPlantAsync(siteId, motherPlantId, cancellationToken).ConfigureAwait(false);
        return MotherPlantMapper.ToHealthLogResponseList(logs);
    }

    public async Task<IReadOnlyList<MotherPlantResponse>> GetOverdueForHealthCheckAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var mothers = await _motherPlantRepository.GetOverdueForHealthCheckAsync(siteId, DefaultHealthCheckThreshold, cancellationToken).ConfigureAwait(false);
        return MotherPlantMapper.ToResponseList(mothers);
    }

    public async Task<PropagationSettingsResponse> GetPropagationSettingsAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var settings = await _propagationSettingsRepository.GetBySiteAsync(siteId, cancellationToken).ConfigureAwait(false);
        if (settings is null)
        {
            return new PropagationSettingsResponse(
                null,
                siteId,
                null,
                null,
                null,
                true,
                null,
                new Dictionary<string, object>(),
                DateTime.MinValue,
                null);
        }

        return MotherPlantMapper.ToResponse(settings);
    }

    public async Task<PropagationSettingsResponse> UpdatePropagationSettingsAsync(Guid siteId, UpdatePropagationSettingsRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureUserId(userId);

        var settings = await _propagationSettingsRepository.GetBySiteAsync(siteId, cancellationToken).ConfigureAwait(false);
        if (settings is null)
        {
            settings = PropagationSettings.Create(
                siteId,
                userId,
                request.DailyLimit,
                request.WeeklyLimit,
                request.MotherPropagationLimit,
                request.RequiresOverrideApproval,
                string.IsNullOrWhiteSpace(request.ApproverRole) ? null : request.ApproverRole,
                request.ApproverPolicy);
        }
        else
        {
            settings.UpdateLimits(
                request.DailyLimit,
                request.WeeklyLimit,
                request.MotherPropagationLimit,
                request.RequiresOverrideApproval,
                string.IsNullOrWhiteSpace(request.ApproverRole) ? null : request.ApproverRole,
                request.ApproverPolicy,
                userId);
        }

        await _propagationSettingsRepository.UpsertAsync(settings, cancellationToken).ConfigureAwait(false);
        return MotherPlantMapper.ToResponse(settings);
    }

    public async Task<PropagationOverrideResponse> RequestPropagationOverrideAsync(Guid siteId, CreatePropagationOverrideRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureUserId(userId);

        var overrideRequest = PropagationOverrideRequest.Create(
            siteId,
            userId,
            request.RequestedQuantity,
            request.Reason,
            request.MotherPlantId,
            request.BatchId);

        await _overrideRepository.AddAsync(overrideRequest, cancellationToken).ConfigureAwait(false);
        return MotherPlantMapper.ToResponse(overrideRequest);
    }

    public async Task<PropagationOverrideResponse> DecidePropagationOverrideAsync(Guid siteId, Guid overrideId, PropagationOverrideDecisionRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureUserId(userId);

        var overrideRequest = await _overrideRepository.GetByIdAsync(siteId, overrideId, cancellationToken).ConfigureAwait(false);
        if (overrideRequest is null)
        {
            throw new KeyNotFoundException($"Propagation override {overrideId} not found for site {siteId}.");
        }

        switch (request.Decision)
        {
            case PropagationOverrideDecision.Approve:
                overrideRequest.Approve(userId, request.Notes);
                break;
            case PropagationOverrideDecision.Reject:
                overrideRequest.Reject(userId, request.Notes);
                break;
            case PropagationOverrideDecision.Expire:
                overrideRequest.Expire();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(request.Decision), request.Decision, "Unsupported override decision.");
        }

        await _overrideRepository.UpdateAsync(overrideRequest, cancellationToken).ConfigureAwait(false);
        return MotherPlantMapper.ToResponse(overrideRequest);
    }

    public async Task<IReadOnlyList<PropagationOverrideResponse>> GetPropagationOverridesAsync(Guid siteId, PropagationOverrideStatus? status, CancellationToken cancellationToken = default)
    {
        var overrides = await _overrideRepository.GetBySiteAsync(siteId, status, cancellationToken).ConfigureAwait(false);
        return MotherPlantMapper.ToResponseList(overrides);
    }

    private async Task<MotherPlant> GetMotherForUpdate(Guid siteId, Guid motherPlantId, CancellationToken cancellationToken)
    {
        var mother = await _motherPlantRepository.GetByIdAsync(siteId, motherPlantId, cancellationToken).ConfigureAwait(false);
        if (mother is null)
        {
            throw new KeyNotFoundException($"Mother plant {motherPlantId} not found for site {siteId}.");
        }

        return mother;
    }

    private async Task<PropagationSettings> EnsureSettings(Guid siteId, Guid userId, CancellationToken cancellationToken)
    {
        var settings = await _propagationSettingsRepository.GetBySiteAsync(siteId, cancellationToken).ConfigureAwait(false);
        if (settings is not null)
        {
            return settings;
        }

        var defaultSettings = PropagationSettings.Create(siteId, userId);
        await _propagationSettingsRepository.UpsertAsync(defaultSettings, cancellationToken).ConfigureAwait(false);
        return defaultSettings;
    }

    private static void ApplyStatusUpdate(MotherPlant mother, MotherPlantStatusUpdate statusUpdate, Guid userId)
    {
        switch (statusUpdate.Action)
        {
            case MotherPlantStatusAction.Retire:
                mother.Retire(statusUpdate.Reason ?? "Retired", userId);
                break;
            case MotherPlantStatusAction.Reactivate:
                mother.Reactivate(userId);
                break;
            case MotherPlantStatusAction.Quarantine:
                mother.Quarantine(statusUpdate.Reason ?? "Quarantine", userId);
                break;
            case MotherPlantStatusAction.ReleaseFromQuarantine:
                mother.ReleaseFromQuarantine(userId);
                break;
            case MotherPlantStatusAction.Destroy:
                mother.Destroy(statusUpdate.Reason ?? "Destroyed", userId);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(statusUpdate.Action), statusUpdate.Action, "Unsupported lifecycle action.");
        }
    }

    private async Task ValidatePropagationLimits(Guid siteId, MotherPlant mother, PropagationSettings settings, int requestedCount, Guid userId, string? notes, CancellationToken cancellationToken)
    {
        if (!mother.CanPropagate(settings, requestedCount))
        {
            throw new InvalidOperationException("Mother plant propagation limits have been exceeded.");
        }

        // Calculate the new propagation count
        var newPropagationCount = mother.PropagationCount + requestedCount;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Atomic limit enforcement at database level - no race conditions
        var success = await _motherPlantRepository.TryUpdatePropagationWithLimitsAsync(
            siteId,
            mother.Id,
            newPropagationCount,
            today,
            requestedCount,
            userId,
            notes,
            settings.DailyLimit,
            settings.WeeklyLimit,
            cancellationToken).ConfigureAwait(false);

        if (!success)
        {
            HandleLimitExceeded(settings);
        }
    }

    private void HandleLimitExceeded(PropagationSettings settings)
    {
        if (settings.RequiresOverrideApproval)
        {
            throw new InvalidOperationException("Propagation limits exceeded. Submit an override request for approval.");
        }
    }

    private static void EnsureUserId(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User identifier is required for this operation.", nameof(userId));
        }
    }

    private IReadOnlyCollection<Uri> ConvertPhotoUrls(IReadOnlyCollection<string>? photoUrls)
    {
        if (photoUrls is null || photoUrls.Count == 0)
        {
            return Array.Empty<Uri>();
        }

        var validUris = new List<Uri>();
        foreach (var urlString in photoUrls)
        {
            if (Uri.TryCreate(urlString, UriKind.Absolute, out var uri))
            {
                if (string.Equals(uri.Scheme, "http", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
                {
                    validUris.Add(uri);
                }
                else
                {
                    _logger.LogWarning("Invalid or unsupported photo URL scheme skipped: {Domain}/{Path} (scheme: {Scheme})",
                        uri.Host, uri.AbsolutePath, uri.Scheme);
                }
            }
            else
            {
                _logger.LogWarning("Invalid photo URL skipped: {Url}", urlString);
            }
        }

        return new ReadOnlyCollection<Uri>(validUris);
    }
}
