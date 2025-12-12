using Harvestry.ProcessingJobs.Domain.Entities;

namespace Harvestry.ProcessingJobs.Application.Interfaces;

/// <summary>
/// Repository interface for ProcessingJobType entities
/// </summary>
public interface IProcessingJobTypeRepository
{
    Task<ProcessingJobType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProcessingJobType?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default);
    Task<List<ProcessingJobType>> GetBySiteAsync(Guid siteId, bool? activeOnly = true, CancellationToken cancellationToken = default);
    Task<ProcessingJobType> AddAsync(ProcessingJobType type, CancellationToken cancellationToken = default);
    Task<ProcessingJobType> UpdateAsync(ProcessingJobType type, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}




