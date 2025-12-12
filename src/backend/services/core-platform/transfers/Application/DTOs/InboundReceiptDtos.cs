namespace Harvestry.Transfers.Application.DTOs;

public sealed record InboundReceiptDto
{
    public Guid Id { get; init; }
    public Guid SiteId { get; init; }
    public Guid? OutboundTransferId { get; init; }
    public long? MetrcTransferId { get; init; }
    public string? MetrcTransferNumber { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime? ReceivedAt { get; init; }
    public string? Notes { get; init; }
    public List<InboundReceiptLineDto> Lines { get; init; } = new();
}

public sealed record InboundReceiptListResponse
{
    public List<InboundReceiptDto> Receipts { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}

public sealed record InboundReceiptLineDto
{
    public Guid Id { get; init; }
    public string PackageLabel { get; init; } = string.Empty;
    public decimal ReceivedQuantity { get; init; }
    public string UnitOfMeasure { get; init; } = string.Empty;
    public bool Accepted { get; init; }
    public string? RejectionReason { get; init; }
}

