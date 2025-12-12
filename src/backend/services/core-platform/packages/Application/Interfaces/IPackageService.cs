using Harvestry.Packages.Application.DTOs;

namespace Harvestry.Packages.Application.Interfaces;

/// <summary>
/// Service interface for Package operations
/// </summary>
public interface IPackageService
{
    Task<PackageDto?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default);
    Task<PackageDto?> GetByLabelAsync(Guid siteId, string label, CancellationToken cancellationToken = default);

    Task<PackageListResponse> GetPackagesAsync(
        Guid siteId,
        int page = 1,
        int pageSize = 50,
        string? status = null,
        string? labTestingState = null,
        string? inventoryCategory = null,
        string? itemCategory = null,
        Guid? locationId = null,
        string? search = null,
        bool? onHold = null,
        bool? expiringSoon = null,
        CancellationToken cancellationToken = default);

    Task<PackageDto> CreateAsync(Guid siteId, CreatePackageRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<PackageDto?> UpdateAsync(Guid siteId, Guid id, UpdatePackageRequest request, Guid userId, CancellationToken cancellationToken = default);

    Task<PackageDto?> AdjustAsync(Guid siteId, Guid id, AdjustPackageRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<PackageDto?> MoveAsync(Guid siteId, Guid id, MovePackageRequest request, Guid userId, CancellationToken cancellationToken = default);
    
    Task<PackageDto?> ReserveAsync(Guid siteId, Guid id, ReserveQuantityRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<PackageDto?> UnreserveAsync(Guid siteId, Guid id, decimal quantity, Guid userId, CancellationToken cancellationToken = default);

    Task<List<PackageDto>> SplitAsync(Guid siteId, SplitPackageRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<PackageDto?> MergeAsync(Guid siteId, MergePackagesRequest request, Guid userId, CancellationToken cancellationToken = default);

    Task<PackageDto?> FinishAsync(Guid siteId, Guid id, DateOnly finishDate, Guid userId, CancellationToken cancellationToken = default);
    Task<PackageDto?> UnfinishAsync(Guid siteId, Guid id, Guid userId, CancellationToken cancellationToken = default);

    Task<PackageSummaryStatsDto> GetSummaryStatsAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<PackageLineageDto?> GetLineageAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default);
    Task<List<ExpiringPackageDto>> GetExpiringAsync(Guid siteId, int withinDays = 30, CancellationToken cancellationToken = default);
}




