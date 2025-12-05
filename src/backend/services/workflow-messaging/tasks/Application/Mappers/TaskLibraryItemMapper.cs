using System;
using System.Linq;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Domain.Entities;

namespace Harvestry.Tasks.Application.Mappers;

public static class TaskLibraryItemMapper
{
    public static TaskLibraryItemResponse ToResponse(TaskLibraryItem item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        return new TaskLibraryItemResponse
        {
            Id = item.Id,
            OrgId = item.OrgId,
            Title = item.Title,
            Description = item.Description,
            DefaultPriority = item.DefaultPriority,
            TaskType = item.TaskType,
            CustomTaskType = item.CustomTaskType,
            DefaultAssignedToRole = item.DefaultAssignedToRole,
            DefaultDueDaysOffset = item.DefaultDueDaysOffset,
            IsActive = item.IsActive,
            CreatedByUserId = item.CreatedByUserId,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
            DefaultSopIds = item.DefaultSopIds.ToArray()
        };
    }
}

