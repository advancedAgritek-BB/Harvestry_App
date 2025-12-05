using Harvestry.ProcessingJobs.Domain.Enums;

namespace Harvestry.ProcessingJobs.Domain.Entities;

public sealed partial class ProcessingJob
{
    /// <summary>
    /// Restores a ProcessingJob entity from persistence
    /// </summary>
    public static ProcessingJob Restore(
        Guid id,
        Guid siteId,
        Guid jobTypeId,
        string jobTypeName,
        DateOnly startDate,
        DateOnly? finishDate,
        ProcessingJobStatus status,
        string? notes,
        long? metrcProcessingJobId,
        DateTime? metrcLastSyncAt,
        string? metrcSyncStatus,
        IDictionary<string, object>? metadata,
        DateTime createdAt,
        Guid createdByUserId,
        DateTime updatedAt,
        Guid updatedByUserId)
    {
        var job = new ProcessingJob(id)
        {
            SiteId = siteId,
            JobTypeId = jobTypeId,
            JobTypeName = jobTypeName,
            StartDate = startDate,
            FinishDate = finishDate,
            Status = status,
            Notes = notes,
            MetrcProcessingJobId = metrcProcessingJobId,
            MetrcLastSyncAt = metrcLastSyncAt,
            MetrcSyncStatus = metrcSyncStatus,
            Metadata = metadata != null
                ? new Dictionary<string, object>(metadata)
                : new Dictionary<string, object>(),
            CreatedAt = createdAt,
            CreatedByUserId = createdByUserId,
            UpdatedAt = updatedAt,
            UpdatedByUserId = updatedByUserId
        };

        return job;
    }
}




