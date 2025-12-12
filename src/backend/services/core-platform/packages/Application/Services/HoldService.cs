using Harvestry.Packages.Application.DTOs;
using Harvestry.Packages.Application.Interfaces;
using Harvestry.Packages.Application.Mappers;
using Harvestry.Packages.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Harvestry.Packages.Application.Services;

/// <summary>
/// Service implementation for Hold management
/// </summary>
public class HoldService : IHoldService
{
    private readonly IPackageRepository _packageRepository;
    private readonly ILogger<HoldService> _logger;

    public HoldService(IPackageRepository packageRepository, ILogger<HoldService> logger)
    {
        _packageRepository = packageRepository;
        _logger = logger;
    }

    public async Task<List<HoldSummaryDto>> GetHoldsAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var packages = await _packageRepository.GetOnHoldAsync(siteId, cancellationToken);

        return packages.Select(p => new HoldSummaryDto
        {
            Id = p.Id,
            PackageLabel = p.PackageLabel.Value,
            ItemName = p.ItemName,
            Quantity = p.Quantity,
            UnitOfMeasure = p.UnitOfMeasure,
            HoldReasonCode = p.HoldReasonCode?.ToString() ?? "Unknown",
            HoldPlacedAt = p.HoldPlacedAt ?? DateTime.MinValue,
            HoldPlacedByUserId = p.HoldPlacedByUserId ?? Guid.Empty,
            RequiresTwoPersonRelease = p.RequiresTwoPersonRelease,
            HasFirstApproval = p.HoldFirstApproverId.HasValue,
            ValueOnHold = p.TotalValue,
            LocationName = p.LocationName
        }).ToList();
    }

    public async Task<PackageDto?> PlaceHoldAsync(Guid siteId, Guid packageId, PlaceHoldRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var package = await _packageRepository.GetByIdAsync(siteId, packageId, cancellationToken);
        if (package == null) return null;

        if (!Enum.TryParse<HoldReasonCode>(request.ReasonCode, true, out var reasonCode))
            throw new ArgumentException($"Invalid hold reason code: {request.ReasonCode}");

        package.PlaceOnHoldWithReason(reasonCode, userId, request.RequiresTwoPersonRelease, request.Notes);

        await _packageRepository.UpdateAsync(package, cancellationToken);
        await _packageRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Placed hold on package {PackageId} for reason {Reason}", packageId, request.ReasonCode);

        return package.ToDto();
    }

    public async Task<PackageDto?> ApproveReleaseAsync(Guid siteId, Guid packageId, Guid approverId, CancellationToken cancellationToken = default)
    {
        var package = await _packageRepository.GetByIdAsync(siteId, packageId, cancellationToken);
        if (package == null) return null;

        package.ApproveHoldRelease(approverId);

        await _packageRepository.UpdateAsync(package, cancellationToken);
        await _packageRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("First approval for hold release on package {PackageId} by user {ApproverId}", packageId, approverId);

        return package.ToDto();
    }

    public async Task<PackageDto?> ReleaseHoldAsync(Guid siteId, Guid packageId, ReleaseHoldRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var package = await _packageRepository.GetByIdAsync(siteId, packageId, cancellationToken);
        if (package == null) return null;

        package.ReleaseFromHoldWithTracking(userId, request.Notes);

        await _packageRepository.UpdateAsync(package, cancellationToken);
        await _packageRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Released hold on package {PackageId}", packageId);

        return package.ToDto();
    }
}




