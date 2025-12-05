using System;
using System.Collections.Generic;

namespace Harvestry.Tasks.Infrastructure.Persistence.Models;

public sealed class TaskBlueprintRecord
{
    public Guid TaskBlueprintId { get; set; }
    public Guid SiteId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int GrowthPhase { get; set; }
    public int RoomType { get; set; }
    public Guid? StrainId { get; set; }
    public int Priority { get; set; }
    public long TimeOffsetTicks { get; set; }
    public string? AssignedToRole { get; set; }
    public bool IsActive { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<TaskBlueprintRequiredSopRecord> RequiredSops { get; set; } = new List<TaskBlueprintRequiredSopRecord>();
    public ICollection<TaskBlueprintRequiredTrainingRecord> RequiredTrainings { get; set; } = new List<TaskBlueprintRequiredTrainingRecord>();
}

public sealed class TaskBlueprintRequiredSopRecord
{
    public Guid TaskBlueprintRequiredSopId { get; set; }
    public Guid TaskBlueprintId { get; set; }
    public Guid SopId { get; set; }

    public TaskBlueprintRecord? TaskBlueprint { get; set; }
}

public sealed class TaskBlueprintRequiredTrainingRecord
{
    public Guid TaskBlueprintRequiredTrainingId { get; set; }
    public Guid TaskBlueprintId { get; set; }
    public Guid TrainingModuleId { get; set; }

    public TaskBlueprintRecord? TaskBlueprint { get; set; }
}

