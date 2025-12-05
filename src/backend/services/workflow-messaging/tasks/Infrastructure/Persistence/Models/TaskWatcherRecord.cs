using System;

namespace Harvestry.Tasks.Infrastructure.Persistence.Models;

public sealed class TaskWatcherRecord
{
    public Guid TaskWatcherId { get; set; }
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public TaskRecord Task { get; set; } = null!;
}
