using System;
using System.Collections.Generic;
using System.Linq;
using Harvestry.Shared.Kernel.Domain;
using Harvestry.Tasks.Domain.Enums;
using TaskPriorityEnum = Harvestry.Tasks.Domain.Enums.TaskPriority;

namespace Harvestry.Tasks.Domain.Entities;

/// <summary>
/// Task blueprint defines a template for auto-generating tasks based on growth phase,
/// strain, and room type. When a batch/plant enters a matching phase, tasks are created.
/// </summary>
public sealed class TaskBlueprint : AggregateRoot<Guid>
{
    private readonly HashSet<Guid> _requiredSopIds = new();
    private readonly HashSet<Guid> _requiredTrainingIds = new();

    private TaskBlueprint(
        Guid id,
        Guid siteId,
        string title,
        string? description,
        GrowthPhase growthPhase,
        BlueprintRoomType roomType,
        Guid? strainId,
        TaskPriorityEnum priority,
        TimeSpan timeOffset,
        string? assignedToRole,
        bool isActive,
        Guid createdByUserId,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt) : base(id)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site identifier is required.", nameof(siteId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));
        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by identifier is required.", nameof(createdByUserId));

        SiteId = siteId;
        Title = title.Trim();
        Description = description?.Trim();
        GrowthPhase = growthPhase;
        RoomType = roomType;
        StrainId = strainId == Guid.Empty ? null : strainId;
        Priority = priority == TaskPriorityEnum.Undefined ? TaskPriorityEnum.Normal : priority;
        TimeOffset = timeOffset;
        AssignedToRole = string.IsNullOrWhiteSpace(assignedToRole) ? null : assignedToRole.Trim();
        IsActive = isActive;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid SiteId { get; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public GrowthPhase GrowthPhase { get; private set; }
    public BlueprintRoomType RoomType { get; private set; }
    public Guid? StrainId { get; private set; }
    public TaskPriorityEnum Priority { get; private set; }
    public TimeSpan TimeOffset { get; private set; }
    public string? AssignedToRole { get; private set; }
    public bool IsActive { get; private set; }
    public Guid CreatedByUserId { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<Guid> RequiredSopIds => _requiredSopIds;
    public IReadOnlyCollection<Guid> RequiredTrainingIds => _requiredTrainingIds;

    public static TaskBlueprint Create(
        Guid siteId,
        string title,
        string? description,
        GrowthPhase growthPhase,
        BlueprintRoomType roomType,
        Guid? strainId,
        TaskPriorityEnum priority,
        TimeSpan timeOffset,
        string? assignedToRole,
        IEnumerable<Guid>? requiredSopIds,
        IEnumerable<Guid>? requiredTrainingIds,
        Guid createdByUserId)
    {
        var now = DateTimeOffset.UtcNow;
        var blueprint = new TaskBlueprint(
            Guid.NewGuid(),
            siteId,
            title,
            description,
            growthPhase,
            roomType,
            strainId,
            priority,
            timeOffset,
            assignedToRole,
            isActive: true,
            createdByUserId,
            now,
            now);

        blueprint.ReplaceRequiredLists(requiredSopIds, requiredTrainingIds);
        return blueprint;
    }

    public static TaskBlueprint FromPersistence(
        Guid id,
        Guid siteId,
        string title,
        string? description,
        GrowthPhase growthPhase,
        BlueprintRoomType roomType,
        Guid? strainId,
        TaskPriorityEnum priority,
        TimeSpan timeOffset,
        string? assignedToRole,
        bool isActive,
        Guid createdByUserId,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        IEnumerable<Guid>? requiredSopIds,
        IEnumerable<Guid>? requiredTrainingIds)
    {
        var blueprint = new TaskBlueprint(
            id,
            siteId,
            title,
            description,
            growthPhase,
            roomType,
            strainId,
            priority,
            timeOffset,
            assignedToRole,
            isActive,
            createdByUserId,
            createdAt,
            updatedAt);

        blueprint.ReplaceRequiredLists(requiredSopIds, requiredTrainingIds);
        return blueprint;
    }

    public void Update(
        string title,
        string? description,
        GrowthPhase growthPhase,
        BlueprintRoomType roomType,
        Guid? strainId,
        TaskPriorityEnum priority,
        TimeSpan timeOffset,
        string? assignedToRole,
        IEnumerable<Guid>? requiredSopIds,
        IEnumerable<Guid>? requiredTrainingIds)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        Title = title.Trim();
        Description = description?.Trim();
        GrowthPhase = growthPhase;
        RoomType = roomType;
        StrainId = strainId == Guid.Empty ? null : strainId;
        Priority = priority == TaskPriorityEnum.Undefined ? TaskPriorityEnum.Normal : priority;
        TimeOffset = timeOffset;
        AssignedToRole = string.IsNullOrWhiteSpace(assignedToRole) ? null : assignedToRole.Trim();
        ReplaceRequiredLists(requiredSopIds, requiredTrainingIds);
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

    /// <summary>
    /// Checks if this blueprint matches the given criteria.
    /// </summary>
    public bool Matches(GrowthPhase phase, BlueprintRoomType roomType, Guid? strainId)
    {
        if (!IsActive) return false;

        // Phase must match or blueprint accepts any phase
        if (GrowthPhase != GrowthPhase.Any && GrowthPhase != phase)
            return false;

        // Room type must match or blueprint accepts any room type
        if (RoomType != BlueprintRoomType.Any && RoomType != roomType)
            return false;

        // Strain must match if specified on blueprint
        if (StrainId.HasValue && StrainId != strainId)
            return false;

        return true;
    }

    private void ReplaceRequiredLists(IEnumerable<Guid>? sopIds, IEnumerable<Guid>? trainingIds)
    {
        _requiredSopIds.Clear();
        if (sopIds != null)
        {
            foreach (var sopId in sopIds.Where(id => id != Guid.Empty))
            {
                _requiredSopIds.Add(sopId);
            }
        }

        _requiredTrainingIds.Clear();
        if (trainingIds != null)
        {
            foreach (var trainingId in trainingIds.Where(id => id != Guid.Empty))
            {
                _requiredTrainingIds.Add(trainingId);
            }
        }
    }
}

