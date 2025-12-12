using Harvestry.Sales.Application.Interfaces;
using Harvestry.Sales.Domain.Entities;
using Harvestry.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Sales.Infrastructure.Repositories;

public sealed class SalesOrderLineRepository : ISalesOrderLineRepository
{
    private readonly SalesDbContext _db;

    public SalesOrderLineRepository(SalesDbContext db)
    {
        _db = db;
    }

    public Task<List<SalesOrderLine>> GetByOrderIdAsync(Guid siteId, Guid salesOrderId, CancellationToken cancellationToken = default)
        => _db.SalesOrderLines.Where(l => l.SiteId == siteId && l.SalesOrderId == salesOrderId)
            .OrderBy(l => l.LineNumber)
            .ToListAsync(cancellationToken);

    public Task<SalesOrderLine?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default)
        => _db.SalesOrderLines.FirstOrDefaultAsync(l => l.SiteId == siteId && l.Id == id, cancellationToken);

    public async Task<SalesOrderLine> AddAsync(SalesOrderLine line, CancellationToken cancellationToken = default)
    {
        await _db.SalesOrderLines.AddAsync(line, cancellationToken);
        return line;
    }

    public Task<SalesOrderLine> UpdateAsync(SalesOrderLine line, CancellationToken cancellationToken = default)
    {
        _db.SalesOrderLines.Update(line);
        return Task.FromResult(line);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}

