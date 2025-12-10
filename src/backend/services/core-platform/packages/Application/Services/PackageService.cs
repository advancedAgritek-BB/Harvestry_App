using Harvestry.Packages.Application.DTOs;
using Harvestry.Packages.Application.Interfaces;
using Harvestry.Packages.Application.Mappers;
using Harvestry.Packages.Domain.Entities;
using Harvestry.Packages.Domain.Enums;
using Harvestry.Packages.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Harvestry.Packages.Application.Services;

/// <summary>
/// Service implementation for Package operations
/// </summary>
public class PackageService : IPackageService
{
    private readonly IPackageRepository _packageRepository;
    private readonly IMovementRepository _movementRepository;
    private readonly ILogger<PackageService> _logger;

    public PackageService(
        IPackageRepository packageRepository,
        IMovementRepository movementRepository,
        ILogger<PackageService> logger)
    {
        _packageRepository = packageRepository;
        _movementRepository = movementRepository;
        _logger = logger;
    }

    public async Task<PackageDto?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default)
    {
        var package = await _packageRepository.GetByIdAsync(siteId, id, cancellationToken);
        return package?.ToDto();
    }

    public async Task<PackageDto?> GetByLabelAsync(Guid siteId, string label, CancellationToken cancellationToken = default)
    {
        var package = await _packageRepository.GetByLabelAsync(siteId, label, cancellationToken);
        return package?.ToDto();
    }

    public async Task<PackageListResponse> GetPackagesAsync(
        Guid siteId, int page = 1, int pageSize = 50,
        string? status = null, string? labTestingState = null, string? inventoryCategory = null,
        string? itemCategory = null, Guid? locationId = null, string? search = null,
        bool? onHold = null, bool? expiringSoon = null, CancellationToken cancellationToken = default)
    {
        PackageStatus? statusEnum = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PackageStatus>(status, true, out var s))
            statusEnum = s;

        LabTestingState? labEnum = null;
        if (!string.IsNullOrWhiteSpace(labTestingState) && Enum.TryParse<LabTestingState>(labTestingState, true, out var l))
            labEnum = l;

        InventoryCategory? invCatEnum = null;
        if (!string.IsNullOrWhiteSpace(inventoryCategory) && Enum.TryParse<InventoryCategory>(inventoryCategory, true, out var ic))
            invCatEnum = ic;

        var (packages, totalCount) = await _packageRepository.GetBySiteAsync(
            siteId, page, pageSize, statusEnum, labEnum, invCatEnum, itemCategory,
            locationId, search, onHold, expiringSoon, cancellationToken);

        return new PackageListResponse
        {
            Packages = packages.Select(p => p.ToSummaryDto()).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PackageDto> CreateAsync(Guid siteId, CreatePackageRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        if (await _packageRepository.LabelExistsAsync(siteId, request.PackageLabel, cancellationToken))
            throw new InvalidOperationException($"Package label '{request.PackageLabel}' already exists");

        var label = PackageLabel.Create(request.PackageLabel);

        Package package;
        if (request.SourceHarvestId.HasValue)
        {
            package = Package.CreateFromHarvest(
                siteId, label, request.ItemId, request.ItemName, request.ItemCategory ?? "",
                request.Quantity, request.UnitOfMeasure, request.SourceHarvestId.Value,
                request.SourceHarvestName ?? "", userId, request.LocationId);
        }
        else
        {
            package = Package.CreateFromPackages(
                siteId, label, request.ItemId, request.ItemName, request.ItemCategory ?? "",
                request.Quantity, request.UnitOfMeasure, request.SourcePackageLabels ?? new List<string>(),
                userId, request.LocationId);
        }

        // Set WMS fields
        if (!string.IsNullOrWhiteSpace(request.InventoryCategory) && 
            Enum.TryParse<InventoryCategory>(request.InventoryCategory, true, out var invCat))
        {
            package.SetInventoryCategory(invCat, userId);
        }

        if (request.UnitCost.HasValue)
            package.SetUnitCost(request.UnitCost.Value, userId);

        if (request.MaterialCost.HasValue || request.LaborCost.HasValue || request.OverheadCost.HasValue)
        {
            package.SetCostComponents(
                request.MaterialCost ?? 0, request.LaborCost ?? 0, request.OverheadCost ?? 0, userId);
        }

        if (request.VendorId.HasValue && !string.IsNullOrWhiteSpace(request.VendorName))
        {
            package.SetVendorInfo(request.VendorId.Value, request.VendorName, request.VendorLotNumber,
                null, null, request.ReceivedDate ?? DateOnly.FromDateTime(DateTime.UtcNow), userId);
        }

        if (!string.IsNullOrWhiteSpace(request.Grade) && Enum.TryParse<QualityGrade>(request.Grade, true, out var grade))
        {
            package.SetGrade(grade, request.QualityScore, null, userId);
        }

        if (request.ExpirationDate.HasValue)
            package.SetExpirationDate(request.ExpirationDate.Value, userId);

        if (!string.IsNullOrWhiteSpace(request.Notes))
            package.AddNote(request.Notes, userId);

        if (!string.IsNullOrWhiteSpace(request.LocationName))
            package.UpdateLocation(request.LocationId, request.LocationName, null, userId);

        var created = await _packageRepository.AddAsync(package, cancellationToken);
        await _packageRepository.SaveChangesAsync(cancellationToken);

        // Create receive movement
        if (request.LocationId.HasValue)
        {
            var movement = InventoryMovement.CreateReceive(
                siteId, created.Id, created.PackageLabel.Value, created.Quantity, created.UnitOfMeasure,
                request.LocationId.Value, request.LocationName ?? "", userId, request.UnitCost);
            movement.SetItemInfo(request.ItemId, request.ItemName);
            await _movementRepository.AddAsync(movement, cancellationToken);
            await _movementRepository.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("Created package {PackageId} ({Label}) for site {SiteId}", 
            created.Id, created.PackageLabel.Value, siteId);

        return created.ToDto();
    }

    public async Task<PackageDto?> UpdateAsync(Guid siteId, Guid id, UpdatePackageRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var package = await _packageRepository.GetByIdAsync(siteId, id, cancellationToken);
        if (package == null) return null;

        if (request.LocationId.HasValue || !string.IsNullOrWhiteSpace(request.LocationName))
            package.UpdateLocation(request.LocationId, request.LocationName, request.SublocationName, userId);

        if (request.ExpirationDate.HasValue)
            package.SetExpirationDate(request.ExpirationDate.Value, userId);

        if (request.UseByDate.HasValue)
            package.SetUseByDate(request.UseByDate.Value, userId);

        if (!string.IsNullOrWhiteSpace(request.InventoryCategory) && 
            Enum.TryParse<InventoryCategory>(request.InventoryCategory, true, out var invCat))
        {
            package.SetInventoryCategory(invCat, userId);
        }

        if (request.UnitCost.HasValue)
            package.SetUnitCost(request.UnitCost.Value, userId);

        if (!string.IsNullOrWhiteSpace(request.Grade) && Enum.TryParse<QualityGrade>(request.Grade, true, out var grade))
        {
            package.SetGrade(grade, request.QualityScore, request.QualityNotes, userId);
        }

        if (!string.IsNullOrWhiteSpace(request.Notes))
            package.AddNote(request.Notes, userId);

        await _packageRepository.UpdateAsync(package, cancellationToken);
        await _packageRepository.SaveChangesAsync(cancellationToken);

        return package.ToDto();
    }

    public async Task<PackageDto?> AdjustAsync(Guid siteId, Guid id, AdjustPackageRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var package = await _packageRepository.GetByIdAsync(siteId, id, cancellationToken);
        if (package == null) return null;

        var quantityBefore = package.Quantity;
        
        if (!Enum.TryParse<AdjustmentReason>(request.Reason, true, out var reason))
            throw new ArgumentException($"Invalid adjustment reason: {request.Reason}");

        package.Adjust(request.AdjustmentQuantity, reason, 
            request.AdjustmentDate ?? DateOnly.FromDateTime(DateTime.UtcNow), userId, request.ReasonNote);

        await _packageRepository.UpdateAsync(package, cancellationToken);
        await _packageRepository.SaveChangesAsync(cancellationToken);

        // Create movement record
        var movement = InventoryMovement.CreateAdjustment(
            siteId, id, package.PackageLabel.Value, request.AdjustmentQuantity, package.UnitOfMeasure,
            quantityBefore, package.Quantity, request.Reason, userId, request.ReasonNote, package.UnitCost);
        movement.SetItemInfo(package.ItemId, package.ItemName);
        await _movementRepository.AddAsync(movement, cancellationToken);
        await _movementRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Adjusted package {PackageId} by {Quantity} for reason {Reason}", 
            id, request.AdjustmentQuantity, request.Reason);

        return package.ToDto();
    }

    public async Task<PackageDto?> MoveAsync(Guid siteId, Guid id, MovePackageRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var package = await _packageRepository.GetByIdAsync(siteId, id, cancellationToken);
        if (package == null) return null;

        var fromLocationId = package.LocationId;
        var fromLocationPath = package.LocationName;

        package.UpdateLocation(request.ToLocationId, request.ToLocationPath, request.SublocationName, userId);

        await _packageRepository.UpdateAsync(package, cancellationToken);
        await _packageRepository.SaveChangesAsync(cancellationToken);

        // Create movement record
        var movement = InventoryMovement.CreateTransfer(
            siteId, id, package.PackageLabel.Value, package.Quantity, package.UnitOfMeasure,
            fromLocationId ?? Guid.Empty, fromLocationPath ?? "", request.ToLocationId,
            request.ToLocationPath ?? "", userId, request.Notes);
        movement.SetItemInfo(package.ItemId, package.ItemName);
        if (!string.IsNullOrWhiteSpace(request.BarcodeScanned))
            movement.Verify(userId, null, request.BarcodeScanned);
        await _movementRepository.AddAsync(movement, cancellationToken);
        await _movementRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Moved package {PackageId} to location {LocationId}", id, request.ToLocationId);

        return package.ToDto();
    }

    public async Task<PackageDto?> ReserveAsync(Guid siteId, Guid id, ReserveQuantityRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var package = await _packageRepository.GetByIdAsync(siteId, id, cancellationToken);
        if (package == null) return null;

        package.Reserve(request.Quantity, userId);
        await _packageRepository.UpdateAsync(package, cancellationToken);
        await _packageRepository.SaveChangesAsync(cancellationToken);

        return package.ToDto();
    }

    public async Task<PackageDto?> UnreserveAsync(Guid siteId, Guid id, decimal quantity, Guid userId, CancellationToken cancellationToken = default)
    {
        var package = await _packageRepository.GetByIdAsync(siteId, id, cancellationToken);
        if (package == null) return null;

        package.Unreserve(quantity, userId);
        await _packageRepository.UpdateAsync(package, cancellationToken);
        await _packageRepository.SaveChangesAsync(cancellationToken);

        return package.ToDto();
    }

    public async Task<List<PackageDto>> SplitAsync(Guid siteId, SplitPackageRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var sourcePackage = await _packageRepository.GetByIdAsync(siteId, request.SourcePackageId, cancellationToken);
        if (sourcePackage == null)
            throw new InvalidOperationException("Source package not found");

        var totalTargetQuantity = request.Targets.Sum(t => t.Quantity);
        if (totalTargetQuantity > sourcePackage.Quantity)
            throw new InvalidOperationException($"Total target quantity ({totalTargetQuantity}) exceeds source quantity ({sourcePackage.Quantity})");

        var newPackages = new List<Package>();

        foreach (var target in request.Targets)
        {
            if (await _packageRepository.LabelExistsAsync(siteId, target.PackageLabel, cancellationToken))
                throw new InvalidOperationException($"Package label '{target.PackageLabel}' already exists");

            var label = PackageLabel.Create(target.PackageLabel);
            var newPackage = Package.CreateFromPackages(
                siteId, label, sourcePackage.ItemId, sourcePackage.ItemName, sourcePackage.ItemCategory,
                target.Quantity, sourcePackage.UnitOfMeasure, new[] { sourcePackage.PackageLabel.Value },
                userId, target.LocationId ?? sourcePackage.LocationId);

            // Copy properties from source
            if (sourcePackage.UnitCost.HasValue)
                newPackage.SetUnitCost(sourcePackage.UnitCost.Value, userId);
            newPackage.SetInventoryCategory(sourcePackage.InventoryCategory, userId);
            newPackage.SetLineage(sourcePackage.RootAncestorId ?? sourcePackage.Id, sourcePackage.GenerationDepth + 1, null);

            await _packageRepository.AddAsync(newPackage, cancellationToken);
            newPackages.Add(newPackage);
        }

        // Reduce source package quantity
        var remainingQuantity = sourcePackage.Quantity - totalTargetQuantity;
        if (remainingQuantity > 0)
        {
            sourcePackage.Adjust(-totalTargetQuantity, AdjustmentReason.Processing, DateOnly.FromDateTime(DateTime.UtcNow), userId, "Split to new packages");
        }
        else
        {
            sourcePackage.Finish(DateOnly.FromDateTime(DateTime.UtcNow), userId);
        }

        await _packageRepository.UpdateAsync(sourcePackage, cancellationToken);
        await _packageRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Split package {PackageId} into {Count} new packages", request.SourcePackageId, request.Targets.Count);

        return newPackages.Select(p => p.ToDto()).ToList();
    }

    public async Task<PackageDto?> MergeAsync(Guid siteId, MergePackagesRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var sourcePackages = await _packageRepository.GetByIdsAsync(siteId, request.SourcePackageIds, cancellationToken);
        if (sourcePackages.Count != request.SourcePackageIds.Count)
            throw new InvalidOperationException("One or more source packages not found");

        // Validate all packages have same item
        var itemId = sourcePackages.First().ItemId;
        if (sourcePackages.Any(p => p.ItemId != itemId))
            throw new InvalidOperationException("All packages must be for the same item");

        if (await _packageRepository.LabelExistsAsync(siteId, request.TargetPackageLabel, cancellationToken))
            throw new InvalidOperationException($"Package label '{request.TargetPackageLabel}' already exists");

        var label = PackageLabel.Create(request.TargetPackageLabel);
        var first = sourcePackages.First();
        var totalQuantity = sourcePackages.Sum(p => p.Quantity);
        var avgUnitCost = sourcePackages.Where(p => p.UnitCost.HasValue).Select(p => p.UnitCost!.Value).DefaultIfEmpty(0).Average();

        var mergedPackage = Package.CreateFromPackages(
            siteId, label, first.ItemId, first.ItemName, first.ItemCategory,
            totalQuantity, first.UnitOfMeasure, sourcePackages.Select(p => p.PackageLabel.Value),
            userId, request.LocationId ?? first.LocationId);

        if (avgUnitCost > 0)
            mergedPackage.SetUnitCost(avgUnitCost, userId);
        mergedPackage.SetInventoryCategory(first.InventoryCategory, userId);

        await _packageRepository.AddAsync(mergedPackage, cancellationToken);

        // Finish source packages
        foreach (var source in sourcePackages)
        {
            source.Finish(DateOnly.FromDateTime(DateTime.UtcNow), userId);
            await _packageRepository.UpdateAsync(source, cancellationToken);
        }

        await _packageRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Merged {Count} packages into {PackageId}", request.SourcePackageIds.Count, mergedPackage.Id);

        return mergedPackage.ToDto();
    }

    public async Task<PackageDto?> FinishAsync(Guid siteId, Guid id, DateOnly finishDate, Guid userId, CancellationToken cancellationToken = default)
    {
        var package = await _packageRepository.GetByIdAsync(siteId, id, cancellationToken);
        if (package == null) return null;

        package.Finish(finishDate, userId);
        await _packageRepository.UpdateAsync(package, cancellationToken);
        await _packageRepository.SaveChangesAsync(cancellationToken);

        return package.ToDto();
    }

    public async Task<PackageDto?> UnfinishAsync(Guid siteId, Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var package = await _packageRepository.GetByIdAsync(siteId, id, cancellationToken);
        if (package == null) return null;

        package.Unfinish(userId);
        await _packageRepository.UpdateAsync(package, cancellationToken);
        await _packageRepository.SaveChangesAsync(cancellationToken);

        return package.ToDto();
    }

    public async Task<PackageSummaryStatsDto> GetSummaryStatsAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var stats = await _packageRepository.GetSummaryStatsAsync(siteId, cancellationToken);
        return new PackageSummaryStatsDto
        {
            TotalPackages = stats.TotalPackages,
            ActivePackages = stats.ActivePackages,
            OnHoldPackages = stats.OnHoldPackages,
            FinishedPackages = stats.FinishedPackages,
            TotalQuantity = stats.TotalQuantity,
            TotalValue = stats.TotalValue,
            ExpiringInWeek = stats.ExpiringInWeek,
            ExpiringInMonth = stats.ExpiringInMonth,
            PendingLabTest = stats.PendingLabTest,
            FailedLabTest = stats.FailedLabTest
        };
    }

    public async Task<PackageLineageDto?> GetLineageAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default)
    {
        var package = await _packageRepository.GetByIdAsync(siteId, id, cancellationToken);
        if (package == null) return null;

        var ancestors = await _packageRepository.GetAncestorsAsync(id, cancellationToken);
        var descendants = await _packageRepository.GetDescendantsAsync(id, cancellationToken);

        return new PackageLineageDto
        {
            PackageId = id,
            PackageLabel = package.PackageLabel.Value,
            GenerationDepth = package.GenerationDepth,
            Ancestors = ancestors.Select(a => new LineageNodeDto
            {
                Id = a.Id,
                PackageLabel = a.PackageLabel.Value,
                ItemName = a.ItemName,
                Quantity = a.Quantity,
                UnitOfMeasure = a.UnitOfMeasure,
                Status = a.Status.ToString(),
                PackagedDate = a.PackagedDate,
                GenerationDepth = a.GenerationDepth
            }).ToList(),
            Descendants = descendants.Select(d => new LineageNodeDto
            {
                Id = d.Id,
                PackageLabel = d.PackageLabel.Value,
                ItemName = d.ItemName,
                Quantity = d.Quantity,
                UnitOfMeasure = d.UnitOfMeasure,
                Status = d.Status.ToString(),
                PackagedDate = d.PackagedDate,
                GenerationDepth = d.GenerationDepth
            }).ToList()
        };
    }

    public async Task<List<ExpiringPackageDto>> GetExpiringAsync(Guid siteId, int withinDays = 30, CancellationToken cancellationToken = default)
    {
        var packages = await _packageRepository.GetExpiringAsync(siteId, withinDays, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return packages.Select(p =>
        {
            var daysUntilExpiry = p.ExpirationDate.HasValue ? p.ExpirationDate.Value.DayNumber - today.DayNumber : 0;
            return new ExpiringPackageDto
            {
                Id = p.Id,
                PackageLabel = p.PackageLabel.Value,
                ItemName = p.ItemName,
                ItemCategory = p.ItemCategory,
                Quantity = p.Quantity,
                UnitOfMeasure = p.UnitOfMeasure,
                ExpirationDate = p.ExpirationDate!.Value,
                DaysUntilExpiry = daysUntilExpiry,
                ValueAtRisk = p.TotalValue,
                LocationName = p.LocationName,
                Grade = p.Grade?.ToString(),
                ExpiryStatus = daysUntilExpiry <= 0 ? "expired" :
                    daysUntilExpiry <= 7 ? "critical" :
                    daysUntilExpiry <= 14 ? "warning" : "upcoming"
            };
        }).ToList();
    }
}



