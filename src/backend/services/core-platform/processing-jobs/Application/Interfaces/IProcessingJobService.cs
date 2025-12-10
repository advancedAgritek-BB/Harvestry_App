using Harvestry.ProcessingJobs.Application.DTOs;

namespace Harvestry.ProcessingJobs.Application.Interfaces;

/// <summary>
/// Service interface for ProcessingJob operations
/// </summary>
public interface IProcessingJobService
{
    Task<ProcessingJobDto?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default);

    Task<ProcessingJobListResponse> GetJobsAsync(
        Guid siteId,
        int page = 1,
        int pageSize = 50,
        string? status = null,
        Guid? typeId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    Task<ProcessingJobDto> CreateAsync(Guid siteId, CreateProcessingJobRequest request, Guid userId, CancellationToken cancellationToken = default);

    Task<ProcessingJobDto?> AddInputAsync(Guid siteId, Guid jobId, AddInputRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<ProcessingJobDto?> AddOutputAsync(Guid siteId, Guid jobId, AddOutputRequest request, Guid userId, CancellationToken cancellationToken = default);

    Task<ProcessingJobDto?> FinishAsync(Guid siteId, Guid jobId, FinishJobRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<ProcessingJobDto?> CancelAsync(Guid siteId, Guid jobId, string reason, Guid userId, CancellationToken cancellationToken = default);

    Task<List<ProcessingJobTypeDto>> GetJobTypesAsync(Guid siteId, CancellationToken cancellationToken = default);
}



