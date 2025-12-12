namespace Harvestry.Transfers.Application.DTOs;

public sealed record CreateOutboundTransferFromShipmentRequest
{
    public Guid ShipmentId { get; init; }
    public DateTime? PlannedDepartureAt { get; init; }
    public DateTime? PlannedArrivalAt { get; init; }
}

public sealed record SubmitOutboundTransferToMetrcRequest
{
    public Guid MetrcSyncJobId { get; init; }
    public string LicenseNumber { get; init; } = string.Empty;
    public int Priority { get; init; } = 100;
}

public sealed record VoidOutboundTransferRequest
{
    public Guid MetrcSyncJobId { get; init; }
    public string LicenseNumber { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}

