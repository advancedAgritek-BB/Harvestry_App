using Harvestry.Packages.Application.Interfaces;
using Harvestry.Packages.Domain.Entities;
using Harvestry.Packages.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Packages.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for Package entities
/// </summary>
public class PackageRepository : IPackageRepository
{
    private readonly PackagesDbContext _context;

    public PackageRepository(PackagesDbContext context)
    {
        _context = context;
    }

    public async Task<Package?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Packages.FindAsync(new object[] { id }, cancellationToken);

    public async Task<Package?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default)
        => await _context.Packages.FirstOrDefaultAsync(p => p.SiteId == siteId && p.Id == id, cancellationToken);

    public async Task<Package?> GetByLabelAsync(Guid siteId, string label, CancellationToken cancellationToken = default)
        => await _context.Packages.FirstOrDefaultAsync(p => p.SiteId == siteId && p.PackageLabel.Value == label, cancellationToken);

    public async Task<(List<Package> Packages, int TotalCount)> GetBySiteAsync(
        Guid siteId, int page = 1, int pageSize = 50,
        PackageStatus? status = null, LabTestingState? labTestingState = null,
        InventoryCategory? inventoryCategory = null, string? itemCategory = null,
        Guid? locationId = null, string? search = null, bool? onHold = null,
        bool? expiringSoon = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Packages.Where(p => p.SiteId == siteId);

        if (status.HasValue) query = query.Where(p => p.Status == status.Value);
        if (labTestingState.HasValue) query = query.Where(p => p.LabTestingState == labTestingState.Value);
        if (inventoryCategory.HasValue) query = query.Where(p => p.InventoryCategory == inventoryCategory.Value);
        if (!string.IsNullOrWhiteSpace(itemCategory)) query = query.Where(p => p.ItemCategory == itemCategory);
        if (locationId.HasValue) query = query.Where(p => p.LocationId == locationId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(p =>
                p.PackageLabel.Value.ToLower().Contains(searchLower) ||
                p.ItemName.ToLower().Contains(searchLower));
        }

        if (onHold == true) query = query.Where(p => p.Status == PackageStatus.OnHold);
        if (expiringSoon == true)
        {
            var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
            query = query.Where(p => p.ExpirationDate != null && p.ExpirationDate <= cutoff);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var packages = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (packages, totalCount);
    }

    public async Task<List<Package>> GetByIdsAsync(Guid siteId, IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        => await _context.Packages.Where(p => p.SiteId == siteId && ids.Contains(p.Id)).ToListAsync(cancellationToken);

    public async Task<List<Package>> GetByLocationAsync(Guid siteId, Guid locationId, CancellationToken cancellationToken = default)
        => await _context.Packages.Where(p => p.SiteId == siteId && p.LocationId == locationId && p.Status == PackageStatus.Active)
            .OrderBy(p => p.ItemName).ToListAsync(cancellationToken);

    public async Task<List<Package>> GetExpiringAsync(Guid siteId, int withinDays = 30, CancellationToken cancellationToken = default)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(withinDays));
        return await _context.Packages
            .Where(p => p.SiteId == siteId && p.Status == PackageStatus.Active && p.ExpirationDate != null && p.ExpirationDate <= cutoff)
            .OrderBy(p => p.ExpirationDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Package>> GetOnHoldAsync(Guid siteId, CancellationToken cancellationToken = default)
        => await _context.Packages.Where(p => p.SiteId == siteId && p.Status == PackageStatus.OnHold)
            .OrderBy(p => p.HoldPlacedAt).ToListAsync(cancellationToken);

    public async Task<List<Package>> GetByItemAsync(Guid siteId, Guid itemId, CancellationToken cancellationToken = default)
        => await _context.Packages.Where(p => p.SiteId == siteId && p.ItemId == itemId && p.Status == PackageStatus.Active)
            .OrderByDescending(p => p.PackagedDate).ToListAsync(cancellationToken);

    public async Task<PackageSummaryStats> GetSummaryStatsAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var packages = await _context.Packages.Where(p => p.SiteId == siteId).ToListAsync(cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var weekAhead = today.AddDays(7);
        var monthAhead = today.AddDays(30);

        return new PackageSummaryStats
        {
            TotalPackages = packages.Count,
            ActivePackages = packages.Count(p => p.Status == PackageStatus.Active),
            OnHoldPackages = packages.Count(p => p.Status == PackageStatus.OnHold),
            FinishedPackages = packages.Count(p => p.Status == PackageStatus.Finished),
            TotalQuantity = packages.Where(p => p.Status == PackageStatus.Active).Sum(p => p.Quantity),
            TotalValue = packages.Where(p => p.Status == PackageStatus.Active).Sum(p => p.TotalValue),
            ExpiringInWeek = packages.Count(p => p.Status == PackageStatus.Active && p.ExpirationDate.HasValue && p.ExpirationDate.Value <= weekAhead && p.ExpirationDate.Value > today),
            ExpiringInMonth = packages.Count(p => p.Status == PackageStatus.Active && p.ExpirationDate.HasValue && p.ExpirationDate.Value <= monthAhead && p.ExpirationDate.Value > today),
            PendingLabTest = packages.Count(p => p.LabTestingState == LabTestingState.TestPending),
            FailedLabTest = packages.Count(p => p.LabTestingState == LabTestingState.TestFailed)
        };
    }

    public async Task<List<Package>> GetAncestorsAsync(Guid packageId, CancellationToken cancellationToken = default)
    {
        // Simple implementation - in production would use recursive CTE
        var package = await GetByIdAsync(packageId, cancellationToken);
        if (package == null) return new List<Package>();

        var ancestors = new List<Package>();
        foreach (var label in package.SourcePackageLabels)
        {
            var ancestor = await _context.Packages.FirstOrDefaultAsync(p => p.PackageLabel.Value == label, cancellationToken);
            if (ancestor != null) ancestors.Add(ancestor);
        }
        return ancestors;
    }

    public async Task<List<Package>> GetDescendantsAsync(Guid packageId, CancellationToken cancellationToken = default)
    {
        var package = await GetByIdAsync(packageId, cancellationToken);
        if (package == null) return new List<Package>();

        // Find packages that list this package as a source
        return await _context.Packages
            .Where(p => p.RootAncestorId == packageId || p.SourcePackageLabels.Contains(package.PackageLabel.Value))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> LabelExistsAsync(Guid siteId, string label, CancellationToken cancellationToken = default)
        => await _context.Packages.AnyAsync(p => p.SiteId == siteId && p.PackageLabel.Value == label, cancellationToken);

    public async Task<Package> AddAsync(Package package, CancellationToken cancellationToken = default)
    {
        await _context.Packages.AddAsync(package, cancellationToken);
        return package;
    }

    public Task<Package> UpdateAsync(Package package, CancellationToken cancellationToken = default)
    {
        _context.Packages.Update(package);
        return Task.FromResult(package);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var package = await GetByIdAsync(id, cancellationToken);
        if (package != null) _context.Packages.Remove(package);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}



