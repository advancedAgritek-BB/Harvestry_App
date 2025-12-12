namespace Harvestry.Transfers.Domain.Enums;

public enum OutboundTransferStatus
{
    Draft = 0,
    Ready = 1,
    SubmittedToMetrc = 2,
    InTransit = 3,
    Delivered = 4,
    Accepted = 5,
    Rejected = 6,
    Voided = 7,
    Cancelled = 8
}

