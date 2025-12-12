namespace Harvestry.Sales.Application.DTOs;

public sealed record SalesAllocationDto
{
    public Guid Id { get; init; }
    public Guid SalesOrderId { get; init; }
    public Guid SalesOrderLineId { get; init; }

    public Guid PackageId { get; init; }
    public string? PackageLabel { get; init; }
    public decimal AllocatedQuantity { get; init; }
    public string UnitOfMeasure { get; init; } = string.Empty;

    public bool IsCancelled { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed record AllocateSalesOrderRequest
{
    public List<AllocateSalesOrderLineRequest> Lines { get; init; } = new();
}

public sealed record AllocateSalesOrderLineRequest
{
    public Guid SalesOrderLineId { get; init; }
    public List<AllocatePackageRequest> Packages { get; init; } = new();
}

public sealed record AllocatePackageRequest
{
    public Guid PackageId { get; init; }
    public decimal Quantity { get; init; }
}

public sealed record UnallocateSalesOrderRequest
{
    public List<Guid> AllocationIds { get; init; } = new();
    public string Reason { get; init; } = string.Empty;
}

