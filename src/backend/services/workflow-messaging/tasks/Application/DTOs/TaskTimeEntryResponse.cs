using System;

namespace Harvestry.Tasks.Application.DTOs;

public sealed class TaskTimeEntryResponse
{
    public Guid TimeEntryId { get; init; }
    public Guid UserId { get; init; }
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset? EndedAt { get; init; }
    public TimeSpan? Duration { get; init; }
    public string? Notes { get; init; }
}
