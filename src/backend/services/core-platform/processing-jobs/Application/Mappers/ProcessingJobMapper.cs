using Harvestry.ProcessingJobs.Application.DTOs;
using Harvestry.ProcessingJobs.Domain.Entities;

namespace Harvestry.ProcessingJobs.Application.Mappers;

/// <summary>
/// Extension methods for mapping ProcessingJob entities to DTOs
/// </summary>
public static class ProcessingJobMapper
{
    public static ProcessingJobDto ToDto(this ProcessingJob job)
    {
        var totalInput = job.Inputs.Sum(i => i.Quantity);
        var totalOutput = job.Outputs.Where(o => !o.IsWaste).Sum(o => o.Quantity);

        return new ProcessingJobDto
        {
            Id = job.Id,
            SiteId = job.SiteId,
            JobNumber = job.JobNumber,
            ProcessingJobTypeId = job.ProcessingJobTypeId,
            ProcessingJobTypeName = job.ProcessingJobTypeName ?? "",
            Status = job.Status.ToString(),
            StartDate = job.StartDate,
            EndDate = job.EndDate,
            ExpectedEndDate = job.ExpectedEndDate,
            Inputs = job.Inputs.Select(i => i.ToDto()).ToList(),
            Outputs = job.Outputs.Select(o => o.ToDto()).ToList(),
            TotalInputQuantity = totalInput,
            TotalOutputQuantity = totalOutput,
            YieldPercent = totalInput > 0 ? Math.Round(totalOutput / totalInput * 100, 2) : 0,
            Notes = job.Notes,
            MetrcJobId = job.MetrcJobId,
            MetrcSyncStatus = job.MetrcSyncStatus,
            CreatedAt = job.CreatedAt,
            CreatedByUserId = job.CreatedByUserId,
            UpdatedAt = job.UpdatedAt,
            UpdatedByUserId = job.UpdatedByUserId
        };
    }

    public static ProcessingJobSummaryDto ToSummaryDto(this ProcessingJob job)
    {
        var totalInput = job.Inputs.Sum(i => i.Quantity);
        var totalOutput = job.Outputs.Where(o => !o.IsWaste).Sum(o => o.Quantity);

        return new ProcessingJobSummaryDto
        {
            Id = job.Id,
            JobNumber = job.JobNumber,
            ProcessingJobTypeName = job.ProcessingJobTypeName ?? "",
            Status = job.Status.ToString(),
            StartDate = job.StartDate,
            EndDate = job.EndDate,
            InputCount = job.Inputs.Count,
            OutputCount = job.Outputs.Count,
            TotalInputQuantity = totalInput,
            TotalOutputQuantity = totalOutput,
            YieldPercent = totalInput > 0 ? Math.Round(totalOutput / totalInput * 100, 2) : 0,
            MetrcSyncStatus = job.MetrcSyncStatus
        };
    }

    public static ProcessingJobInputDto ToDto(this ProcessingJobInput input)
    {
        return new ProcessingJobInputDto
        {
            Id = input.Id,
            PackageId = input.PackageId,
            PackageLabel = input.PackageLabel,
            ItemName = input.ItemName ?? "",
            Quantity = input.Quantity,
            UnitOfMeasure = input.UnitOfMeasure,
            UnitCost = input.UnitCost,
            TotalCost = input.Quantity * (input.UnitCost ?? 0)
        };
    }

    public static ProcessingJobOutputDto ToDto(this ProcessingJobOutput output)
    {
        return new ProcessingJobOutputDto
        {
            Id = output.Id,
            PackageId = output.PackageId,
            PackageLabel = output.PackageLabel,
            ItemName = output.ItemName ?? "",
            Quantity = output.Quantity,
            UnitOfMeasure = output.UnitOfMeasure,
            IsWaste = output.IsWaste,
            WasteType = output.WasteType
        };
    }

    public static ProcessingJobTypeDto ToDto(this ProcessingJobType type)
    {
        return new ProcessingJobTypeDto
        {
            Id = type.Id,
            Name = type.Name,
            Description = type.Description,
            IsActive = type.IsActive,
            DefaultOutputCategory = type.DefaultOutputCategory,
            ExpectedYieldPercent = type.ExpectedYieldPercent,
            EstimatedDurationHours = type.EstimatedDurationHours
        };
    }
}



