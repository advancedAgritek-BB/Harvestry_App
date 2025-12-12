namespace Harvestry.Sales.Application.DTOs;

public sealed record SalesOrderDto
{
    public Guid Id { get; init; }
    public Guid SiteId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;

    public string CustomerName { get; init; } = string.Empty;
    public string? DestinationLicenseNumber { get; init; }
    public string? DestinationFacilityName { get; init; }

    public string Status { get; init; } = string.Empty;
    public DateOnly? RequestedShipDate { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public DateTime? CancelledAt { get; init; }

    public string? Notes { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public List<SalesOrderLineDto> Lines { get; init; } = new();
}

public sealed record SalesOrderLineDto
{
    public Guid Id { get; init; }
    public int LineNumber { get; init; }
    public Guid ItemId { get; init; }
    public string ItemName { get; init; } = string.Empty;
    public string UnitOfMeasure { get; init; } = string.Empty;

    public decimal RequestedQuantity { get; init; }
    public decimal AllocatedQuantity { get; init; }
    public decimal ShippedQuantity { get; init; }

    public decimal? UnitPrice { get; init; }
    public string CurrencyCode { get; init; } = "USD";
}

public sealed record SalesOrderListResponse
{
    public List<SalesOrderDto> Orders { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}

