namespace Harvestry.Sales.Application.DTOs;

public sealed record ShipmentDto
{
    public Guid Id { get; init; }
    public Guid SiteId { get; init; }
    public string ShipmentNumber { get; init; } = string.Empty;
    public Guid SalesOrderId { get; init; }

    public string Status { get; init; } = string.Empty;
    public DateTime? PickingStartedAt { get; init; }
    public DateTime? PackedAt { get; init; }
    public DateTime? ShippedAt { get; init; }
    public DateTime? CancelledAt { get; init; }

    public string? CarrierName { get; init; }
    public string? TrackingNumber { get; init; }

    public List<ShipmentPackageDto> Packages { get; init; } = new();
}

public sealed record ShipmentPackageDto
{
    public Guid Id { get; init; }
    public Guid PackageId { get; init; }
    public string? PackageLabel { get; init; }
    public decimal Quantity { get; init; }
    public string UnitOfMeasure { get; init; } = string.Empty;
    public DateTime? PackedAt { get; init; }
}

public sealed record ShipmentListResponse
{
    public List<ShipmentDto> Shipments { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}

