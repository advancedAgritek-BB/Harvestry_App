using System;
using TaskPriorityEnum = Harvestry.Tasks.Domain.Enums.TaskPriority;

namespace Harvestry.Tasks.Application.DTOs;

public sealed class UpdateTaskRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public TaskPriorityEnum? Priority { get; init; }
    public DateTimeOffset? DueDate { get; init; }
    public string? RelatedEntityType { get; init; }
    public Guid? RelatedEntityId { get; init; }
}
