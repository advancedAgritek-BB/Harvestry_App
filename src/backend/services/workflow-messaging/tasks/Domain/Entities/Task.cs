using System;
using System.Collections.Generic;
using System.Linq;
using Harvestry.Shared.Kernel.Domain;
using Harvestry.Tasks.Domain.ValueObjects;
using TaskPriorityEnum = Harvestry.Tasks.Domain.Enums.TaskPriority;
using TaskStatusEnum = Harvestry.Tasks.Domain.Enums.TaskStatus;
using TaskTypeEnum = Harvestry.Tasks.Domain.Enums.TaskType;
using DependencyTypeEnum = Harvestry.Tasks.Domain.Enums.DependencyType;

namespace Harvestry.Tasks.Domain.Entities;

/// <summary>
/// Aggregate root for task lifecycle management.
/// </summary>
public sealed class Task : AggregateRoot<Guid>
{
    private readonly HashSet<Guid> _requiredSopIds = new();
    private readonly HashSet<Guid> _requiredTrainingIds = new();
    private readonly List<TaskStateHistory> _stateHistory = new();
    private readonly List<TaskDependency> _dependencies = new();
    private readonly List<TaskWatcher> _watchers = new();
    private readonly List<TaskTimeEntry> _timeEntries = new();

    private Task(
        Guid id,
        Guid siteId,
        TaskTypeEnum taskType,
        string? customTaskType,
        string title,
        string? description,
        Guid createdByUserId,
        Guid assignedByUserId,
        TaskStatusEnum status,
        TaskPriorityEnum priority,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        Guid? assignedToUserId,
        string? assignedToRole,
        DateTimeOffset? assignedAt,
        DateTimeOffset? dueDate,
        DateTimeOffset? startedAt,
        DateTimeOffset? completedAt,
        DateTimeOffset? cancelledAt,
        string? cancellationReason,
        string? blockingReason,
        string? relatedEntityType,
        Guid? relatedEntityId) : base(id)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site identifier is required.", nameof(siteId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));
        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by identifier is required.", nameof(createdByUserId));
        if (assignedByUserId == Guid.Empty)
            throw new ArgumentException("Assigned by identifier is required.", nameof(assignedByUserId));

