using Harvestry.Packages.Application.DTOs;

namespace Harvestry.Packages.Application.Interfaces;

/// <summary>
/// Service interface for Hold management operations
/// </summary>
public interface IHoldService
{
    /// <summary>
    /// Get all packages currently on hold
    /// </summary>
    Task<List<HoldSummaryDto>> GetHoldsAsync(Guid siteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Place a package on hold
    /// </summary>
    Task<PackageDto?> PlaceHoldAsync(Guid siteId, Guid packageId, PlaceHoldRequest request, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approve release (first approval for two-person release)
    /// </summary>
    Task<PackageDto?> ApproveReleaseAsync(Guid siteId, Guid packageId, Guid approverId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Release a package from hold
    /// </summary>
    Task<PackageDto?> ReleaseHoldAsync(Guid siteId, Guid packageId, ReleaseHoldRequest request, Guid userId, CancellationToken cancellationToken = default);
}




