using System;

namespace Harvestry.Tasks.Infrastructure.Persistence.Models;

public sealed class TaskDependencyRecord
{
    public Guid TaskDependencyId { get; set; }
    public Guid TaskId { get; set; }
    public Guid DependsOnTaskId { get; set; }
    public int DependencyType { get; set; }
    public bool IsBlocking { get; set; }
    public TimeSpan? MinimumLag { get; set; }

    public TaskRecord Task { get; set; } = null!;
}
