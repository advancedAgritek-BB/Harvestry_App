using Harvestry.Compliance.Metrc.Domain.Entities;
using Harvestry.Compliance.Metrc.Domain.Enums;

namespace Harvestry.Compliance.Metrc.Application.Interfaces;

/// <summary>
/// Repository interface for METRC sync jobs
/// </summary>
public interface IMetrcSyncJobRepository
{
    Task<MetrcSyncJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<MetrcSyncJob>> GetBySiteIdAsync(
        Guid siteId,
        int limit = 20,
        CancellationToken cancellationToken = default);
    
    Task<MetrcSyncJob?> GetActiveJobAsync(
        string licenseNumber,
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<MetrcSyncJob>> GetPendingJobsAsync(
        int limit = 100,
        CancellationToken cancellationToken = default);
    
    Task CreateAsync(MetrcSyncJob job, CancellationToken cancellationToken = default);
    
    Task UpdateAsync(MetrcSyncJob job, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for METRC queue items
/// </summary>
public interface IMetrcQueueItemRepository
{
    Task<MetrcQueueItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<MetrcQueueItem>> GetBySyncJobIdAsync(
        Guid syncJobId,
        SyncStatus? statusFilter = null,
        int limit = 100,
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<MetrcQueueItem>> GetReadyForProcessingAsync(
        string licenseNumber,
        int batchSize = 50,
        CancellationToken cancellationToken = default);
    
    Task<int> GetPendingCountAsync(
        string licenseNumber,
        CancellationToken cancellationToken = default);
    
    Task<int> GetFailedCountAsync(
        string licenseNumber,
        CancellationToken cancellationToken = default);
    
    Task<MetrcQueueItem?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default);
    
    Task CreateAsync(MetrcQueueItem item, CancellationToken cancellationToken = default);
    
    Task CreateBatchAsync(
        IEnumerable<MetrcQueueItem> items,
        CancellationToken cancellationToken = default);
    
    Task UpdateAsync(MetrcQueueItem item, CancellationToken cancellationToken = default);
    
    Task<int> RetryFailedItemsAsync(
        Guid syncJobId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for METRC licenses
/// </summary>
public interface IMetrcLicenseRepository
{
    Task<MetrcLicense?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<MetrcLicense?> GetByLicenseNumberAsync(
        string licenseNumber,
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<MetrcLicense>> GetBySiteIdAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<MetrcLicense>> GetDueForSyncAsync(
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<MetrcLicense>> GetActiveAsync(
        CancellationToken cancellationToken = default);
    
    Task CreateAsync(MetrcLicense license, CancellationToken cancellationToken = default);
    
    Task UpdateAsync(MetrcLicense license, CancellationToken cancellationToken = default);
    
    Task<bool> ExistsAsync(
        string licenseNumber,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for METRC sync checkpoints
/// </summary>
public interface IMetrcSyncCheckpointRepository
{
    Task<MetrcSyncCheckpoint?> GetAsync(
        Guid licenseId,
        MetrcEntityType entityType,
        SyncDirection direction,
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<MetrcSyncCheckpoint>> GetByLicenseIdAsync(
        Guid licenseId,
        CancellationToken cancellationToken = default);
    
    Task UpsertAsync(
        MetrcSyncCheckpoint checkpoint,
        CancellationToken cancellationToken = default);
    
    Task ResetAsync(
        Guid licenseId,
        MetrcEntityType? entityType = null,
        CancellationToken cancellationToken = default);
}
