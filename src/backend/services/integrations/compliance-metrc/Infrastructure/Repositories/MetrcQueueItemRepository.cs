using Harvestry.Compliance.Metrc.Application.Interfaces;
using Harvestry.Compliance.Metrc.Domain.Entities;
using Harvestry.Compliance.Metrc.Domain.Enums;
using Harvestry.Compliance.Metrc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Compliance.Metrc.Infrastructure.Repositories;

/// <summary>
/// Repository for METRC queue item persistence
/// </summary>
public sealed class MetrcQueueItemRepository : IMetrcQueueItemRepository
{
    private readonly MetrcDbContext _context;

    public MetrcQueueItemRepository(MetrcDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<MetrcQueueItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.QueueItems
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<MetrcQueueItem>> GetBySyncJobIdAsync(
        Guid syncJobId,
        SyncStatus? statusFilter = null,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var query = _context.QueueItems.Where(i => i.SyncJobId == syncJobId);

        if (statusFilter.HasValue)
        {
            query = query.Where(i => i.Status == statusFilter.Value);
        }

        return await query
            .OrderBy(i => i.Priority)
            .ThenBy(i => i.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MetrcQueueItem>> GetReadyForProcessingAsync(
        string licenseNumber,
        int batchSize = 50,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        return await _context.QueueItems
            .Where(i => i.LicenseNumber == licenseNumber.ToUpperInvariant()
                && (i.Status == SyncStatus.Pending || i.Status == SyncStatus.Failed)
                && (i.ScheduledAt == null || i.ScheduledAt <= now)
                && i.DependsOnItemId == null) // Don't process items with unmet dependencies
            .OrderBy(i => i.Priority)
            .ThenBy(i => i.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetPendingCountAsync(
        string licenseNumber,
        CancellationToken cancellationToken = default)
    {
        return await _context.QueueItems
            .CountAsync(i => i.LicenseNumber == licenseNumber.ToUpperInvariant()
                && i.Status == SyncStatus.Pending, cancellationToken);
    }

    public async Task<int> GetFailedCountAsync(
        string licenseNumber,
        CancellationToken cancellationToken = default)
    {
        return await _context.QueueItems
            .CountAsync(i => i.LicenseNumber == licenseNumber.ToUpperInvariant()
                && (i.Status == SyncStatus.Failed || i.Status == SyncStatus.FailedPermanent), cancellationToken);
    }

    public async Task<MetrcQueueItem?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        return await _context.QueueItems
            .FirstOrDefaultAsync(i => i.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public async Task CreateAsync(MetrcQueueItem item, CancellationToken cancellationToken = default)
    {
        await _context.QueueItems.AddAsync(item, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateBatchAsync(
        IEnumerable<MetrcQueueItem> items,
        CancellationToken cancellationToken = default)
    {
        await _context.QueueItems.AddRangeAsync(items, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(MetrcQueueItem item, CancellationToken cancellationToken = default)
    {
        _context.QueueItems.Update(item);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> RetryFailedItemsAsync(
        Guid syncJobId,
        CancellationToken cancellationToken = default)
    {
        var failedItems = await _context.QueueItems
            .Where(i => i.SyncJobId == syncJobId && i.Status == SyncStatus.Failed && i.CanRetry)
            .ToListAsync(cancellationToken);

        foreach (var item in failedItems)
        {
            // Reset to pending for retry
            item.Schedule(DateTimeOffset.UtcNow);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return failedItems.Count;
    }
}
