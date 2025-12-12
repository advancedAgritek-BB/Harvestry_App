using Harvestry.ProcessingJobs.Domain.Entities;
using Harvestry.ProcessingJobs.Domain.Enums;

namespace Harvestry.ProcessingJobs.Application.Interfaces;

/// <summary>
/// Repository interface for ProcessingJob entities
/// </summary>
public interface IProcessingJobRepository
{
    Task<ProcessingJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProcessingJob?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default);
    Task<ProcessingJob?> GetByJobNumberAsync(Guid siteId, string jobNumber, CancellationToken cancellationToken = default);

    Task<(List<ProcessingJob> Jobs, int TotalCount)> GetBySiteAsync(
        Guid siteId,
        int page = 1,
        int pageSize = 50,
        ProcessingJobStatus? status = null,
        Guid? typeId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    Task<List<ProcessingJob>> GetActiveAsync(Guid siteId, CancellationToken cancellationToken = default);

    Task<string> GenerateJobNumberAsync(Guid siteId, CancellationToken cancellationToken = default);

    Task<ProcessingJob> AddAsync(ProcessingJob job, CancellationToken cancellationToken = default);
    Task<ProcessingJob> UpdateAsync(ProcessingJob job, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}




