using Harvestry.Packages.Domain.Entities;
using Harvestry.Packages.Domain.Enums;

namespace Harvestry.Packages.Application.Interfaces;

/// <summary>
/// Repository interface for InventoryMovement entities
/// </summary>
public interface IMovementRepository
{
    Task<InventoryMovement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InventoryMovement?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default);

    Task<(List<InventoryMovement> Movements, int TotalCount)> GetBySiteAsync(
        Guid siteId,
        int page = 1,
        int pageSize = 50,
        MovementType? movementType = null,
        MovementStatus? status = null,
        Guid? packageId = null,
        Guid? locationId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? syncStatus = null,
        CancellationToken cancellationToken = default);

    Task<List<InventoryMovement>> GetByPackageAsync(Guid packageId, CancellationToken cancellationToken = default);
    Task<List<InventoryMovement>> GetByBatchAsync(Guid batchMovementId, CancellationToken cancellationToken = default);
    Task<List<InventoryMovement>> GetRecentAsync(Guid siteId, int count = 20, CancellationToken cancellationToken = default);

    Task<InventoryMovement> AddAsync(InventoryMovement movement, CancellationToken cancellationToken = default);
    Task<List<InventoryMovement>> AddRangeAsync(IEnumerable<InventoryMovement> movements, CancellationToken cancellationToken = default);
    Task<InventoryMovement> UpdateAsync(InventoryMovement movement, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}



