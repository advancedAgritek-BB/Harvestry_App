using Harvestry.Shared.Kernel.Domain;
using Harvestry.Transfers.Domain.Enums;

namespace Harvestry.Transfers.Domain.Entities;

public sealed class InboundTransferReceipt : AggregateRoot<Guid>
{
    private readonly List<InboundTransferReceiptLine> _lines = new();

    private InboundTransferReceipt(Guid id) : base(id) { }

    private InboundTransferReceipt(
        Guid id,
        Guid siteId,
        Guid? outboundTransferId,
        long? metrcTransferId,
        string? metrcTransferNumber,
        Guid createdByUserId) : base(id)
    {
        if (siteId == Guid.Empty) throw new ArgumentException("SiteId cannot be empty", nameof(siteId));
        if (createdByUserId == Guid.Empty) throw new ArgumentException("CreatedByUserId cannot be empty", nameof(createdByUserId));

        SiteId = siteId;
        OutboundTransferId = outboundTransferId;
        MetrcTransferId = metrcTransferId;
        MetrcTransferNumber = metrcTransferNumber?.Trim();
        Status = InboundReceiptStatus.Draft;

        CreatedAt = DateTime.UtcNow;
        CreatedByUserId = createdByUserId;
        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = createdByUserId;
    }

    public Guid SiteId { get; private set; }
    public Guid? OutboundTransferId { get; private set; }
    public long? MetrcTransferId { get; private set; }
    public string? MetrcTransferNumber { get; private set; }

    public InboundReceiptStatus Status { get; private set; }
    public DateTime? ReceivedAt { get; private set; }
    public Guid? ReceivedByUserId { get; private set; }
    public string? Notes { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    public IReadOnlyList<InboundTransferReceiptLine> Lines => _lines.AsReadOnly();

    public static InboundTransferReceipt CreateDraft(
        Guid siteId,
        Guid? outboundTransferId,
        long? metrcTransferId,
        string? metrcTransferNumber,
        Guid createdByUserId)
    {
        return new InboundTransferReceipt(
            Guid.NewGuid(),
            siteId,
            outboundTransferId,
            metrcTransferId,
            metrcTransferNumber,
            createdByUserId);
    }

    public InboundTransferReceiptLine AddLine(string packageLabel, decimal receivedQuantity, string unitOfMeasure, bool accepted, string? rejectionReason)
    {
        EnsureDraft();
        var line = InboundTransferReceiptLine.Create(SiteId, Id, packageLabel, receivedQuantity, unitOfMeasure, accepted, rejectionReason);
        _lines.Add(line);
        return line;
    }

    public void Accept(Guid receivedByUserId, string? notes)
    {
        EnsureDraft();
        if (receivedByUserId == Guid.Empty) throw new ArgumentException("ReceivedByUserId cannot be empty", nameof(receivedByUserId));

        Status = InboundReceiptStatus.Accepted;
        ReceivedAt = DateTime.UtcNow;
        ReceivedByUserId = receivedByUserId;
        Notes = notes?.Trim();
        Touch(receivedByUserId);
    }

    public void Reject(Guid receivedByUserId, string reason)
    {
        EnsureDraft();
        if (receivedByUserId == Guid.Empty) throw new ArgumentException("ReceivedByUserId cannot be empty", nameof(receivedByUserId));
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Rejection reason is required", nameof(reason));

        Status = InboundReceiptStatus.Rejected;
        ReceivedAt = DateTime.UtcNow;
        ReceivedByUserId = receivedByUserId;
        Notes = string.IsNullOrWhiteSpace(Notes) ? $"Rejected: {reason}" : $"{Notes}\n\nRejected: {reason}";
        Touch(receivedByUserId);
    }

    public void SetNotes(string? notes, Guid userId)
    {
        EnsureDraft();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        Touch(userId);
    }

    private void EnsureDraft()
    {
        if (Status != InboundReceiptStatus.Draft) throw new InvalidOperationException("Receipt is not in Draft state.");
    }

    private void Touch(Guid userId)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = userId;
    }
}

