namespace Harvestry.Sales.Application.DTOs;

public sealed record CreateSalesOrderRequest
{
    public string OrderNumber { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string DestinationLicenseNumber { get; init; } = string.Empty;
    public string? DestinationFacilityName { get; init; }
    public DateOnly? RequestedShipDate { get; init; }
    public string? Notes { get; init; }
}

public sealed record AddSalesOrderLineRequest
{
    public int LineNumber { get; init; }
    public Guid ItemId { get; init; }
    public string ItemName { get; init; } = string.Empty;
    public decimal RequestedQuantity { get; init; }
    public string UnitOfMeasure { get; init; } = string.Empty;
    public decimal? UnitPrice { get; init; }
    public string CurrencyCode { get; init; } = "USD";
}

public sealed record CancelSalesOrderRequest
{
    public string Reason { get; init; } = string.Empty;
}

