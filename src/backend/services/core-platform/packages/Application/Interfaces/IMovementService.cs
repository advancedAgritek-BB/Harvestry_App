using Harvestry.Packages.Application.DTOs;

namespace Harvestry.Packages.Application.Interfaces;

/// <summary>
/// Service interface for Movement operations
/// </summary>
public interface IMovementService
{
    Task<MovementDto?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default);

    Task<MovementListResponse> GetMovementsAsync(
        Guid siteId,
        int page = 1,
        int pageSize = 50,
        string? movementType = null,
        string? status = null,
        Guid? packageId = null,
        Guid? locationId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? syncStatus = null,
        CancellationToken cancellationToken = default);

    Task<List<MovementSummaryDto>> GetByPackageAsync(Guid siteId, Guid packageId, CancellationToken cancellationToken = default);
    Task<List<MovementSummaryDto>> GetRecentAsync(Guid siteId, int count = 20, CancellationToken cancellationToken = default);

    Task<List<MovementDto>> CreateBatchAsync(Guid siteId, BatchMovementRequest request, Guid userId, CancellationToken cancellationToken = default);
}



