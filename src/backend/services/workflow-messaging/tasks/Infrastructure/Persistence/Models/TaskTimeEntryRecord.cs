using System;

namespace Harvestry.Tasks.Infrastructure.Persistence.Models;

public sealed class TaskTimeEntryRecord
{
    public Guid TaskTimeEntryId { get; set; }
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public string? Notes { get; set; }

    public TaskRecord Task { get; set; } = null!;
}
