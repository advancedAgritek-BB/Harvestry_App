using Harvestry.Compliance.Metrc.Application.DTOs;
using Harvestry.Compliance.Metrc.Domain.Entities;
using Harvestry.Compliance.Metrc.Domain.Enums;

namespace Harvestry.Compliance.Metrc.Application.Interfaces;

/// <summary>
/// Service interface for METRC synchronization operations
/// </summary>
public interface IMetrcSyncService
{
    /// <summary>
    /// Starts a new sync job
    /// </summary>
    Task<StartSyncResponse> StartSyncAsync(
        StartSyncRequest request,
        Guid? initiatedByUserId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a sync job by ID
    /// </summary>
    Task<SyncJobDto?> GetSyncJobAsync(
        Guid syncJobId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets sync jobs for a site
    /// </summary>
    Task<IReadOnlyList<SyncJobDto>> GetSyncJobsAsync(
        Guid siteId,
        int limit = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a running sync job
    /// </summary>
    Task<bool> CancelSyncJobAsync(
        Guid syncJobId,
        string? reason = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets sync status summary for a license
    /// </summary>
    Task<SyncStatusSummaryDto?> GetSyncStatusAsync(
        Guid licenseId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs reconciliation between Harvestry and METRC
    /// </summary>
    Task<ReconciliationResponseDto> ReconcileAsync(
        ReconciliationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retries failed queue items for a sync job
    /// </summary>
    Task<int> RetryFailedItemsAsync(
        Guid syncJobId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets queue items for a sync job
    /// </summary>
    Task<IReadOnlyList<QueueItemDto>> GetQueueItemsAsync(
        Guid syncJobId,
        SyncStatus? statusFilter = null,
        int limit = 100,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for license management
/// </summary>
public interface IMetrcLicenseService
{
    /// <summary>
    /// Gets a license by ID
    /// </summary>
    Task<LicenseDto?> GetLicenseAsync(
        Guid licenseId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all licenses for a site
    /// </summary>
    Task<IReadOnlyList<LicenseDto>> GetLicensesForSiteAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a license by license number
    /// </summary>
    Task<LicenseDto?> GetLicenseByNumberAsync(
        string licenseNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates a license
    /// </summary>
    Task<LicenseDto> UpsertLicenseAsync(
        UpsertLicenseRequest request,
        Guid? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets credentials for a license
    /// </summary>
    Task<bool> SetCredentialsAsync(
        SetCredentialsRequest request,
        Guid? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates a license
    /// </summary>
    Task<bool> ActivateLicenseAsync(
        Guid licenseId,
        Guid? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a license
    /// </summary>
    Task<bool> DeactivateLicenseAsync(
        Guid licenseId,
        Guid? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests connection to METRC with license credentials
    /// </summary>
    Task<(bool Success, string Message)> TestConnectionAsync(
        Guid licenseId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets licenses that are due for automatic sync
    /// </summary>
    Task<IReadOnlyList<MetrcLicense>> GetLicensesDueForSyncAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for queue operations
/// </summary>
public interface IMetrcQueueService
{
    /// <summary>
    /// Enqueues an entity for sync to METRC
    /// </summary>
    Task<Guid> EnqueueAsync(
        Guid syncJobId,
        Guid siteId,
        string licenseNumber,
        MetrcEntityType entityType,
        MetrcOperationType operationType,
        Guid harvestryEntityId,
        object payload,
        int priority = 100,
        long? metrcId = null,
        string? metrcLabel = null,
        Guid? dependsOnItemId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the next batch of items to process
    /// </summary>
    Task<IReadOnlyList<MetrcQueueItem>> GetNextBatchAsync(
        string licenseNumber,
        int batchSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an item as completed
    /// </summary>
    Task CompleteItemAsync(
        Guid itemId,
        long? metrcId = null,
        string? metrcLabel = null,
        string? responseJson = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an item as failed
    /// </summary>
    Task FailItemAsync(
        Guid itemId,
        string errorMessage,
        string? errorCode = null,
        string? responseJson = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending item count for a license
    /// </summary>
    Task<int> GetPendingCountAsync(
        string licenseNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets failed item count for a license
    /// </summary>
    Task<int> GetFailedCountAsync(
        string licenseNumber,
        CancellationToken cancellationToken = default);
}
