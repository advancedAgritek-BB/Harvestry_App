namespace Harvestry.Sales.Application.DTOs;

public sealed record CreateShipmentRequest
{
    public string ShipmentNumber { get; init; } = string.Empty;
    public string? Notes { get; init; }
}

public sealed record MarkShipmentShippedRequest
{
    public string? CarrierName { get; init; }
    public string? TrackingNumber { get; init; }
    public Guid? OutboundTransferId { get; init; }
}

public sealed record CancelShipmentRequest
{
    public string Reason { get; init; } = string.Empty;
}

