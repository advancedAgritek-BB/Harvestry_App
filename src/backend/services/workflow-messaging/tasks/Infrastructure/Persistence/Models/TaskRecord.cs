using System;
using System.Collections.Generic;

namespace Harvestry.Tasks.Infrastructure.Persistence.Models;

public sealed class TaskRecord
{
    public Guid TaskId { get; set; }
    public Guid SiteId { get; set; }
    public int TaskType { get; set; }
    public string? CustomTaskType { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid AssignedByUserId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToRole { get; set; }
    public DateTimeOffset? AssignedAt { get; set; }
    public int Status { get; set; }
    public int Priority { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public string? BlockingReason { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }

    public ICollection<TaskStateHistoryRecord> StateHistory { get; set; } = new List<TaskStateHistoryRecord>();
    public ICollection<TaskDependencyRecord> Dependencies { get; set; } = new List<TaskDependencyRecord>();
    public ICollection<TaskWatcherRecord> Watchers { get; set; } = new List<TaskWatcherRecord>();
    public ICollection<TaskTimeEntryRecord> TimeEntries { get; set; } = new List<TaskTimeEntryRecord>();
    public ICollection<TaskRequiredSopRecord> RequiredSops { get; set; } = new List<TaskRequiredSopRecord>();
    public ICollection<TaskRequiredTrainingRecord> RequiredTrainings { get; set; } = new List<TaskRequiredTrainingRecord>();
}
