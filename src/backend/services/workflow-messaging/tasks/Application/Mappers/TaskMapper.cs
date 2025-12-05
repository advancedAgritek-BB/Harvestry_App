using System;
using System.Collections.Generic;
using System.Linq;
using Harvestry.Tasks.Application.DTOs;
using DomainTask = Harvestry.Tasks.Domain.Entities.Task;
using DomainTaskDependency = Harvestry.Tasks.Domain.Entities.TaskDependency;

namespace Harvestry.Tasks.Application.Mappers;

public static class TaskMapper
{
    public static TaskResponse ToResponse(DomainTask task)
    {
        if (task is null)
        {
            throw new ArgumentNullException(nameof(task));
        }

        return new TaskResponse
        {
            TaskId = task.Id,
            SiteId = task.SiteId,
            TaskType = task.TaskType,
            CustomTaskType = task.CustomTaskType,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            CreatedByUserId = task.CreatedByUserId,
            AssignedByUserId = task.AssignedByUserId,
            AssignedToUserId = task.AssignedToUserId,
            AssignedToRole = task.AssignedToRole,
            AssignedAt = task.AssignedAt,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            DueDate = task.DueDate,
            StartedAt = task.StartedAt,
            CompletedAt = task.CompletedAt,
            CancelledAt = task.CancelledAt,
            CancellationReason = task.CancellationReason,
            BlockingReason = task.BlockingReason,
            RelatedEntityType = task.RelatedEntityType,
            RelatedEntityId = task.RelatedEntityId,
            RequiredSopIds = task.RequiredSopIds.ToArray(),
            RequiredTrainingIds = task.RequiredTrainingIds.ToArray(),
            Dependencies = MapDependencies(task.Dependencies),
            Watchers = task.Watchers.Select(w => new TaskWatcherResponse
            {
                UserId = w.UserId,
                CreatedAt = w.CreatedAt
            }).ToArray(),
            TimeEntries = task.TimeEntries.Select(t => new TaskTimeEntryResponse
            {
                TimeEntryId = t.Id,
                UserId = t.UserId,
                StartedAt = t.StartedAt,
                EndedAt = t.EndedAt,
                Duration = t.Duration,
                Notes = t.Notes
            }).ToArray(),
            History = task.StateHistory.Select(h => new TaskStateHistoryResponse
            {
                PreviousStatus = h.FromStatus,
                Status = h.ToStatus,
                ChangedBy = h.ChangedBy,
                ChangedAt = h.ChangedAt,
                Reason = h.Reason
            }).ToArray()
        };
    }

    public static TaskWithGatingResponse ToResponse(DomainTask task, TaskGatingStatusResponse gating)
    {
        return new TaskWithGatingResponse
        {
            Task = ToResponse(task),
            Gating = gating
        };
    }

    private static TaskResponse.TaskDependencySummary[] MapDependencies(IEnumerable<DomainTaskDependency>? dependencies)
    {
        if (dependencies is null)
        {
            return Array.Empty<TaskResponse.TaskDependencySummary>();
        }

        return dependencies.Select(d => new TaskResponse.TaskDependencySummary
        {
            DependsOnTaskId = d.DependsOnTaskId,
            IsBlocking = d.IsBlocking,
            DependencyType = d.DependencyType,
            MinimumLag = d.MinimumLag
        }).ToArray();
    }
}
