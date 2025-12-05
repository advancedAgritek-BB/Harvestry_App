using System;

namespace Harvestry.Tasks.Infrastructure.Persistence.Models;

public sealed class TaskRequiredSopRecord
{
    public Guid TaskRequiredSopId { get; set; }
    public Guid TaskId { get; set; }
    public Guid SopId { get; set; }

    public TaskRecord Task { get; set; } = null!;
}
