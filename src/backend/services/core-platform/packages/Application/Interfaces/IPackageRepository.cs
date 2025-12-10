using Harvestry.Packages.Domain.Entities;
using Harvestry.Packages.Domain.Enums;

namespace Harvestry.Packages.Application.Interfaces;

/// <summary>
/// Repository interface for Package entities
/// </summary>
public interface IPackageRepository
{
    Task<Package?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Package?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default);
    Task<Package?> GetByLabelAsync(Guid siteId, string label, CancellationToken cancellationToken = default);
    
    Task<(List<Package> Packages, int TotalCount)> GetBySiteAsync(
        Guid siteId,
        int page = 1,
        int pageSize = 50,
        PackageStatus? status = null,
        LabTestingState? labTestingState = null,
        InventoryCategory? inventoryCategory = null,
        string? itemCategory = null,
        Guid? locationId = null,
        string? search = null,
        bool? onHold = null,
        bool? expiringSoon = null,
        CancellationToken cancellationToken = default);

    Task<List<Package>> GetByIdsAsync(Guid siteId, IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<List<Package>> GetByLocationAsync(Guid siteId, Guid locationId, CancellationToken cancellationToken = default);
    Task<List<Package>> GetExpiringAsync(Guid siteId, int withinDays = 30, CancellationToken cancellationToken = default);
    Task<List<Package>> GetOnHoldAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<List<Package>> GetByItemAsync(Guid siteId, Guid itemId, CancellationToken cancellationToken = default);

    Task<PackageSummaryStats> GetSummaryStatsAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<List<Package>> GetAncestorsAsync(Guid packageId, CancellationToken cancellationToken = default);
    Task<List<Package>> GetDescendantsAsync(Guid packageId, CancellationToken cancellationToken = default);

    Task<bool> LabelExistsAsync(Guid siteId, string label, CancellationToken cancellationToken = default);

    Task<Package> AddAsync(Package package, CancellationToken cancellationToken = default);
    Task<Package> UpdateAsync(Package package, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public record PackageSummaryStats
{
    public int TotalPackages { get; init; }
    public int ActivePackages { get; init; }
    public int OnHoldPackages { get; init; }
    public int FinishedPackages { get; init; }
    public decimal TotalQuantity { get; init; }
    public decimal TotalValue { get; init; }
    public int ExpiringInWeek { get; init; }
    public int ExpiringInMonth { get; init; }
    public int PendingLabTest { get; init; }
    public int FailedLabTest { get; init; }
}