        SiteId = siteId;
        TaskType = taskType == TaskTypeEnum.Undefined ? TaskTypeEnum.Custom : taskType;
        CustomTaskType = TaskType == TaskTypeEnum.Custom ? customTaskType?.Trim() : null;
        Title = title.Trim();
        Description = description?.Trim();
        CreatedByUserId = createdByUserId;
        AssignedByUserId = assignedByUserId;
        Status = status == TaskStatusEnum.Undefined ? TaskStatusEnum.Pending : status;
        Priority = priority == TaskPriorityEnum.Undefined ? TaskPriorityEnum.Normal : priority;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        AssignedToUserId = assignedToUserId;
        AssignedToRole = string.IsNullOrWhiteSpace(assignedToRole) ? null : assignedToRole.Trim();
        AssignedAt = assignedAt;
        DueDate = dueDate;
        StartedAt = startedAt;
        CompletedAt = completedAt;
        CancelledAt = cancelledAt;
        CancellationReason = string.IsNullOrWhiteSpace(cancellationReason) ? null : cancellationReason.Trim();
        BlockingReason = string.IsNullOrWhiteSpace(blockingReason) ? null : blockingReason.Trim();
        RelatedEntityType = string.IsNullOrWhiteSpace(relatedEntityType) ? null : relatedEntityType.Trim();
        RelatedEntityId = relatedEntityId;
    }

    public Guid SiteId { get; }
    public TaskTypeEnum TaskType { get; private set; }
    public string? CustomTaskType { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public Guid CreatedByUserId { get; }
    public Guid AssignedByUserId { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public string? AssignedToRole { get; private set; }
    public DateTimeOffset? AssignedAt { get; private set; }
    public TaskStatusEnum Status { get; private set; }
    public TaskPriorityEnum Priority { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DueDate { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public string? BlockingReason { get; private set; }
    public string? RelatedEntityType { get; private set; }
    public Guid? RelatedEntityId { get; private set; }

    public IReadOnlyCollection<Guid> RequiredSopIds => _requiredSopIds;
    public IReadOnlyCollection<Guid> RequiredTrainingIds => _requiredTrainingIds;
    public IReadOnlyCollection<TaskStateHistory> StateHistory => _stateHistory.AsReadOnly();
    public IReadOnlyCollection<TaskDependency> Dependencies => _dependencies.AsReadOnly();
    public IReadOnlyCollection<TaskWatcher> Watchers => _watchers.AsReadOnly();
    public IReadOnlyCollection<TaskTimeEntry> TimeEntries => _timeEntries.AsReadOnly();

    public static Task Create(
        Guid siteId,
        TaskTypeEnum taskType,
        string? customTaskType,
        string title,
        string? description,
        Guid createdByUserId,
        Guid assignedByUserId,
        TaskPriorityEnum priority,
        IEnumerable<Guid>? requiredSopIds,
        IEnumerable<Guid>? requiredTrainingIds,
        string? relatedEntityType,
        Guid? relatedEntityId)
    {
        var now = DateTimeOffset.UtcNow;
        var task = new Task(
            Guid.NewGuid(),
            siteId,
            taskType,
            customTaskType,
            title,
            description,
            createdByUserId,
            assignedByUserId,
            TaskStatusEnum.Pending,
            priority,
            now,
            now,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            relatedEntityType,
            relatedEntityId);

        task.ReplaceRequiredLists(requiredSopIds, requiredTrainingIds);
        task.AddStateHistory(TaskStatusEnum.Pending, TaskStatusEnum.Pending, createdByUserId, "Task created.");
        return task;
    }

    public static Task FromPersistence(
        Guid id,
        Guid siteId,
        TaskTypeEnum taskType,
        string? customTaskType,
        string title,
        string? description,
        Guid createdByUserId,
        Guid assignedByUserId,
        Guid? assignedToUserId,
        string? assignedToRole,
        DateTimeOffset? assignedAt,
        TaskStatusEnum status,
        TaskPriorityEnum priority,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        DateTimeOffset? dueDate,
        DateTimeOffset? startedAt,
        DateTimeOffset? completedAt,
        DateTimeOffset? cancelledAt,
        string? cancellationReason,
        string? blockingReason,
        string? relatedEntityType,
        Guid? relatedEntityId,
        IEnumerable<Guid>? requiredSopIds,
        IEnumerable<Guid>? requiredTrainingIds,
        IEnumerable<TaskStateHistory>? stateHistory,
        IEnumerable<TaskDependency>? dependencies,
        IEnumerable<TaskWatcher>? watchers,
        IEnumerable<TaskTimeEntry>? timeEntries)
    {
        var task = new Task(
            id,
            siteId,
            taskType,
            customTaskType,
            title,
            description,
            createdByUserId,
            assignedByUserId,
            status,
            priority,
            createdAt,
            updatedAt,
            assignedToUserId,
            assignedToRole,
            assignedAt,
            dueDate,
            startedAt,
            completedAt,
            cancelledAt,
            cancellationReason,
            blockingReason,
            relatedEntityType,
            relatedEntityId);

        task.ReplaceRequiredLists(requiredSopIds, requiredTrainingIds);

        if (stateHistory != null)
        {
            task._stateHistory.AddRange(stateHistory.OrderBy(h => h.ChangedAt));
        }

        if (dependencies != null)
        {
            task._dependencies.AddRange(dependencies);
        }

        if (watchers != null)
        {
            task._watchers.AddRange(watchers);
        }

        if (timeEntries != null)
        {
            task._timeEntries.AddRange(timeEntries);
        }

        return task;
    }

    public void Assign(Guid? userId, string? role, Guid assignedBy, DateTimeOffset? assignedAt = null)
    {
        if (assignedBy == Guid.Empty)
            throw new ArgumentException("Assigned by identifier is required.", nameof(assignedBy));

        AssignedToUserId = userId == Guid.Empty ? null : userId;
        AssignedToRole = string.IsNullOrWhiteSpace(role) ? null : role.Trim();
        AssignedAt = assignedAt ?? DateTimeOffset.UtcNow;
        AssignedByUserId = assignedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public TaskGatingResult CheckGating(
        IReadOnlyCollection<Guid> completedSopIds,
        IReadOnlyCollection<Guid> completedTrainingIds)
    {
        var missingSops = _requiredSopIds.Except(completedSopIds ?? Array.Empty<Guid>()).ToArray();
        var missingTraining = _requiredTrainingIds.Except(completedTrainingIds ?? Array.Empty<Guid>()).ToArray();

        if (missingSops.Length == 0 && missingTraining.Length == 0)
        {
            return TaskGatingResult.NotGated();
        }

        var reasons = new List<string>(2);
        if (missingSops.Length > 0)
        {
            reasons.Add("Missing SOP acknowledgement");
        }

        if (missingTraining.Length > 0)
        {
            reasons.Add("Training requirements incomplete");
        }

        return TaskGatingResult.Gated(missingSops, missingTraining, reasons);
    }

    public TaskDependencyResult CheckDependencies(IReadOnlyCollection<Task>? dependentTasks)
    {
        if (_dependencies.Count == 0)
        {
            return TaskDependencyResult.Satisfied();
        }

        var blocking = new List<Guid>();
        var reasons = new List<string>();

        foreach (var dependency in _dependencies)
        {
            var match = dependentTasks?.FirstOrDefault(t => t.Id == dependency.DependsOnTaskId);
            if (match is null)
            {
                // Missing dependent task indicates data integrity issue; throw to surface the problem
                throw new InvalidOperationException(
                    $"Dependent task {dependency.DependsOnTaskId} not found. This indicates a data integrity issue.");
            }

            var isComplete = match.Status == TaskStatusEnum.Completed;
            var hasStarted = match.StartedAt.HasValue || match.Status is TaskStatusEnum.InProgress or TaskStatusEnum.Completed;
            
            switch (dependency.DependencyType)
            {
                case DependencyTypeEnum.FinishToStart:
                case DependencyTypeEnum.FinishToFinish:
                    if (!isComplete)
                    {
                        blocking.Add(match.Id);
                        reasons.Add($"Task {match.Title} must complete first");
                    }
                    break;
                case DependencyTypeEnum.StartToStart:
                    // Require that dependent task has actually started (StartedAt is set OR status indicates it started)
                    if (!hasStarted)
                    {
                        blocking.Add(match.Id);
                        reasons.Add($"Task {match.Title} must start before this task");
                    }
                    break;
                default:
                    if (!isComplete)
                    {
                        blocking.Add(match.Id);
                        reasons.Add($"Task {match.Title} must complete first");
                    }
                    break;
            }
        }

        return blocking.Count == 0
            ? TaskDependencyResult.Satisfied()
            : TaskDependencyResult.Blocked(blocking, reasons);
    }

    public bool CanStart(TaskGatingResult gatingResult, TaskDependencyResult dependencyResult)
    {
        if (Status != TaskStatusEnum.Pending && Status != TaskStatusEnum.Blocked)
        {
            return false;
        }

        if (gatingResult.IsGated || !dependencyResult.IsSatisfied)
        {
            return false;
        }

        return true;
    }

    public void Start(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User identifier is required.", nameof(userId));

        if (Status is TaskStatusEnum.Completed or TaskStatusEnum.Cancelled)
            throw new InvalidOperationException("Cannot start a completed or cancelled task.");

        if (Status == TaskStatusEnum.InProgress)
        {
            return;
        }

        if (!string.IsNullOrEmpty(BlockingReason))
            throw new InvalidOperationException("Task is blocked; clear the blocking reason before starting.");

        var previous = Status;
        Status = TaskStatusEnum.InProgress;
        StartedAt ??= DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddStateHistory(previous, Status, userId);
    }

    public void Block(string reason, Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User identifier is required.", nameof(userId));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Blocking reason is required.", nameof(reason));

        var trimmedReason = reason.Trim();
        if (BlockingReason == trimmedReason && Status == TaskStatusEnum.Blocked)
        {
            return;
        }

        var previous = Status;
        Status = TaskStatusEnum.Blocked;
        BlockingReason = trimmedReason;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddStateHistory(previous, Status, userId, trimmedReason);
    }

    public void Unblock(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User identifier is required.", nameof(userId));

        if (Status != TaskStatusEnum.Blocked)
        {
            BlockingReason = null;
            return;
        }

        var previous = Status;
        Status = TaskStatusEnum.Pending;
        BlockingReason = null;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddStateHistory(previous, Status, userId, "Task unblocked");
    }

    public void Complete(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User identifier is required.", nameof(userId));

        if (Status == TaskStatusEnum.Completed)
        {
            return;
        }

        if (Status is not TaskStatusEnum.InProgress)
            throw new InvalidOperationException("Task must be in progress before it can be completed.");

        var previous = Status;
        Status = TaskStatusEnum.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CompletedAt.Value;
        AddStateHistory(previous, Status, userId);
    }

    public void Cancel(string reason, Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User identifier is required.", nameof(userId));
        if (Status == TaskStatusEnum.Cancelled)
        {
            return;
        }

        if (Status == TaskStatusEnum.Completed)
            throw new InvalidOperationException("Completed tasks cannot be cancelled.");

        var trimmedReason = string.IsNullOrWhiteSpace(reason) ? "Cancelled" : reason.Trim();
        var previous = Status;
        Status = TaskStatusEnum.Cancelled;
        CancellationReason = trimmedReason;
        CancelledAt = DateTimeOffset.UtcNow;
        UpdatedAt = CancelledAt.Value;
        AddStateHistory(previous, Status, userId, trimmedReason);
    }

    public void UpdatePriority(TaskPriorityEnum priority, Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User identifier is required.", nameof(userId));
        if (priority == TaskPriorityEnum.Undefined)
            throw new ArgumentException("Priority cannot be undefined.", nameof(priority));

        if (Priority == priority)
        {
            return;
        }

        Priority = priority;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDueDate(DateTimeOffset? dueDate, Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User identifier is required.", nameof(userId));

        if (dueDate.HasValue && dueDate.Value < CreatedAt)
            throw new ArgumentException("Due date cannot precede creation time.", nameof(dueDate));

        if (DueDate == dueDate)
        {
            return;
        }

        DueDate = dueDate;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsOverdue()
    {
        if (!DueDate.HasValue)
            return false;

        if (Status is TaskStatusEnum.Completed or TaskStatusEnum.Cancelled)
            return false;

        return DateTimeOffset.UtcNow > DueDate.Value;
    }

    public TimeSpan? GetTimeToComplete()
    {
        if (StartedAt.HasValue && CompletedAt.HasValue)
        {
            return CompletedAt.Value - StartedAt.Value;
        }

        return null;
    }

    public void AddWatcher(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User identifier is required.", nameof(userId));

        if (_watchers.Any(w => w.UserId == userId))
        {
            return;
        }

        _watchers.Add(TaskWatcher.Create(Id, userId));
    }

    public void RemoveWatcher(Guid userId)
    {
        var watcher = _watchers.FirstOrDefault(w => w.UserId == userId);
        if (watcher is not null)
        {
            _watchers.Remove(watcher);
        }
    }

    public TaskTimeEntry StartTimeEntry(Guid userId, DateTimeOffset? startedAt = null, string? notes = null)
    {
        var entry = TaskTimeEntry.Create(Id, userId, startedAt, notes);
        _timeEntries.Add(entry);
        return entry;
    }

    public void SetRelatedEntity(string? entityType, Guid? entityId)
    {
        RelatedEntityType = string.IsNullOrWhiteSpace(entityType) ? null : entityType.Trim();
        RelatedEntityId = entityId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetCustomTaskType(string? customType)
    {
        if (TaskType != TaskTypeEnum.Custom)
        {
            return;
        }

        CustomTaskType = string.IsNullOrWhiteSpace(customType) ? null : customType.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDescription(string? description, Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User identifier is required.", nameof(userId));

        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private void AddStateHistory(TaskStatusEnum fromStatus, TaskStatusEnum toStatus, Guid changedBy, string? reason = null)
    {
        if (changedBy == Guid.Empty)
            throw new ArgumentException("Changed by identifier is required.", nameof(changedBy));

        if (_stateHistory.Count > 0 && fromStatus == toStatus)
        {
            return;
        }

        var entry = TaskStateHistory.Create(Id, fromStatus, toStatus, changedBy, reason);
        _stateHistory.Add(entry);
    }

    private void ReplaceRequiredLists(IEnumerable<Guid>? sopIds, IEnumerable<Guid>? trainingIds)
    {
        _requiredSopIds.Clear();
        if (sopIds != null)
        {
            foreach (var sopId in sopIds.Where(id => id != Guid.Empty))
            {
                _requiredSopIds.Add(sopId);
            }
        }

        _requiredTrainingIds.Clear();
        if (trainingIds != null)
        {
            foreach (var trainingId in trainingIds.Where(id => id != Guid.Empty))
            {
                _requiredTrainingIds.Add(trainingId);
            }
        }
    }
}
