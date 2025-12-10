using Harvestry.Packages.Application.Interfaces;
using Harvestry.Packages.Domain.Entities;
using Harvestry.Packages.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Packages.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for InventoryMovement entities
/// </summary>
public class MovementRepository : IMovementRepository
{
    private readonly PackagesDbContext _context;

    public MovementRepository(PackagesDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryMovement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Movements.FindAsync(new object[] { id }, cancellationToken);

    public async Task<InventoryMovement?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default)
        => await _context.Movements.FirstOrDefaultAsync(m => m.SiteId == siteId && m.Id == id, cancellationToken);

    public async Task<(List<InventoryMovement> Movements, int TotalCount)> GetBySiteAsync(
        Guid siteId, int page = 1, int pageSize = 50,
        MovementType? movementType = null, MovementStatus? status = null,
        Guid? packageId = null, Guid? locationId = null,
        DateTime? fromDate = null, DateTime? toDate = null,
        string? syncStatus = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Movements.Where(m => m.SiteId == siteId);

        if (movementType.HasValue) query = query.Where(m => m.MovementType == movementType.Value);
        if (status.HasValue) query = query.Where(m => m.Status == status.Value);
        if (packageId.HasValue) query = query.Where(m => m.PackageId == packageId.Value);
        if (locationId.HasValue) query = query.Where(m => m.FromLocationId == locationId.Value || m.ToLocationId == locationId.Value);
        if (fromDate.HasValue) query = query.Where(m => m.CreatedAt >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(m => m.CreatedAt <= toDate.Value);
        if (!string.IsNullOrWhiteSpace(syncStatus)) query = query.Where(m => m.SyncStatus == syncStatus);

        var totalCount = await query.CountAsync(cancellationToken);
        var movements = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (movements, totalCount);
    }

    public async Task<List<InventoryMovement>> GetByPackageAsync(Guid packageId, CancellationToken cancellationToken = default)
        => await _context.Movements.Where(m => m.PackageId == packageId).OrderByDescending(m => m.CreatedAt).ToListAsync(cancellationToken);

    public async Task<List<InventoryMovement>> GetByBatchAsync(Guid batchMovementId, CancellationToken cancellationToken = default)
        => await _context.Movements.Where(m => m.BatchMovementId == batchMovementId).OrderBy(m => m.BatchSequence).ToListAsync(cancellationToken);

    public async Task<List<InventoryMovement>> GetRecentAsync(Guid siteId, int count = 20, CancellationToken cancellationToken = default)
        => await _context.Movements.Where(m => m.SiteId == siteId).OrderByDescending(m => m.CreatedAt).Take(count).ToListAsync(cancellationToken);

    public async Task<InventoryMovement> AddAsync(InventoryMovement movement, CancellationToken cancellationToken = default)
    {
        await _context.Movements.AddAsync(movement, cancellationToken);
        return movement;
    }

    public async Task<List<InventoryMovement>> AddRangeAsync(IEnumerable<InventoryMovement> movements, CancellationToken cancellationToken = default)
    {
        var list = movements.ToList();
        await _context.Movements.AddRangeAsync(list, cancellationToken);
        return list;
    }

    public Task<InventoryMovement> UpdateAsync(InventoryMovement movement, CancellationToken cancellationToken = default)
    {
        _context.Movements.Update(movement);
        return Task.FromResult(movement);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}



