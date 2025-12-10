using Harvestry.ProcessingJobs.Application.Interfaces;
using Harvestry.ProcessingJobs.Domain.Entities;
using Harvestry.ProcessingJobs.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.ProcessingJobs.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for ProcessingJob entities
/// </summary>
public class ProcessingJobRepository : IProcessingJobRepository
{
    private readonly ProcessingJobsDbContext _context;

    public ProcessingJobRepository(ProcessingJobsDbContext context) => _context = context;

    public async Task<ProcessingJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.ProcessingJobs
            .Include(j => j.Inputs)
            .Include(j => j.Outputs)
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);

    public async Task<ProcessingJob?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default)
        => await _context.ProcessingJobs
            .Include(j => j.Inputs)
            .Include(j => j.Outputs)
            .FirstOrDefaultAsync(j => j.SiteId == siteId && j.Id == id, cancellationToken);

    public async Task<ProcessingJob?> GetByJobNumberAsync(Guid siteId, string jobNumber, CancellationToken cancellationToken = default)
        => await _context.ProcessingJobs
            .Include(j => j.Inputs)
            .Include(j => j.Outputs)
            .FirstOrDefaultAsync(j => j.SiteId == siteId && j.JobNumber == jobNumber, cancellationToken);

    public async Task<(List<ProcessingJob> Jobs, int TotalCount)> GetBySiteAsync(
        Guid siteId, int page = 1, int pageSize = 50, ProcessingJobStatus? status = null,
        Guid? typeId = null, DateTime? fromDate = null, DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ProcessingJobs
            .Include(j => j.Inputs)
            .Include(j => j.Outputs)
            .Where(j => j.SiteId == siteId);

        if (status.HasValue) query = query.Where(j => j.Status == status.Value);
        if (typeId.HasValue) query = query.Where(j => j.ProcessingJobTypeId == typeId.Value);
        if (fromDate.HasValue)
        {
            var fromDateOnly = DateOnly.FromDateTime(fromDate.Value);
            query = query.Where(j => j.StartDate >= fromDateOnly);
        }
        if (toDate.HasValue)
        {
            var toDateOnly = DateOnly.FromDateTime(toDate.Value);
            query = query.Where(j => j.StartDate <= toDateOnly);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var jobs = await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (jobs, totalCount);
    }

    public async Task<List<ProcessingJob>> GetActiveAsync(Guid siteId, CancellationToken cancellationToken = default)
        => await _context.ProcessingJobs
            .Include(j => j.Inputs)
            .Include(j => j.Outputs)
            .Where(j => j.SiteId == siteId && j.Status == ProcessingJobStatus.InProgress)
            .OrderBy(j => j.StartDate)
            .ToListAsync(cancellationToken);

    public async Task<string> GenerateJobNumberAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow;
        var prefix = $"PJ-{today:yyyyMMdd}";
        var todaysCount = await _context.ProcessingJobs
            .CountAsync(j => j.SiteId == siteId && j.JobNumber.StartsWith(prefix), cancellationToken);
        return $"{prefix}-{(todaysCount + 1):D3}";
    }

    public async Task<ProcessingJob> AddAsync(ProcessingJob job, CancellationToken cancellationToken = default)
    {
        await _context.ProcessingJobs.AddAsync(job, cancellationToken);
        return job;
    }

    public Task<ProcessingJob> UpdateAsync(ProcessingJob job, CancellationToken cancellationToken = default)
    {
        _context.ProcessingJobs.Update(job);
        return Task.FromResult(job);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}



