using Harvestry.ProcessingJobs.Application.DTOs;
using Harvestry.ProcessingJobs.Application.Interfaces;
using Harvestry.ProcessingJobs.Application.Mappers;
using Harvestry.ProcessingJobs.Domain.Entities;
using Harvestry.ProcessingJobs.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Harvestry.ProcessingJobs.Application.Services;

/// <summary>
/// Service implementation for ProcessingJob operations
/// </summary>
public class ProcessingJobService : IProcessingJobService
{
    private readonly IProcessingJobRepository _jobRepository;
    private readonly IProcessingJobTypeRepository _typeRepository;
    private readonly ILogger<ProcessingJobService> _logger;

    public ProcessingJobService(
        IProcessingJobRepository jobRepository,
        IProcessingJobTypeRepository typeRepository,
        ILogger<ProcessingJobService> logger)
    {
        _jobRepository = jobRepository;
        _typeRepository = typeRepository;
        _logger = logger;
    }

    public async Task<ProcessingJobDto?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(siteId, id, cancellationToken);
        return job?.ToDto();
    }

    public async Task<ProcessingJobListResponse> GetJobsAsync(
        Guid siteId, int page = 1, int pageSize = 50, string? status = null, Guid? typeId = null,
        DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        ProcessingJobStatus? statusEnum = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ProcessingJobStatus>(status, true, out var s))
            statusEnum = s;

        var (jobs, totalCount) = await _jobRepository.GetBySiteAsync(
            siteId, page, pageSize, statusEnum, typeId, fromDate, toDate, cancellationToken);

        return new ProcessingJobListResponse
        {
            Jobs = jobs.Select(j => j.ToSummaryDto()).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ProcessingJobDto> CreateAsync(Guid siteId, CreateProcessingJobRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var jobType = await _typeRepository.GetByIdAsync(siteId, request.ProcessingJobTypeId, cancellationToken);
        if (jobType == null)
            throw new InvalidOperationException($"Processing job type not found: {request.ProcessingJobTypeId}");

        var jobNumber = await _jobRepository.GenerateJobNumberAsync(siteId, cancellationToken);

        var job = ProcessingJob.Create(
            siteId,
            jobNumber,
            request.ProcessingJobTypeId,
            jobType.Name,
            request.StartDate,
            userId);

        if (request.ExpectedEndDate.HasValue)
            job.SetExpectedEndDate(request.ExpectedEndDate.Value);

        if (!string.IsNullOrWhiteSpace(request.Notes))
            job.AddNote(request.Notes);

        var created = await _jobRepository.AddAsync(job, cancellationToken);
        await _jobRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created processing job {JobId} ({JobNumber}) for site {SiteId}", created.Id, jobNumber, siteId);

        return created.ToDto();
    }

    public async Task<ProcessingJobDto?> AddInputAsync(Guid siteId, Guid jobId, AddInputRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(siteId, jobId, cancellationToken);
        if (job == null) return null;

        // Note: In a real implementation, we would fetch the package from PackageRepository
        // to get the label, item name, unit cost, etc.
        job.AddInput(
            request.PackageId,
            "PKG-LABEL", // Would come from package lookup
            "Item Name", // Would come from package lookup
            request.Quantity,
            "g", // Would come from package lookup
            null, // Unit cost from package
            userId);

        await _jobRepository.UpdateAsync(job, cancellationToken);
        await _jobRepository.SaveChangesAsync(cancellationToken);

        return job.ToDto();
    }

    public async Task<ProcessingJobDto?> AddOutputAsync(Guid siteId, Guid jobId, AddOutputRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(siteId, jobId, cancellationToken);
        if (job == null) return null;

        // Note: In a real implementation, this would create a new Package via PackageService
        var outputPackageId = Guid.NewGuid(); // Would be the created package ID

        job.AddOutput(
            outputPackageId,
            request.PackageLabel,
            request.ItemName,
            request.Quantity,
            request.UnitOfMeasure,
            request.IsWaste,
            request.WasteType,
            userId);

        await _jobRepository.UpdateAsync(job, cancellationToken);
        await _jobRepository.SaveChangesAsync(cancellationToken);

        return job.ToDto();
    }

    public async Task<ProcessingJobDto?> FinishAsync(Guid siteId, Guid jobId, FinishJobRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(siteId, jobId, cancellationToken);
        if (job == null) return null;

        job.Finish(request.EndDate, userId);

        if (!string.IsNullOrWhiteSpace(request.Notes))
            job.AddNote(request.Notes);

        await _jobRepository.UpdateAsync(job, cancellationToken);
        await _jobRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Finished processing job {JobId}", jobId);

        return job.ToDto();
    }

    public async Task<ProcessingJobDto?> CancelAsync(Guid siteId, Guid jobId, string reason, Guid userId, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(siteId, jobId, cancellationToken);
        if (job == null) return null;

        job.Cancel(reason, userId);

        await _jobRepository.UpdateAsync(job, cancellationToken);
        await _jobRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cancelled processing job {JobId}: {Reason}", jobId, reason);

        return job.ToDto();
    }

    public async Task<List<ProcessingJobTypeDto>> GetJobTypesAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var types = await _typeRepository.GetBySiteAsync(siteId, true, cancellationToken);
        return types.Select(t => t.ToDto()).ToList();
    }
}



