namespace Harvestry.Transfers.Application.DTOs;

public sealed record OutboundTransferDto
{
    public Guid Id { get; init; }
    public Guid SiteId { get; init; }
    public Guid? ShipmentId { get; init; }
    public Guid? SalesOrderId { get; init; }

    public string DestinationLicenseNumber { get; init; } = string.Empty;
    public string? DestinationFacilityName { get; init; }

    public string Status { get; init; } = string.Empty;
    public string? StatusReason { get; init; }

    public DateTime? PlannedDepartureAt { get; init; }
    public DateTime? PlannedArrivalAt { get; init; }

    public long? MetrcTransferTemplateId { get; init; }
    public string? MetrcTransferNumber { get; init; }
    public string? MetrcSyncStatus { get; init; }
    public string? MetrcSyncError { get; init; }

    public List<OutboundTransferPackageDto> Packages { get; init; } = new();
}

public sealed record OutboundTransferPackageDto
{
    public Guid Id { get; init; }
    public Guid PackageId { get; init; }
    public string? PackageLabel { get; init; }
    public decimal Quantity { get; init; }
    public string UnitOfMeasure { get; init; } = string.Empty;
}

public sealed record OutboundTransferListResponse
{
    public List<OutboundTransferDto> Transfers { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}

