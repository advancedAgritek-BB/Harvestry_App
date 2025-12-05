using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Domain.Entities;
using Harvestry.Tasks.Domain.Enums;
using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskPriorityEnum = Harvestry.Tasks.Domain.Enums.TaskPriority;

namespace Harvestry.Tasks.Infrastructure.Persistence;

public sealed class TaskBlueprintRepository : ITaskBlueprintRepository
{
    private readonly TasksDbContext _dbContext;
    private readonly ILogger<TaskBlueprintRepository> _logger;

    public TaskBlueprintRepository(TasksDbContext dbContext, ILogger<TaskBlueprintRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task AddAsync(TaskBlueprint blueprint, CancellationToken cancellationToken)
    {
        if (blueprint is null)
            throw new ArgumentNullException(nameof(blueprint));

        _logger.LogDebug("Adding task blueprint {BlueprintId} for site {SiteId}", blueprint.Id, blueprint.SiteId);
        var record = ToRecord(blueprint);
        await _dbContext.TaskBlueprints.AddAsync(record, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TaskBlueprint?> GetByIdAsync(Guid siteId, Guid blueprintId, CancellationToken cancellationToken)
    {
        var record = await QueryableBlueprints()
            .FirstOrDefaultAsync(x => x.TaskBlueprintId == blueprintId && x.SiteId == siteId, cancellationToken)
            .ConfigureAwait(false);

        return record is null ? null : ToDomain(record);
    }

    public async Task<IReadOnlyList<TaskBlueprint>> GetBySiteAsync(Guid siteId, bool? activeOnly, CancellationToken cancellationToken)
    {
        var query = QueryableBlueprints().Where(x => x.SiteId == siteId);

        if (activeOnly == true)
        {
            query = query.Where(x => x.IsActive);
        }

        var records = await query
            .OrderBy(x => x.Title)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return records.Select(ToDomain).ToArray();
    }

    public async Task<IReadOnlyList<TaskBlueprint>> GetMatchingBlueprintsAsync(
        Guid siteId,
        GrowthPhase growthPhase,
        BlueprintRoomType roomType,
        Guid? strainId,
        CancellationToken cancellationToken)
    {
        var anyPhase = (int)GrowthPhase.Any;
        var anyRoom = (int)BlueprintRoomType.Any;
        var phaseValue = (int)growthPhase;
        var roomValue = (int)roomType;

        var records = await QueryableBlueprints()
            .Where(x => x.SiteId == siteId && x.IsActive)
            .Where(x => x.GrowthPhase == anyPhase || x.GrowthPhase == phaseValue)
            .Where(x => x.RoomType == anyRoom || x.RoomType == roomValue)
            .Where(x => !x.StrainId.HasValue || x.StrainId == strainId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return records.Select(ToDomain).ToArray();
    }

    public async Task UpdateAsync(TaskBlueprint blueprint, CancellationToken cancellationToken)
    {
        if (blueprint is null)
            throw new ArgumentNullException(nameof(blueprint));

        var record = await _dbContext.TaskBlueprints
            .Include(x => x.RequiredSops)
            .Include(x => x.RequiredTrainings)
            .FirstOrDefaultAsync(x => x.TaskBlueprintId == blueprint.Id && x.SiteId == blueprint.SiteId, cancellationToken)
            .ConfigureAwait(false);

        if (record is null)
            throw new InvalidOperationException($"Task blueprint {blueprint.Id} could not be found for update.");

        _logger.LogDebug("Updating task blueprint {BlueprintId} for site {SiteId}", blueprint.Id, blueprint.SiteId);
        ApplyScalarProperties(record, blueprint);
        SyncRequiredSops(record, blueprint);
        SyncRequiredTrainings(record, blueprint);
    }

    public async Task DeleteAsync(Guid siteId, Guid blueprintId, CancellationToken cancellationToken)
    {
        var record = await _dbContext.TaskBlueprints
            .FirstOrDefaultAsync(x => x.TaskBlueprintId == blueprintId && x.SiteId == siteId, cancellationToken)
            .ConfigureAwait(false);

        if (record is not null)
        {
            _logger.LogDebug("Deleting task blueprint {BlueprintId} for site {SiteId}", blueprintId, siteId);
            _dbContext.TaskBlueprints.Remove(record);
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<TaskBlueprintRecord> QueryableBlueprints()
    {
        return _dbContext.TaskBlueprints
            .Include(x => x.RequiredSops)
            .Include(x => x.RequiredTrainings)
            .AsNoTracking();
    }

    private static void ApplyScalarProperties(TaskBlueprintRecord record, TaskBlueprint blueprint)
    {
        record.Title = blueprint.Title;
        record.Description = blueprint.Description;
        record.GrowthPhase = (int)blueprint.GrowthPhase;
        record.RoomType = (int)blueprint.RoomType;
        record.StrainId = blueprint.StrainId;
        record.Priority = (int)blueprint.Priority;
        record.TimeOffsetTicks = blueprint.TimeOffset.Ticks;
        record.AssignedToRole = blueprint.AssignedToRole;
        record.IsActive = blueprint.IsActive;
        record.UpdatedAt = blueprint.UpdatedAt;
    }

    private static void SyncRequiredSops(TaskBlueprintRecord record, TaskBlueprint blueprint)
    {
        record.RequiredSops.Clear();
        foreach (var sopId in blueprint.RequiredSopIds)
        {
            record.RequiredSops.Add(new TaskBlueprintRequiredSopRecord
            {
                TaskBlueprintRequiredSopId = Guid.NewGuid(),
                TaskBlueprintId = blueprint.Id,
                SopId = sopId
            });
        }
    }

    private static void SyncRequiredTrainings(TaskBlueprintRecord record, TaskBlueprint blueprint)
    {
        record.RequiredTrainings.Clear();
        foreach (var moduleId in blueprint.RequiredTrainingIds)
        {
            record.RequiredTrainings.Add(new TaskBlueprintRequiredTrainingRecord
            {
                TaskBlueprintRequiredTrainingId = Guid.NewGuid(),
                TaskBlueprintId = blueprint.Id,
                TrainingModuleId = moduleId
            });
        }
    }

    private static TaskBlueprint ToDomain(TaskBlueprintRecord record)
    {
        return TaskBlueprint.FromPersistence(
            id: record.TaskBlueprintId,
            siteId: record.SiteId,
            title: record.Title,
            description: record.Description,
            growthPhase: (GrowthPhase)record.GrowthPhase,
            roomType: (BlueprintRoomType)record.RoomType,
            strainId: record.StrainId,
            priority: (TaskPriorityEnum)record.Priority,
            timeOffset: TimeSpan.FromTicks(record.TimeOffsetTicks),
            assignedToRole: record.AssignedToRole,
            isActive: record.IsActive,
            createdByUserId: record.CreatedByUserId,
            createdAt: record.CreatedAt,
            updatedAt: record.UpdatedAt,
            requiredSopIds: record.RequiredSops.Select(x => x.SopId),
            requiredTrainingIds: record.RequiredTrainings.Select(x => x.TrainingModuleId));
    }

    private static TaskBlueprintRecord ToRecord(TaskBlueprint blueprint)
    {
        var record = new TaskBlueprintRecord
        {
            TaskBlueprintId = blueprint.Id,
            SiteId = blueprint.SiteId,
            Title = blueprint.Title,
            Description = blueprint.Description,
            GrowthPhase = (int)blueprint.GrowthPhase,
            RoomType = (int)blueprint.RoomType,
            StrainId = blueprint.StrainId,
            Priority = (int)blueprint.Priority,
            TimeOffsetTicks = blueprint.TimeOffset.Ticks,
            AssignedToRole = blueprint.AssignedToRole,
            IsActive = blueprint.IsActive,
            CreatedByUserId = blueprint.CreatedByUserId,
            CreatedAt = blueprint.CreatedAt,
            UpdatedAt = blueprint.UpdatedAt
        };

        foreach (var sopId in blueprint.RequiredSopIds)
        {
            record.RequiredSops.Add(new TaskBlueprintRequiredSopRecord
            {
                TaskBlueprintRequiredSopId = Guid.NewGuid(),
                TaskBlueprintId = blueprint.Id,
                SopId = sopId
            });
        }

        foreach (var trainingId in blueprint.RequiredTrainingIds)
        {
            record.RequiredTrainings.Add(new TaskBlueprintRequiredTrainingRecord
            {
                TaskBlueprintRequiredTrainingId = Guid.NewGuid(),
                TaskBlueprintId = blueprint.Id,
                TrainingModuleId = trainingId
            });
        }

        return record;
    }
}

