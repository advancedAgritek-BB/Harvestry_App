namespace Harvestry.Transfers.Application.DTOs;

public sealed record TransportManifestDto
{
    public Guid Id { get; init; }
    public Guid SiteId { get; init; }
    public Guid OutboundTransferId { get; init; }

    public string Status { get; init; } = string.Empty;

    public string? TransporterName { get; init; }
    public string? TransporterLicenseNumber { get; init; }

    public string? DriverName { get; init; }
    public string? DriverLicenseNumber { get; init; }
    public string? DriverPhone { get; init; }

    public string? VehicleMake { get; init; }
    public string? VehicleModel { get; init; }
    public string? VehiclePlate { get; init; }

    public DateTime? DepartureAt { get; init; }
    public DateTime? ArrivalAt { get; init; }

    public string? MetrcManifestNumber { get; init; }
}

public sealed record UpsertTransportManifestRequest
{
    public string? TransporterName { get; init; }
    public string? TransporterLicenseNumber { get; init; }
    public string? DriverName { get; init; }
    public string? DriverLicenseNumber { get; init; }
    public string? DriverPhone { get; init; }
    public string? VehicleMake { get; init; }
    public string? VehicleModel { get; init; }
    public string? VehiclePlate { get; init; }
    public DateTime? DepartureAt { get; init; }
    public DateTime? ArrivalAt { get; init; }
}

