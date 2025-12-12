using Harvestry.Sales.Application.Interfaces;
using Harvestry.Sales.Domain.Entities;
using Harvestry.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Sales.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for ComplianceEvent entities.
/// </summary>
public sealed class ComplianceEventRepository : IComplianceEventRepository
{
    private readonly SalesDbContext _db;

    public ComplianceEventRepository(SalesDbContext db)
    {
        _db = db;
    }

    public async Task<(IReadOnlyList<ComplianceEvent> Items, int TotalCount)> ListAsync(
        Guid siteId,
        string? entityType,
        Guid? entityId,
        string? eventType,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _db.ComplianceEvents.Where(e => e.SiteId == siteId);

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query = query.Where(e => e.EntityType == entityType);
        }

        if (entityId.HasValue)
        {
            query = query.Where(e => e.EntityId == entityId.Value);
        }

        if (!string.IsNullOrWhiteSpace(eventType))
        {
            query = query.Where(e => e.EventType == eventType);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(e => e.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(e => e.CreatedAt <= toDate.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<ComplianceEvent>> GetByEntityAsync(
        Guid siteId,
        string entityType,
        Guid entityId,
        CancellationToken ct = default)
    {
        return await _db.ComplianceEvents
            .Where(e => e.SiteId == siteId && e.EntityType == entityType && e.EntityId == entityId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ComplianceEvent evt, CancellationToken ct = default)
    {
        await _db.ComplianceEvents.AddAsync(evt, ct);
        await _db.SaveChangesAsync(ct);
    }
}
