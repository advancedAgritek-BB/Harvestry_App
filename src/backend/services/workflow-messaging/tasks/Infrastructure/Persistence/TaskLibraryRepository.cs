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

namespace Harvestry.Tasks.Infrastructure.Persistence;

public sealed class TaskLibraryRepository : ITaskLibraryRepository
{
    private readonly TasksDbContext _dbContext;
    private readonly ILogger<TaskLibraryRepository> _logger;

    public TaskLibraryRepository(TasksDbContext dbContext, ILogger<TaskLibraryRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task AddAsync(TaskLibraryItem item, CancellationToken cancellationToken)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        _logger.LogDebug("Adding task library item {ItemId} for org {OrgId}", item.Id, item.OrgId);
        var record = ToRecord(item);
        await _dbContext.TaskLibraryItems.AddAsync(record, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TaskLibraryItem?> GetByIdAsync(Guid orgId, Guid itemId, CancellationToken cancellationToken)
    {
        var record = await QueryableItems()
            .FirstOrDefaultAsync(x => x.TaskLibraryItemId == itemId && x.OrgId == orgId, cancellationToken)
            .ConfigureAwait(false);

        return record is null ? null : ToDomain(record);
    }

    public async Task<IReadOnlyList<TaskLibraryItem>> GetByOrgAsync(
        Guid orgId,
        bool? activeOnly,
        CancellationToken cancellationToken)
    {
        var query = QueryableItems().Where(x => x.OrgId == orgId);

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

    public async Task UpdateAsync(TaskLibraryItem item, CancellationToken cancellationToken)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        var record = await _dbContext.TaskLibraryItems
            .Include(x => x.DefaultSops)
            .FirstOrDefaultAsync(x => x.TaskLibraryItemId == item.Id && x.OrgId == item.OrgId, cancellationToken)
            .ConfigureAwait(false);

        if (record is null)
            throw new InvalidOperationException($"Task library item {item.Id} could not be found for update.");

        _logger.LogDebug("Updating task library item {ItemId} for org {OrgId}", item.Id, item.OrgId);
        ApplyScalarProperties(record, item);
        SyncDefaultSops(record, item);
    }

    public async Task DeleteAsync(Guid orgId, Guid itemId, CancellationToken cancellationToken)
    {
        var record = await _dbContext.TaskLibraryItems
            .FirstOrDefaultAsync(x => x.TaskLibraryItemId == itemId && x.OrgId == orgId, cancellationToken)
            .ConfigureAwait(false);

        if (record is not null)
        {
            _logger.LogDebug("Deleting task library item {ItemId} for org {OrgId}", itemId, orgId);
            _dbContext.TaskLibraryItems.Remove(record);
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<TaskLibraryItemRecord> QueryableItems()
    {
        return _dbContext.TaskLibraryItems
            .Include(x => x.DefaultSops)
            .AsNoTracking();
    }

    private static void ApplyScalarProperties(TaskLibraryItemRecord record, TaskLibraryItem item)
    {
        record.Title = item.Title;
        record.Description = item.Description;
        record.DefaultPriority = (int)item.DefaultPriority;
        record.TaskType = (int)item.TaskType;
        record.CustomTaskType = item.CustomTaskType;
        record.DefaultAssignedToRole = item.DefaultAssignedToRole;
        record.DefaultDueDaysOffset = item.DefaultDueDaysOffset;
        record.IsActive = item.IsActive;
        record.UpdatedAt = item.UpdatedAt;
    }

    private static void SyncDefaultSops(TaskLibraryItemRecord record, TaskLibraryItem item)
    {
        record.DefaultSops.Clear();
        foreach (var sopId in item.DefaultSopIds)
        {
            record.DefaultSops.Add(new TaskLibraryItemSopRecord
            {
                TaskLibraryItemSopId = Guid.NewGuid(),
                TaskLibraryItemId = item.Id,
                SopId = sopId
            });
        }
    }

    private static TaskLibraryItem ToDomain(TaskLibraryItemRecord record)
    {
        return TaskLibraryItem.FromPersistence(
            id: record.TaskLibraryItemId,
            orgId: record.OrgId,
            title: record.Title,
            description: record.Description,
            defaultPriority: (TaskPriority)record.DefaultPriority,
            taskType: (TaskType)record.TaskType,
            customTaskType: record.CustomTaskType,
            defaultAssignedToRole: record.DefaultAssignedToRole,
            defaultDueDaysOffset: record.DefaultDueDaysOffset,
            isActive: record.IsActive,
            createdByUserId: record.CreatedByUserId,
            createdAt: record.CreatedAt,
            updatedAt: record.UpdatedAt,
            defaultSopIds: record.DefaultSops.Select(x => x.SopId));
    }

    private static TaskLibraryItemRecord ToRecord(TaskLibraryItem item)
    {
        var record = new TaskLibraryItemRecord
        {
            TaskLibraryItemId = item.Id,
            OrgId = item.OrgId,
            Title = item.Title,
            Description = item.Description,
            DefaultPriority = (int)item.DefaultPriority,
            TaskType = (int)item.TaskType,
            CustomTaskType = item.CustomTaskType,
            DefaultAssignedToRole = item.DefaultAssignedToRole,
            DefaultDueDaysOffset = item.DefaultDueDaysOffset,
            IsActive = item.IsActive,
            CreatedByUserId = item.CreatedByUserId,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };

        foreach (var sopId in item.DefaultSopIds)
        {
            record.DefaultSops.Add(new TaskLibraryItemSopRecord
            {
                TaskLibraryItemSopId = Guid.NewGuid(),
                TaskLibraryItemId = item.Id,
                SopId = sopId
            });
        }

        return record;
    }
}

