using System;
using System.Collections.Generic;
using System.Linq;
using Harvestry.Shared.Kernel.Domain;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.Domain.Entities;

/// <summary>
/// Task Library Item (Template) entity representing saved task templates
/// that can be quickly instantiated. Templates are scoped at the organization level.
/// </summary>
public sealed class TaskLibraryItem : AggregateRoot<Guid>
{
    private readonly HashSet<Guid> _defaultSopIds = new();

    private TaskLibraryItem(
        Guid id,
        Guid orgId,
        string title,
        string? description,
        TaskPriority defaultPriority,
        TaskType taskType,
        string? customTaskType,
        string? defaultAssignedToRole,
        int? defaultDueDaysOffset,
        bool isActive,
        Guid createdByUserId,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt) : base(id)
    {
        if (orgId == Guid.Empty)
            throw new ArgumentException("Organization identifier is required.", nameof(orgId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));
        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by identifier is required.", nameof(createdByUserId));

        OrgId = orgId;
        Title = title.Trim();
        Description = description?.Trim();
        DefaultPriority = defaultPriority == TaskPriority.Undefined ? TaskPriority.Normal : defaultPriority;
        TaskType = taskType == TaskType.Undefined ? TaskType.Custom : taskType;
        CustomTaskType = TaskType == TaskType.Custom ? customTaskType?.Trim() : null;
        DefaultAssignedToRole = string.IsNullOrWhiteSpace(defaultAssignedToRole) ? null : defaultAssignedToRole.Trim();
        DefaultDueDaysOffset = defaultDueDaysOffset;
        IsActive = isActive;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid OrgId { get; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public TaskPriority DefaultPriority { get; private set; }
    public TaskType TaskType { get; private set; }
    public string? CustomTaskType { get; private set; }
    public string? DefaultAssignedToRole { get; private set; }
    public int? DefaultDueDaysOffset { get; private set; }
    public bool IsActive { get; private set; }
    public Guid CreatedByUserId { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<Guid> DefaultSopIds => _defaultSopIds;

    public static TaskLibraryItem Create(
        Guid orgId,
        string title,
        string? description,
        TaskPriority defaultPriority,
        TaskType taskType,
        string? customTaskType,
        string? defaultAssignedToRole,
        int? defaultDueDaysOffset,
        IEnumerable<Guid>? defaultSopIds,
        Guid createdByUserId)
    {
        var now = DateTimeOffset.UtcNow;
        var item = new TaskLibraryItem(
            Guid.NewGuid(),
            orgId,
            title,
            description,
            defaultPriority,
            taskType,
            customTaskType,
            defaultAssignedToRole,
            defaultDueDaysOffset,
            isActive: true,
            createdByUserId,
            now,
            now);

        item.ReplaceDefaultSops(defaultSopIds);
        return item;
    }

    public static TaskLibraryItem FromPersistence(
        Guid id,
        Guid orgId,
        string title,
        string? description,
        TaskPriority defaultPriority,
        TaskType taskType,
        string? customTaskType,
        string? defaultAssignedToRole,
        int? defaultDueDaysOffset,
        bool isActive,
        Guid createdByUserId,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        IEnumerable<Guid>? defaultSopIds)
    {
        var item = new TaskLibraryItem(
            id,
            orgId,
            title,
            description,
            defaultPriority,
            taskType,
            customTaskType,
            defaultAssignedToRole,
            defaultDueDaysOffset,
            isActive,
            createdByUserId,
            createdAt,
            updatedAt);

        item.ReplaceDefaultSops(defaultSopIds);
        return item;
    }

    public void Update(
        string title,
        string? description,
        TaskPriority defaultPriority,
        TaskType taskType,
        string? customTaskType,
        string? defaultAssignedToRole,
        int? defaultDueDaysOffset,
        IEnumerable<Guid>? defaultSopIds)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        Title = title.Trim();
        Description = description?.Trim();
        DefaultPriority = defaultPriority == TaskPriority.Undefined ? TaskPriority.Normal : defaultPriority;
        TaskType = taskType == TaskType.Undefined ? TaskType.Custom : taskType;
        CustomTaskType = TaskType == TaskType.Custom ? customTaskType?.Trim() : null;
        DefaultAssignedToRole = string.IsNullOrWhiteSpace(defaultAssignedToRole) ? null : defaultAssignedToRole.Trim();
        DefaultDueDaysOffset = defaultDueDaysOffset;
        ReplaceDefaultSops(defaultSopIds);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private void ReplaceDefaultSops(IEnumerable<Guid>? sopIds)
    {
        _defaultSopIds.Clear();
        if (sopIds != null)
        {
            foreach (var sopId in sopIds.Where(id => id != Guid.Empty))
            {
                _defaultSopIds.Add(sopId);
            }
        }
    }
}

