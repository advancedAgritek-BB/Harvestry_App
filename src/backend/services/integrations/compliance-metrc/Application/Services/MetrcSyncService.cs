using System.Text.Json;
using Harvestry.Compliance.Metrc.Application.DTOs;
using Harvestry.Compliance.Metrc.Application.Interfaces;
using Harvestry.Compliance.Metrc.Domain.Entities;
using Harvestry.Compliance.Metrc.Domain.Enums;
using Harvestry.Compliance.Metrc.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Harvestry.Compliance.Metrc.Application.Services;

/// <summary>
/// Service for orchestrating METRC synchronization operations
/// </summary>
public sealed class MetrcSyncService : IMetrcSyncService
{
    private readonly IMetrcSyncJobRepository _syncJobRepository;
    private readonly IMetrcQueueItemRepository _queueItemRepository;
    private readonly IMetrcLicenseRepository _licenseRepository;
    private readonly IMetrcSyncCheckpointRepository _checkpointRepository;
    private readonly ILogger<MetrcSyncService> _logger;

    public MetrcSyncService(
        IMetrcSyncJobRepository syncJobRepository,
        IMetrcQueueItemRepository queueItemRepository,
        IMetrcLicenseRepository licenseRepository,
        IMetrcSyncCheckpointRepository checkpointRepository,
        ILogger<MetrcSyncService> logger)
    {
        _syncJobRepository = syncJobRepository ?? throw new ArgumentNullException(nameof(syncJobRepository));
        _queueItemRepository = queueItemRepository ?? throw new ArgumentNullException(nameof(queueItemRepository));
        _licenseRepository = licenseRepository ?? throw new ArgumentNullException(nameof(licenseRepository));
        _checkpointRepository = checkpointRepository ?? throw new ArgumentNullException(nameof(checkpointRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<StartSyncResponse> StartSyncAsync(
        StartSyncRequest request,
        Guid? initiatedByUserId = null,
        CancellationToken cancellationToken = default)
    {
        // Validate license exists
        var license = await _licenseRepository.GetByLicenseNumberAsync(
            request.LicenseNumber, cancellationToken);

        if (license == null)
        {
            return new StartSyncResponse
            {
                SyncJobId = Guid.Empty,
                Status = SyncStatus.Failed,
                Message = $"License {request.LicenseNumber} not found"
            };
        }

        if (!license.HasCredentials)
        {
            return new StartSyncResponse
            {
                SyncJobId = Guid.Empty,
                Status = SyncStatus.Failed,
                Message = $"License {request.LicenseNumber} does not have credentials configured"
            };
        }

        // Check for existing active job
        var existingJob = await _syncJobRepository.GetActiveJobAsync(
            request.LicenseNumber, cancellationToken);

        if (existingJob != null)
        {
            return new StartSyncResponse
            {
                SyncJobId = existingJob.Id,
                Status = existingJob.Status,
                Message = $"Sync job already in progress: {existingJob.Id}"
            };
        }

        // Create new sync job
        var job = MetrcSyncJob.Create(
            request.SiteId,
            request.LicenseNumber,
            license.StateCode,
            request.Direction,
            initiatedByUserId,
            initiatedByUserId.HasValue ? "user" : "system");

        await _syncJobRepository.CreateAsync(job, cancellationToken);

        _logger.LogInformation(
            "Started METRC sync job {JobId} for license {LicenseNumber} (direction: {Direction})",
            job.Id, request.LicenseNumber, request.Direction);

        // Reset checkpoints if forcing full sync
        if (request.ForceFullSync)
        {
            await _checkpointRepository.ResetAsync(license.Id, cancellationToken: cancellationToken);
            _logger.LogInformation(
                "Reset sync checkpoints for license {LicenseNumber} (forced full sync)",
                request.LicenseNumber);
        }

        return new StartSyncResponse
        {
            SyncJobId = job.Id,
            Status = job.Status,
            Message = "Sync job started successfully"
        };
    }

    public async Task<SyncJobDto?> GetSyncJobAsync(
        Guid syncJobId,
        CancellationToken cancellationToken = default)
    {
        var job = await _syncJobRepository.GetByIdAsync(syncJobId, cancellationToken);
        return job != null ? MapToDto(job) : null;
    }

    public async Task<IReadOnlyList<SyncJobDto>> GetSyncJobsAsync(
        Guid siteId,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var jobs = await _syncJobRepository.GetBySiteIdAsync(siteId, limit, cancellationToken);
        return jobs.Select(MapToDto).ToList();
    }

    public async Task<bool> CancelSyncJobAsync(
        Guid syncJobId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var job = await _syncJobRepository.GetByIdAsync(syncJobId, cancellationToken);
        if (job == null || job.IsTerminal)
        {
            return false;
        }

        job.Cancel(reason);
        await _syncJobRepository.UpdateAsync(job, cancellationToken);

        _logger.LogInformation(
            "Cancelled METRC sync job {JobId} for license {LicenseNumber}: {Reason}",
            syncJobId, job.LicenseNumber, reason ?? "No reason provided");

        return true;
    }

    public async Task<SyncStatusSummaryDto?> GetSyncStatusAsync(
        Guid licenseId,
        CancellationToken cancellationToken = default)
    {
        var license = await _licenseRepository.GetByIdAsync(licenseId, cancellationToken);
        if (license == null)
        {
            return null;
        }

        var activeJob = await _syncJobRepository.GetActiveJobAsync(
            license.LicenseNumber, cancellationToken);

        var pendingCount = await _queueItemRepository.GetPendingCountAsync(
            license.LicenseNumber, cancellationToken);

        var failedCount = await _queueItemRepository.GetFailedCountAsync(
            license.LicenseNumber, cancellationToken);

        var checkpoints = await _checkpointRepository.GetByLicenseIdAsync(
            licenseId, cancellationToken);

        return new SyncStatusSummaryDto
        {
            LicenseId = licenseId,
            LicenseNumber = license.LicenseNumber,
            LastSyncAt = license.LastSyncAt,
            LastSuccessfulSyncAt = license.LastSuccessfulSyncAt,
            IsSyncInProgress = activeJob != null,
            CurrentSyncJobId = activeJob?.Id,
            PendingQueueItems = pendingCount,
            FailedQueueItems = failedCount,
            EntityStatuses = checkpoints.Select(c => new EntitySyncStatusDto
            {
                EntityType = c.EntityType,
                LastSyncAt = c.LastSuccessfulSyncAt,
                LastSyncItemCount = c.LastSyncItemCount,
                ConsecutiveFailures = c.ConsecutiveFailures,
                LastError = c.LastError
            }).ToList()
        };
    }

    public async Task<ReconciliationResponseDto> ReconcileAsync(
        ReconciliationRequest request,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTimeOffset.UtcNow;
        var license = await _licenseRepository.GetByIdAsync(request.LicenseId, cancellationToken);

        if (license == null)
        {
            throw new InvalidOperationException($"License {request.LicenseId} not found");
        }

        _logger.LogInformation(
            "Starting reconciliation for license {LicenseNumber} ({EntityTypes})",
            license.LicenseNumber,
            string.Join(", ", request.EntityTypes));

        var entityResults = new List<EntityReconciliationDto>();
        var entityTypes = request.EntityTypes.Count > 0
            ? request.EntityTypes
            : new List<MetrcEntityType>
            {
                MetrcEntityType.Plant,
                MetrcEntityType.PlantBatch,
                MetrcEntityType.Harvest,
                MetrcEntityType.Package
            };

        // Reconciliation would call METRC APIs and compare with local data
        // For now, return a placeholder response
        foreach (var entityType in entityTypes)
        {
            entityResults.Add(new EntityReconciliationDto
            {
                EntityType = entityType,
                HarvestryCount = 0,
                MetrcCount = 0,
                MatchedCount = 0,
                HarvestryOnlyCount = 0,
                MetrcOnlyCount = 0,
                DiscrepancyCount = 0,
                Discrepancies = request.IncludeDetails ? new List<DiscrepancyDto>() : null
            });
        }

        var duration = DateTimeOffset.UtcNow - startTime;

        return new ReconciliationResponseDto
        {
            LicenseId = license.Id,
            LicenseNumber = license.LicenseNumber,
            ReconciliationTimestamp = DateTimeOffset.UtcNow,
            Duration = duration,
            IsInSync = entityResults.All(r => r.IsInSync),
            EntityResults = entityResults
        };
    }

    public async Task<int> RetryFailedItemsAsync(
        Guid syncJobId,
        CancellationToken cancellationToken = default)
    {
        var job = await _syncJobRepository.GetByIdAsync(syncJobId, cancellationToken);
        if (job == null)
        {
            return 0;
        }

        var retriedCount = await _queueItemRepository.RetryFailedItemsAsync(
            syncJobId, cancellationToken);

        if (retriedCount > 0)
        {
            _logger.LogInformation(
                "Retrying {Count} failed items for sync job {JobId}",
                retriedCount, syncJobId);
        }

        return retriedCount;
    }

    public async Task<IReadOnlyList<QueueItemDto>> GetQueueItemsAsync(
        Guid syncJobId,
        SyncStatus? statusFilter = null,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var items = await _queueItemRepository.GetBySyncJobIdAsync(
            syncJobId, statusFilter, limit, cancellationToken);

        return items.Select(MapToDto).ToList();
    }

    private static SyncJobDto MapToDto(MetrcSyncJob job)
    {
        return new SyncJobDto
        {
            Id = job.Id,
            SiteId = job.SiteId,
            LicenseNumber = job.LicenseNumber,
            StateCode = job.StateCode,
            Direction = job.Direction,
            Status = job.Status,
            CreatedAt = job.CreatedAt,
            StartedAt = job.StartedAt,
            CompletedAt = job.CompletedAt,
            TotalItems = job.TotalItems,
            ProcessedItems = job.ProcessedItems,
            SuccessfulItems = job.SuccessfulItems,
            FailedItems = job.FailedItems,
            RetryCount = job.RetryCount,
            ErrorMessage = job.ErrorMessage,
            Duration = job.Duration
        };
    }

    private static QueueItemDto MapToDto(MetrcQueueItem item)
    {
        return new QueueItemDto
        {
            Id = item.Id,
            SyncJobId = item.SyncJobId,
            EntityType = item.EntityType,
            OperationType = item.OperationType,
            HarvestryEntityId = item.HarvestryEntityId,
            MetrcId = item.MetrcId,
            MetrcLabel = item.MetrcLabel,
            Status = item.Status,
            Priority = item.Priority,
            RetryCount = item.RetryCount,
            CreatedAt = item.CreatedAt,
            ProcessedAt = item.ProcessedAt,
            CompletedAt = item.CompletedAt,
            ErrorMessage = item.ErrorMessage,
            ErrorCode = item.ErrorCode
        };
    }
}
