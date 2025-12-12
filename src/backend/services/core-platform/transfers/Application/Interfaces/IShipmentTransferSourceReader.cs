namespace Harvestry.Transfers.Application.Interfaces;

public interface IShipmentTransferSourceReader
{
    Task<ShipmentTransferSource?> GetAsync(Guid siteId, Guid shipmentId, CancellationToken cancellationToken = default);
}

public sealed record ShipmentTransferSource
{
    public Guid ShipmentId { get; init; }
    public Guid? SalesOrderId { get; init; }
    public string DestinationLicenseNumber { get; init; } = string.Empty;
    public string? DestinationFacilityName { get; init; }
    public List<ShipmentTransferSourcePackage> Packages { get; init; } = new();
}

public sealed record ShipmentTransferSourcePackage
{
    public Guid PackageId { get; init; }
    public string? PackageLabel { get; init; }
    public decimal Quantity { get; init; }
    public string UnitOfMeasure { get; init; } = string.Empty;
}

