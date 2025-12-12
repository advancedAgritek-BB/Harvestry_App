using System.Transactions;
using Harvestry.Packages.Application.Interfaces;
using Harvestry.Sales.Application.DTOs;
using Harvestry.Sales.Application.Interfaces;
using Harvestry.Sales.Application.Mappers;
using Harvestry.Sales.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Harvestry.Sales.Application.Services;

public sealed class AllocationService : IAllocationService
{
    private readonly ISalesOrderRepository _salesOrderRepository;
    private readonly ISalesOrderLineRepository _lineRepository;
    private readonly ISalesAllocationRepository _allocationRepository;
    private readonly IPackageRepository _packageRepository;
    private readonly ILogger<AllocationService> _logger;

    public AllocationService(
        ISalesOrderRepository salesOrderRepository,
        ISalesOrderLineRepository lineRepository,
        ISalesAllocationRepository allocationRepository,
        IPackageRepository packageRepository,
        ILogger<AllocationService> logger)
    {
        _salesOrderRepository = salesOrderRepository;
        _lineRepository = lineRepository;
        _allocationRepository = allocationRepository;
        _packageRepository = packageRepository;
        _logger = logger;
    }

    public async Task<List<SalesAllocationDto>> GetAllocationsAsync(Guid siteId, Guid salesOrderId, CancellationToken cancellationToken = default)
    {
        var allocations = await _allocationRepository.GetBySalesOrderIdAsync(siteId, salesOrderId, cancellationToken);
        return allocations.Select(a => a.ToDto()).ToList();
    }

    public async Task<List<SalesAllocationDto>> AllocateAsync(Guid siteId, Guid salesOrderId, AllocateSalesOrderRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var order = await _salesOrderRepository.GetByIdAsync(siteId, salesOrderId, cancellationToken);
        if (order == null) throw new InvalidOperationException("Sales order not found.");

        var lines = await _lineRepository.GetByOrderIdAsync(siteId, salesOrderId, cancellationToken);
        if (lines.Count == 0) throw new InvalidOperationException("Sales order has no lines.");

        var lineById = lines.ToDictionary(l => l.Id, l => l);

        var packageIds = request.Lines
            .SelectMany(l => l.Packages)
            .Select(p => p.PackageId)
            .Distinct()
            .ToList();

        var packages = await _packageRepository.GetByIdsAsync(siteId, packageIds, cancellationToken);
        var packagesById = packages.ToDictionary(p => p.Id, p => p);

        var newAllocations = new List<SalesAllocation>();

        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        foreach (var lineReq in request.Lines)
        {
            if (!lineById.TryGetValue(lineReq.SalesOrderLineId, out var line))
            {
                throw new InvalidOperationException($"Sales order line {lineReq.SalesOrderLineId} not found on order.");
            }

            foreach (var pkgReq in lineReq.Packages)
            {
                if (!packagesById.TryGetValue(pkgReq.PackageId, out var package))
                {
                    throw new InvalidOperationException($"Package {pkgReq.PackageId} not found.");
                }

                package.Reserve(pkgReq.Quantity, userId);
                await _packageRepository.UpdateAsync(package, cancellationToken);

                line.AddAllocation(pkgReq.Quantity);
                await _lineRepository.UpdateAsync(line, cancellationToken);

                var allocation = SalesAllocation.Create(
                    siteId,
                    salesOrderId,
                    line.Id,
                    package.Id,
                    package.PackageLabel.Value,
                    pkgReq.Quantity,
                    package.UnitOfMeasure,
                    userId);

                newAllocations.Add(allocation);
            }
        }

        if (newAllocations.Count == 0)
        {
            throw new InvalidOperationException("No allocations were provided.");
        }

        await _allocationRepository.AddRangeAsync(newAllocations, cancellationToken);
        order.MarkAllocated(userId);
        await _salesOrderRepository.UpdateAsync(order, cancellationToken);

        await _packageRepository.SaveChangesAsync(cancellationToken);
        await _allocationRepository.SaveChangesAsync(cancellationToken);
        await _lineRepository.SaveChangesAsync(cancellationToken);
        await _salesOrderRepository.SaveChangesAsync(cancellationToken);

        tx.Complete();

        _logger.LogInformation("Allocated {Count} package allocations for sales order {SalesOrderId}", newAllocations.Count, salesOrderId);

        return newAllocations.Select(a => a.ToDto()).ToList();
    }

    public async Task<List<SalesAllocationDto>> UnallocateAsync(Guid siteId, Guid salesOrderId, UnallocateSalesOrderRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        if (request.AllocationIds.Count == 0) return new List<SalesAllocationDto>();
        if (string.IsNullOrWhiteSpace(request.Reason)) throw new ArgumentException("Reason is required", nameof(request.Reason));

        var allocations = await _allocationRepository.GetBySalesOrderIdAsync(siteId, salesOrderId, cancellationToken);
        var allocationsById = allocations.ToDictionary(a => a.Id, a => a);

        var targetAllocations = request.AllocationIds
            .Where(id => allocationsById.ContainsKey(id))
            .Select(id => allocationsById[id])
            .Where(a => !a.IsCancelled)
            .ToList();

        if (targetAllocations.Count == 0) return new List<SalesAllocationDto>();

        var orderLines = await _lineRepository.GetByOrderIdAsync(siteId, salesOrderId, cancellationToken);
        var lineById = orderLines.ToDictionary(l => l.Id, l => l);

        var packageIds = targetAllocations.Select(a => a.PackageId).Distinct().ToList();
        var packages = await _packageRepository.GetByIdsAsync(siteId, packageIds, cancellationToken);
        var packagesById = packages.ToDictionary(p => p.Id, p => p);

        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        foreach (var allocation in targetAllocations)
        {
            allocation.Cancel(request.Reason, userId);
            await _allocationRepository.UpdateAsync(allocation, cancellationToken);

            if (packagesById.TryGetValue(allocation.PackageId, out var package))
            {
                package.Unreserve(allocation.AllocatedQuantity, userId);
                await _packageRepository.UpdateAsync(package, cancellationToken);
            }

            // We do not decrement AllocatedQuantity on the order line here to preserve audit accuracy;
            // instead, allocations are the source of truth for current reserved state.
            // Line.AllocatedQuantity is used as a derived quick metric and can be rebuilt.
            if (lineById.TryGetValue(allocation.SalesOrderLineId, out var line))
            {
                // Best-effort: keep allocated quantity meaningful (non-negative).
                line.RemoveAllocation(allocation.AllocatedQuantity);
                await _lineRepository.UpdateAsync(line, cancellationToken);
            }
        }

        await _packageRepository.SaveChangesAsync(cancellationToken);
        await _allocationRepository.SaveChangesAsync(cancellationToken);
        await _lineRepository.SaveChangesAsync(cancellationToken);

        tx.Complete();

        return targetAllocations.Select(a => a.ToDto()).ToList();
    }
}

