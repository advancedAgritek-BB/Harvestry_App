using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DomainTask = Harvestry.Tasks.Domain.Entities.Task;
using DomainTaskDependency = Harvestry.Tasks.Domain.Entities.TaskDependency;
using DomainTaskStateHistory = Harvestry.Tasks.Domain.Entities.TaskStateHistory;
using DomainTaskWatcher = Harvestry.Tasks.Domain.Entities.TaskWatcher;
using DomainTaskTimeEntry = Harvestry.Tasks.Domain.Entities.TaskTimeEntry;
using DependencyTypeEnum = Harvestry.Tasks.Domain.Enums.DependencyType;
using TaskPriorityEnum = Harvestry.Tasks.Domain.Enums.TaskPriority;
using TaskStatusEnum = Harvestry.Tasks.Domain.Enums.TaskStatus;
using TaskTypeEnum = Harvestry.Tasks.Domain.Enums.TaskType;

namespace Harvestry.Tasks.Infrastructure.Persistence;

public sealed class TaskRepository : ITaskRepository
{
    private readonly TasksDbContext _dbContext;
    private readonly ILogger<TaskRepository> _logger;

    public TaskRepository(TasksDbContext dbContext, ILogger<TaskRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task AddAsync(DomainTask task, CancellationToken cancellationToken)
    {
        if (task is null)
        {
            throw new ArgumentNullException(nameof(task));
        }

        _logger.LogDebug("Adding task {TaskId} for site {SiteId}", task.Id, task.SiteId);
        var record = ToRecord(task);
        await _dbContext.Tasks.AddAsync(record, cancellationToken).ConfigureAwait(false);
    }

    public async Task<DomainTask?> GetByIdAsync(Guid siteId, Guid taskId, CancellationToken cancellationToken)
    {
        var record = await QueryableTasks()
            .FirstOrDefaultAsync(x => x.TaskId == taskId && x.SiteId == siteId, cancellationToken)
            .ConfigureAwait(false);

        return record is null ? null : ToDomain(record);
    }

    public async Task<IReadOnlyList<DomainTask>> GetBySiteAsync(Guid siteId, TaskStatusEnum? statusFilter, Guid? assignedToUserId, CancellationToken cancellationToken)
    {
        var query = QueryableTasks().Where(x => x.SiteId == siteId);

        if (statusFilter.HasValue)
        {
            query = query.Where(x => x.Status == (int)statusFilter.Value);
        }

        if (assignedToUserId.HasValue)
        {
            query = query.Where(x => x.AssignedToUserId == assignedToUserId.Value);
        }

        var records = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        return records.Select(ToDomain).ToArray();
    }

    public async Task<IReadOnlyList<DomainTask>> GetOverdueAsync(Guid siteId, DateTimeOffset referenceTime, CancellationToken cancellationToken)
    {
        var records = await QueryableTasks()
            .Where(x => x.SiteId == siteId
                && x.DueDate != null
                && x.DueDate < referenceTime
                && x.Status != (int)TaskStatusEnum.Completed
                && x.Status != (int)TaskStatusEnum.Cancelled)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return records.Select(ToDomain).ToArray();
    }

    public async Task<IReadOnlyList<DomainTask>> GetOverdueAsync(DateTimeOffset referenceTime, int batchSize, CancellationToken cancellationToken)
    {
        var records = await QueryableTasks()
            .Where(x => x.DueDate != null
                && x.DueDate < referenceTime
                && x.Status != (int)TaskStatusEnum.Completed
                && x.Status != (int)TaskStatusEnum.Cancelled)
            .OrderBy(x => x.DueDate)
            .ThenBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return records.Select(ToDomain).ToArray();
    }

    public async Task<IReadOnlyList<DomainTask>> GetBlockedWithDependenciesAsync(int batchSize, CancellationToken cancellationToken)
    {
        var records = await QueryableTasks()
            .Where(x => x.Status == (int)TaskStatusEnum.Blocked && x.Dependencies.Any())
            .OrderBy(x => x.UpdatedAt)
            .ThenBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return records.Select(ToDomain).ToArray();
    }

    public async Task<IReadOnlyList<DomainTask>> GetByIdsAsync(Guid siteId, IReadOnlyCollection<Guid> taskIds, CancellationToken cancellationToken)
    {
        if (taskIds.Count == 0)
        {
            return Array.Empty<DomainTask>();
        }

        var records = await QueryableTasks()
            .Where(x => x.SiteId == siteId && taskIds.Contains(x.TaskId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return records.Select(ToDomain).ToArray();
    }

    public async Task UpdateAsync(DomainTask task, CancellationToken cancellationToken)
    {
        if (task is null)
        {
            throw new ArgumentNullException(nameof(task));
        }

        var record = await _dbContext.Tasks
            .Include(x => x.StateHistory)
            .Include(x => x.Dependencies)
            .Include(x => x.Watchers)
            .Include(x => x.TimeEntries)
            .Include(x => x.RequiredSops)
            .Include(x => x.RequiredTrainings)
            .FirstOrDefaultAsync(x => x.TaskId == task.Id && x.SiteId == task.SiteId, cancellationToken)
            .ConfigureAwait(false);

        if (record is null)
        {
            throw new InvalidOperationException($"Task {task.Id} could not be found for update.");
        }

        _logger.LogDebug("Updating task {TaskId} for site {SiteId}", task.Id, task.SiteId);
        ApplyScalarProperties(record, task);
        SyncStateHistory(record, task);
        SyncDependencies(record, task);
        SyncWatchers(record, task);
        SyncTimeEntries(record, task);
        SyncRequiredSops(record, task);
        SyncRequiredTrainings(record, task);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<TaskRecord> QueryableTasks()
    {
        return _dbContext.Tasks
            .Include(x => x.StateHistory)
            .Include(x => x.Dependencies)
            .Include(x => x.Watchers)
            .Include(x => x.TimeEntries)
            .Include(x => x.RequiredSops)
            .Include(x => x.RequiredTrainings)
            .AsNoTracking();
    }

    private static void ApplyScalarProperties(TaskRecord record, DomainTask task)
    {
        record.SiteId = task.SiteId;
        record.TaskType = (int)task.TaskType;
        record.CustomTaskType = task.CustomTaskType;
        record.Title = task.Title;
        record.Description = task.Description;
        record.CreatedByUserId = task.CreatedByUserId;
        record.AssignedByUserId = task.AssignedByUserId;
        record.AssignedToUserId = task.AssignedToUserId;
        record.AssignedToRole = task.AssignedToRole;
        record.AssignedAt = task.AssignedAt;
        record.Status = (int)task.Status;
        record.Priority = (int)task.Priority;
        record.CreatedAt = task.CreatedAt;
        record.UpdatedAt = task.UpdatedAt;
        record.DueDate = task.DueDate;
        record.StartedAt = task.StartedAt;
        record.CompletedAt = task.CompletedAt;
        record.CancelledAt = task.CancelledAt;
        record.CancellationReason = task.CancellationReason;
        record.BlockingReason = task.BlockingReason;
        record.RelatedEntityType = task.RelatedEntityType;
        record.RelatedEntityId = task.RelatedEntityId;
    }

    private static void SyncStateHistory(TaskRecord record, DomainTask task)
    {
        var domainById = task.StateHistory.ToDictionary(h => h.Id);
        var toRemove = new List<TaskStateHistoryRecord>();

        foreach (var existing in record.StateHistory)
        {
            if (!domainById.TryGetValue(existing.TaskStateHistoryId, out var domain))
            {
                toRemove.Add(existing);
                continue;
            }

            existing.FromStatus = (int)domain.FromStatus;
            existing.ToStatus = (int)domain.ToStatus;
            existing.ChangedBy = domain.ChangedBy;
            existing.ChangedAt = domain.ChangedAt;
            existing.Reason = domain.Reason;
        }

        foreach (var remove in toRemove)
        {
            record.StateHistory.Remove(remove);
        }

        foreach (var domain in task.StateHistory)
        {
            if (record.StateHistory.All(r => r.TaskStateHistoryId != domain.Id))
            {
                record.StateHistory.Add(new TaskStateHistoryRecord
                {
                    TaskStateHistoryId = domain.Id,
                    TaskId = task.Id,
                    FromStatus = (int)domain.FromStatus,
                    ToStatus = (int)domain.ToStatus,
                    ChangedBy = domain.ChangedBy,
                    ChangedAt = domain.ChangedAt,
                    Reason = domain.Reason
                });
            }
        }
    }

    private static void SyncDependencies(TaskRecord record, DomainTask task)
    {
        var domainById = task.Dependencies.ToDictionary(d => d.Id);
        var toRemove = new List<TaskDependencyRecord>();

        foreach (var existing in record.Dependencies)
        {
            if (!domainById.TryGetValue(existing.TaskDependencyId, out var domain))
            {
                toRemove.Add(existing);
                continue;
            }

            existing.DependsOnTaskId = domain.DependsOnTaskId;
            existing.DependencyType = (int)domain.DependencyType;
            existing.IsBlocking = domain.IsBlocking;
            existing.MinimumLag = domain.MinimumLag;
        }

        foreach (var remove in toRemove)
        {
            record.Dependencies.Remove(remove);
        }

        foreach (var domain in task.Dependencies)
        {
            if (record.Dependencies.All(r => r.TaskDependencyId != domain.Id))
            {
                record.Dependencies.Add(new TaskDependencyRecord
                {
                    TaskDependencyId = domain.Id,
                    TaskId = task.Id,
                    DependsOnTaskId = domain.DependsOnTaskId,
                    DependencyType = (int)domain.DependencyType,
                    IsBlocking = domain.IsBlocking,
                    MinimumLag = domain.MinimumLag
                });
            }
        }
    }

    private static void SyncWatchers(TaskRecord record, DomainTask task)
    {
        var domainById = task.Watchers.ToDictionary(w => w.Id);
        var toRemove = new List<TaskWatcherRecord>();

        foreach (var existing in record.Watchers)
        {
            if (!domainById.TryGetValue(existing.TaskWatcherId, out var domain))
            {
                toRemove.Add(existing);
                continue;
            }

            existing.UserId = domain.UserId;
            existing.CreatedAt = domain.CreatedAt;
        }

        foreach (var remove in toRemove)
        {
            record.Watchers.Remove(remove);
        }

        foreach (var domain in task.Watchers)
        {
            if (record.Watchers.All(r => r.TaskWatcherId != domain.Id))
            {
                record.Watchers.Add(new TaskWatcherRecord
                {
                    TaskWatcherId = domain.Id,
                    TaskId = task.Id,
                    UserId = domain.UserId,
                    CreatedAt = domain.CreatedAt
                });
            }
        }
    }

    private static void SyncTimeEntries(TaskRecord record, DomainTask task)
    {
        var domainById = task.TimeEntries.ToDictionary(t => t.Id);
        var toRemove = new List<TaskTimeEntryRecord>();

        foreach (var existing in record.TimeEntries)
        {
            if (!domainById.TryGetValue(existing.TaskTimeEntryId, out var domain))
            {
                toRemove.Add(existing);
                continue;
            }

            existing.UserId = domain.UserId;
            existing.StartedAt = domain.StartedAt;
            existing.EndedAt = domain.EndedAt;
            existing.Notes = domain.Notes;
        }

        foreach (var remove in toRemove)
        {
            record.TimeEntries.Remove(remove);
        }

        foreach (var domain in task.TimeEntries)
        {
            if (record.TimeEntries.All(r => r.TaskTimeEntryId != domain.Id))
            {
                record.TimeEntries.Add(new TaskTimeEntryRecord
                {
                    TaskTimeEntryId = domain.Id,
                    TaskId = task.Id,
                    UserId = domain.UserId,
                    StartedAt = domain.StartedAt,
                    EndedAt = domain.EndedAt,
                    Notes = domain.Notes
                });
            }
        }
    }

    private static void SyncRequiredSops(TaskRecord record, DomainTask task)
    {
        record.RequiredSops.Clear();

        foreach (var sopId in task.RequiredSopIds)
        {
            record.RequiredSops.Add(new TaskRequiredSopRecord
            {
                TaskRequiredSopId = Guid.NewGuid(),
                TaskId = task.Id,
                SopId = sopId
            });
        }
    }

    private static void SyncRequiredTrainings(TaskRecord record, DomainTask task)
    {
        record.RequiredTrainings.Clear();

        foreach (var moduleId in task.RequiredTrainingIds)
        {
            record.RequiredTrainings.Add(new TaskRequiredTrainingRecord
            {
                TaskRequiredTrainingId = Guid.NewGuid(),
                TaskId = task.Id,
                TrainingModuleId = moduleId
            });
        }
    }

    private static DomainTask ToDomain(TaskRecord record)
    {
        var stateHistory = record.StateHistory
            .OrderBy(x => x.ChangedAt)
            .Select(x => DomainTaskStateHistory.FromPersistence(
                x.TaskStateHistoryId,
                record.TaskId,
                (TaskStatusEnum)x.FromStatus,
                (TaskStatusEnum)x.ToStatus,
                x.ChangedBy,
                x.ChangedAt,
                x.Reason))
            .ToList();

        var dependencies = record.Dependencies
            .Select(x => DomainTaskDependency.FromPersistence(
                x.TaskDependencyId,
                record.TaskId,
                x.DependsOnTaskId,
                (DependencyTypeEnum)x.DependencyType,
                x.IsBlocking,
                x.MinimumLag))
            .ToList();

        var watchers = record.Watchers
            .Select(x => DomainTaskWatcher.FromPersistence(
                x.TaskWatcherId,
                record.TaskId,
                x.UserId,
                x.CreatedAt))
            .ToList();

        var timeEntries = record.TimeEntries
            .Select(x => DomainTaskTimeEntry.FromPersistence(
                x.TaskTimeEntryId,
                record.TaskId,
                x.UserId,
                x.StartedAt,
                x.EndedAt,
                x.Notes))
            .ToList();

        return DomainTask.FromPersistence(
            id: record.TaskId,
            siteId: record.SiteId,
            taskType: (TaskTypeEnum)record.TaskType,
            customTaskType: record.CustomTaskType,
            title: record.Title,
            description: record.Description,
            createdByUserId: record.CreatedByUserId,
            assignedByUserId: record.AssignedByUserId,
            assignedToUserId: record.AssignedToUserId,
            assignedToRole: record.AssignedToRole,
            assignedAt: record.AssignedAt,
            status: (TaskStatusEnum)record.Status,
            priority: (TaskPriorityEnum)record.Priority,
            createdAt: record.CreatedAt,
            updatedAt: record.UpdatedAt,
            dueDate: record.DueDate,
            startedAt: record.StartedAt,
            completedAt: record.CompletedAt,
            cancelledAt: record.CancelledAt,
            cancellationReason: record.CancellationReason,
            blockingReason: record.BlockingReason,
            relatedEntityType: record.RelatedEntityType,
            relatedEntityId: record.RelatedEntityId,
            requiredSopIds: record.RequiredSops.Select(x => x.SopId),
            requiredTrainingIds: record.RequiredTrainings.Select(x => x.TrainingModuleId),
            stateHistory: stateHistory,
            dependencies: dependencies,
            watchers: watchers,
            timeEntries: timeEntries);
    }

    private static TaskRecord ToRecord(DomainTask task)
    {
        var record = new TaskRecord
        {
            TaskId = task.Id,
            SiteId = task.SiteId,
            TaskType = (int)task.TaskType,
            CustomTaskType = task.CustomTaskType,
            Title = task.Title,
            Description = task.Description,
            CreatedByUserId = task.CreatedByUserId,
            AssignedByUserId = task.AssignedByUserId,
            AssignedToUserId = task.AssignedToUserId,
            AssignedToRole = task.AssignedToRole,
            AssignedAt = task.AssignedAt,
            Status = (int)task.Status,
            Priority = (int)task.Priority,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            DueDate = task.DueDate,
            StartedAt = task.StartedAt,
            CompletedAt = task.CompletedAt,
            CancelledAt = task.CancelledAt,
            CancellationReason = task.CancellationReason,
            BlockingReason = task.BlockingReason,
            RelatedEntityType = task.RelatedEntityType,
            RelatedEntityId = task.RelatedEntityId
        };

        foreach (var history in task.StateHistory)
        {
            record.StateHistory.Add(new TaskStateHistoryRecord
            {
                TaskStateHistoryId = history.Id,
                TaskId = task.Id,
                FromStatus = (int)history.FromStatus,
                ToStatus = (int)history.ToStatus,
                ChangedBy = history.ChangedBy,
                ChangedAt = history.ChangedAt,
                Reason = history.Reason
            });
        }

        foreach (var dependency in task.Dependencies)
        {
            record.Dependencies.Add(new TaskDependencyRecord
            {
                TaskDependencyId = dependency.Id,
                TaskId = task.Id,
                DependsOnTaskId = dependency.DependsOnTaskId,
                DependencyType = (int)dependency.DependencyType,
                IsBlocking = dependency.IsBlocking,
                MinimumLag = dependency.MinimumLag
            });
        }

        foreach (var watcher in task.Watchers)
        {
            record.Watchers.Add(new TaskWatcherRecord
            {
                TaskWatcherId = watcher.Id,
                TaskId = task.Id,
                UserId = watcher.UserId,
                CreatedAt = watcher.CreatedAt
            });
        }

        foreach (var timeEntry in task.TimeEntries)
        {
            record.TimeEntries.Add(new TaskTimeEntryRecord
            {
                TaskTimeEntryId = timeEntry.Id,
                TaskId = task.Id,
                UserId = timeEntry.UserId,
                StartedAt = timeEntry.StartedAt,
                EndedAt = timeEntry.EndedAt,
                Notes = timeEntry.Notes
            });
        }

        foreach (var sopId in task.RequiredSopIds)
        {
            record.RequiredSops.Add(new TaskRequiredSopRecord
            {
                TaskRequiredSopId = Guid.NewGuid(),
                TaskId = task.Id,
                SopId = sopId
            });
        }

        foreach (var trainingId in task.RequiredTrainingIds)
        {
            record.RequiredTrainings.Add(new TaskRequiredTrainingRecord
            {
                TaskRequiredTrainingId = Guid.NewGuid(),
                TaskId = task.Id,
                TrainingModuleId = trainingId
            });
        }

        return record;
    }
}
