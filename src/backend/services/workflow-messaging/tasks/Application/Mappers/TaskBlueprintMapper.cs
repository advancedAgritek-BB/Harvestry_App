using System;
using System.Linq;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Domain.Entities;

namespace Harvestry.Tasks.Application.Mappers;

public static class TaskBlueprintMapper
{
    public static TaskBlueprintResponse ToResponse(TaskBlueprint blueprint)
    {
        if (blueprint is null)
            throw new ArgumentNullException(nameof(blueprint));

        return new TaskBlueprintResponse
        {
            Id = blueprint.Id,
            SiteId = blueprint.SiteId,
            Title = blueprint.Title,
            Description = blueprint.Description,
            GrowthPhase = blueprint.GrowthPhase,
            RoomType = blueprint.RoomType,
            StrainId = blueprint.StrainId,
            Priority = blueprint.Priority,
            TimeOffsetHours = (int)blueprint.TimeOffset.TotalHours,
            AssignedToRole = blueprint.AssignedToRole,
            IsActive = blueprint.IsActive,
            CreatedByUserId = blueprint.CreatedByUserId,
            CreatedAt = blueprint.CreatedAt,
            UpdatedAt = blueprint.UpdatedAt,
            RequiredSopIds = blueprint.RequiredSopIds.ToArray(),
            RequiredTrainingIds = blueprint.RequiredTrainingIds.ToArray()
        };
    }
}

