using Harvestry.ProcessingJobs.Application.Interfaces;
using Harvestry.ProcessingJobs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.ProcessingJobs.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for ProcessingJobType entities
/// </summary>
public class ProcessingJobTypeRepository : IProcessingJobTypeRepository
{
    private readonly ProcessingJobsDbContext _context;

    public ProcessingJobTypeRepository(ProcessingJobsDbContext context) => _context = context;

    public async Task<ProcessingJobType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.ProcessingJobTypes.FindAsync(new object[] { id }, cancellationToken);

    public async Task<ProcessingJobType?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default)
        => await _context.ProcessingJobTypes.FirstOrDefaultAsync(t => t.SiteId == siteId && t.Id == id, cancellationToken);

    public async Task<List<ProcessingJobType>> GetBySiteAsync(Guid siteId, bool? activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _context.ProcessingJobTypes.Where(t => t.SiteId == siteId);
        if (activeOnly == true) query = query.Where(t => t.IsActive);
        return await query.OrderBy(t => t.Name).ToListAsync(cancellationToken);
    }

    public async Task<ProcessingJobType> AddAsync(ProcessingJobType type, CancellationToken cancellationToken = default)
    {
        await _context.ProcessingJobTypes.AddAsync(type, cancellationToken);
        return type;
    }

    public Task<ProcessingJobType> UpdateAsync(ProcessingJobType type, CancellationToken cancellationToken = default)
    {
        _context.ProcessingJobTypes.Update(type);
        return Task.FromResult(type);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}




