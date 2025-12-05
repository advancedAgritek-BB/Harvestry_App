using System;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Domain.Entities;

namespace Harvestry.Tasks.Application.Mappers;

public static class SopMapper
{
    public static SopResponse ToResponse(StandardOperatingProcedure sop)
    {
        if (sop is null)
            throw new ArgumentNullException(nameof(sop));

        return new SopResponse
        {
            Id = sop.Id,
            OrgId = sop.OrgId,
            Title = sop.Title,
            Content = sop.Content,
            Category = sop.Category,
            Version = sop.Version,
            IsActive = sop.IsActive,
            CreatedByUserId = sop.CreatedByUserId,
            CreatedAt = sop.CreatedAt,
            UpdatedAt = sop.UpdatedAt
        };
    }

    public static SopSummaryResponse ToSummary(StandardOperatingProcedure sop)
    {
        if (sop is null)
            throw new ArgumentNullException(nameof(sop));

        return new SopSummaryResponse
        {
            Id = sop.Id,
            Title = sop.Title,
            Category = sop.Category,
            Version = sop.Version,
            IsActive = sop.IsActive,
            UpdatedAt = sop.UpdatedAt
        };
    }
}

