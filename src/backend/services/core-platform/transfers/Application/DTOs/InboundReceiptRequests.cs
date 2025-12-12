namespace Harvestry.Transfers.Application.DTOs;

public sealed record CreateInboundReceiptRequest
{
    public Guid? OutboundTransferId { get; init; }
    public long? MetrcTransferId { get; init; }
    public string? MetrcTransferNumber { get; init; }
    public string? Notes { get; init; }
    public List<CreateInboundReceiptLineRequest> Lines { get; init; } = new();
}

public sealed record CreateInboundReceiptLineRequest
{
    public string PackageLabel { get; init; } = string.Empty;
    public decimal ReceivedQuantity { get; init; }
    public string UnitOfMeasure { get; init; } = string.Empty;
    public bool Accepted { get; init; } = true;
    public string? RejectionReason { get; init; }
}

public sealed record AcceptInboundReceiptRequest
{
    public string? Notes { get; init; }
}

public sealed record RejectInboundReceiptRequest
{
    public string Reason { get; init; } = string.Empty;
}

