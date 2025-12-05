using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Domain.Entities;
using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harvestry.Tasks.Infrastructure.Persistence;

public sealed class SopRepository : ISopRepository
{
    private readonly TasksDbContext _dbContext;
    private readonly ILogger<SopRepository> _logger;

    public SopRepository(TasksDbContext dbContext, ILogger<SopRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task AddAsync(StandardOperatingProcedure sop, CancellationToken cancellationToken)
    {
        if (sop is null)
            throw new ArgumentNullException(nameof(sop));

        _logger.LogDebug("Adding SOP {SopId} for org {OrgId}", sop.Id, sop.OrgId);
        var record = ToRecord(sop);
        await _dbContext.StandardOperatingProcedures.AddAsync(record, cancellationToken).ConfigureAwait(false);
    }

    public async Task<StandardOperatingProcedure?> GetByIdAsync(Guid orgId, Guid sopId, CancellationToken cancellationToken)
    {
        var record = await _dbContext.StandardOperatingProcedures
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SopId == sopId && x.OrgId == orgId, cancellationToken)
            .ConfigureAwait(false);

        return record is null ? null : ToDomain(record);
    }

    public async Task<IReadOnlyList<StandardOperatingProcedure>> GetByOrgAsync(
        Guid orgId,
        bool? activeOnly,
        string? category,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.StandardOperatingProcedures
            .AsNoTracking()
            .Where(x => x.OrgId == orgId);

        if (activeOnly == true)
        {
            query = query.Where(x => x.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(x => x.Category == category);
        }

        var records = await query
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return records.Select(ToDomain).ToArray();
    }

    public async Task<IReadOnlyList<StandardOperatingProcedure>> GetByIdsAsync(
        Guid orgId,
        IReadOnlyCollection<Guid> sopIds,
        CancellationToken cancellationToken)
    {
        if (sopIds.Count == 0)
        {
            return Array.Empty<StandardOperatingProcedure>();
        }

        var records = await _dbContext.StandardOperatingProcedures
            .AsNoTracking()
            .Where(x => x.OrgId == orgId && sopIds.Contains(x.SopId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return records.Select(ToDomain).ToArray();
    }

    public async Task UpdateAsync(StandardOperatingProcedure sop, CancellationToken cancellationToken)
    {
        if (sop is null)
            throw new ArgumentNullException(nameof(sop));

        var record = await _dbContext.StandardOperatingProcedures
            .FirstOrDefaultAsync(x => x.SopId == sop.Id && x.OrgId == sop.OrgId, cancellationToken)
            .ConfigureAwait(false);

        if (record is null)
            throw new InvalidOperationException($"SOP {sop.Id} could not be found for update.");

        _logger.LogDebug("Updating SOP {SopId} for org {OrgId}", sop.Id, sop.OrgId);
        ApplyScalarProperties(record, sop);
    }

    public async Task DeleteAsync(Guid orgId, Guid sopId, CancellationToken cancellationToken)
    {
        var record = await _dbContext.StandardOperatingProcedures
            .FirstOrDefaultAsync(x => x.SopId == sopId && x.OrgId == orgId, cancellationToken)
            .ConfigureAwait(false);

        if (record is not null)
        {
            _logger.LogDebug("Deleting SOP {SopId} for org {OrgId}", sopId, orgId);
            _dbContext.StandardOperatingProcedures.Remove(record);
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void ApplyScalarProperties(StandardOperatingProcedureRecord record, StandardOperatingProcedure sop)
    {
        record.Title = sop.Title;
        record.Content = sop.Content;
        record.Category = sop.Category;
        record.Version = sop.Version;
        record.IsActive = sop.IsActive;
        record.UpdatedAt = sop.UpdatedAt;
    }

    private static StandardOperatingProcedure ToDomain(StandardOperatingProcedureRecord record)
    {
        return StandardOperatingProcedure.FromPersistence(
            id: record.SopId,
            orgId: record.OrgId,
            title: record.Title,
            content: record.Content,
            category: record.Category,
            version: record.Version,
            isActive: record.IsActive,
            createdByUserId: record.CreatedByUserId,
            createdAt: record.CreatedAt,
            updatedAt: record.UpdatedAt);
    }

    private static StandardOperatingProcedureRecord ToRecord(StandardOperatingProcedure sop)
    {
        return new StandardOperatingProcedureRecord
        {
            SopId = sop.Id,
            OrgId = sop.OrgId,
            Title = sop.Title,
            Content = sop.Content,
            Category = sop.Category,
            Version = sop.Version,
            IsActive = sop.IsActive,
            CreatedByUserId = sop.CreatedByUserId,
            CreatedAt = sop.CreatedAt,
            UpdatedAt = sop.UpdatedAt
        };
    }
}

