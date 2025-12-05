using System;

namespace Harvestry.Tasks.Infrastructure.Persistence.Models;

public sealed class TaskStateHistoryRecord
{
    public Guid TaskStateHistoryId { get; set; }
    public Guid TaskId { get; set; }
    public int FromStatus { get; set; }
    public int ToStatus { get; set; }
    public Guid ChangedBy { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
    public string? Reason { get; set; }

    public TaskRecord Task { get; set; } = null!;
}
