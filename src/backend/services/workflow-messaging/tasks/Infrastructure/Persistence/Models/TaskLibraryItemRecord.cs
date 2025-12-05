using System;
using System.Collections.Generic;

namespace Harvestry.Tasks.Infrastructure.Persistence.Models;

public sealed class TaskLibraryItemRecord
{
    public Guid TaskLibraryItemId { get; set; }
    public Guid OrgId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int DefaultPriority { get; set; }
    public int TaskType { get; set; }
    public string? CustomTaskType { get; set; }
    public string? DefaultAssignedToRole { get; set; }
    public int? DefaultDueDaysOffset { get; set; }
    public bool IsActive { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<TaskLibraryItemSopRecord> DefaultSops { get; set; } = new List<TaskLibraryItemSopRecord>();
}

public sealed class TaskLibraryItemSopRecord
{
    public Guid TaskLibraryItemSopId { get; set; }
    public Guid TaskLibraryItemId { get; set; }
    public Guid SopId { get; set; }

    public TaskLibraryItemRecord? TaskLibraryItem { get; set; }
}

