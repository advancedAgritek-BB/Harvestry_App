using Harvestry.Compliance.Metrc.Application.Interfaces;
using Harvestry.Compliance.Metrc.Domain.Entities;
using Harvestry.Compliance.Metrc.Domain.Enums;
using Harvestry.Compliance.Metrc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Compliance.Metrc.Infrastructure.Repositories;

/// <summary>
/// Repository for METRC sync job persistence
/// </summary>
public sealed class MetrcSyncJobRepository : IMetrcSyncJobRepository
{
    private readonly MetrcDbContext _context;

    public MetrcSyncJobRepository(MetrcDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<MetrcSyncJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SyncJobs
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<MetrcSyncJob>> GetBySiteIdAsync(
        Guid siteId,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        return await _context.SyncJobs
            .Where(j => j.SiteId == siteId)
            .OrderByDescending(j => j.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<MetrcSyncJob?> GetActiveJobAsync(
        string licenseNumber,
        CancellationToken cancellationToken = default)
    {
        return await _context.SyncJobs
            .Where(j => j.LicenseNumber == licenseNumber.ToUpperInvariant()
                && (j.Status == SyncStatus.Pending || j.Status == SyncStatus.Processing))
            .OrderByDescending(j => j.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MetrcSyncJob>> GetPendingJobsAsync(
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        return await _context.SyncJobs
            .Where(j => j.Status == SyncStatus.Pending)
            .OrderBy(j => j.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(MetrcSyncJob job, CancellationToken cancellationToken = default)
    {
        await _context.SyncJobs.AddAsync(job, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(MetrcSyncJob job, CancellationToken cancellationToken = default)
    {
        _context.SyncJobs.Update(job);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
