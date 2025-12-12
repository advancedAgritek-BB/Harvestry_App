using Harvestry.Packages.Application.DTOs;
using Harvestry.Packages.Application.Interfaces;
using Harvestry.Packages.Application.Mappers;
using Harvestry.Packages.Domain.Entities;
using Harvestry.Packages.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Harvestry.Packages.Application.Services;

/// <summary>
/// Service implementation for Movement operations
/// </summary>
public class MovementService : IMovementService
{
    private readonly IMovementRepository _movementRepository;
    private readonly IPackageRepository _packageRepository;
    private readonly ILogger<MovementService> _logger;

    public MovementService(
        IMovementRepository movementRepository,
        IPackageRepository packageRepository,
        ILogger<MovementService> logger)
    {
        _movementRepository = movementRepository;
        _packageRepository = packageRepository;
        _logger = logger;
    }

    public async Task<MovementDto?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default)
    {
        var movement = await _movementRepository.GetByIdAsync(siteId, id, cancellationToken);
        return movement?.ToDto();
    }

    public async Task<MovementListResponse> GetMovementsAsync(
        Guid siteId, int page = 1, int pageSize = 50,
        string? movementType = null, string? status = null, Guid? packageId = null,
        Guid? locationId = null, DateTime? fromDate = null, DateTime? toDate = null,
        string? syncStatus = null, CancellationToken cancellationToken = default)
    {
        MovementType? typeEnum = null;
        if (!string.IsNullOrWhiteSpace(movementType) && Enum.TryParse<MovementType>(movementType, true, out var mt))
            typeEnum = mt;

        MovementStatus? statusEnum = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<MovementStatus>(status, true, out var ms))
            statusEnum = ms;

        var (movements, totalCount) = await _movementRepository.GetBySiteAsync(
            siteId, page, pageSize, typeEnum, statusEnum, packageId, locationId, fromDate, toDate, syncStatus, cancellationToken);

        return new MovementListResponse
        {
            Movements = movements.Select(m => m.ToSummaryDto()).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<MovementSummaryDto>> GetByPackageAsync(Guid siteId, Guid packageId, CancellationToken cancellationToken = default)
    {
        var movements = await _movementRepository.GetByPackageAsync(packageId, cancellationToken);
        return movements.Select(m => m.ToSummaryDto()).ToList();
    }

    public async Task<List<MovementSummaryDto>> GetRecentAsync(Guid siteId, int count = 20, CancellationToken cancellationToken = default)
    {
        var movements = await _movementRepository.GetRecentAsync(siteId, count, cancellationToken);
        return movements.Select(m => m.ToSummaryDto()).ToList();
    }

    public async Task<List<MovementDto>> CreateBatchAsync(Guid siteId, BatchMovementRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var batchId = Guid.NewGuid();
        var movements = new List<InventoryMovement>();
        var sequence = 0;

        foreach (var item in request.Movements)
        {
            sequence++;
            var package = await _packageRepository.GetByIdAsync(siteId, item.PackageId, cancellationToken);
            if (package == null)
            {
                _logger.LogWarning("Package {PackageId} not found for batch movement", item.PackageId);
                continue;
            }

            InventoryMovement movement;

            if (item.ToLocationId.HasValue)
            {
                // Transfer movement
                var fromLocationId = package.LocationId ?? Guid.Empty;
                var fromLocationPath = package.LocationName ?? "";

                movement = InventoryMovement.CreateTransfer(
                    siteId, package.Id, package.PackageLabel.Value, package.Quantity, package.UnitOfMeasure,
                    fromLocationId, fromLocationPath, item.ToLocationId.Value, "", userId, item.Notes);

                // Update package location
                package.UpdateLocation(item.ToLocationId, null, null, userId);
                await _packageRepository.UpdateAsync(package, cancellationToken);
            }
            else if (item.Quantity.HasValue && !string.IsNullOrWhiteSpace(item.ReasonCode))
            {
                // Adjustment movement
                var quantityBefore = package.Quantity;

                if (!Enum.TryParse<AdjustmentReason>(item.ReasonCode, true, out var reason))
                    continue;

                package.Adjust(item.Quantity.Value, reason, DateOnly.FromDateTime(DateTime.UtcNow), userId, item.Notes);
                await _packageRepository.UpdateAsync(package, cancellationToken);

                movement = InventoryMovement.CreateAdjustment(
                    siteId, package.Id, package.PackageLabel.Value, item.Quantity.Value, package.UnitOfMeasure,
                    quantityBefore, package.Quantity, item.ReasonCode, userId, item.Notes, package.UnitCost);
            }
            else
            {
                continue;
            }

            movement.SetItemInfo(package.ItemId, package.ItemName);
            movement.SetBatchMovement(batchId, sequence);
            movements.Add(movement);
        }

        if (movements.Any())
        {
            await _movementRepository.AddRangeAsync(movements, cancellationToken);
            await _movementRepository.SaveChangesAsync(cancellationToken);
            await _packageRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created batch movement {BatchId} with {Count} movements", batchId, movements.Count);
        }

        return movements.Select(m => m.ToDto()).ToList();
    }
}




