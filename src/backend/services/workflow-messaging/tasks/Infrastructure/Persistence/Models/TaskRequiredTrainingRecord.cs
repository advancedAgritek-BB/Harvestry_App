using System;

namespace Harvestry.Tasks.Infrastructure.Persistence.Models;

public sealed class TaskRequiredTrainingRecord
{
    public Guid TaskRequiredTrainingId { get; set; }
    public Guid TaskId { get; set; }
    public Guid TrainingModuleId { get; set; }

    public TaskRecord Task { get; set; } = null!;
}
