using Harvestry.Transfers.Application.DTOs;
using Harvestry.Transfers.Application.Interfaces;
using Harvestry.Transfers.Application.Mappers;
using Harvestry.Transfers.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Harvestry.Transfers.Application.Services;

public sealed class InboundTransferReceiptService : IInboundTransferReceiptService
{
    private readonly IInboundReceiptRepository _receiptRepository;
    private readonly IInboundReceiptLineRepository _lineRepository;
    private readonly IOutboundTransferRepository _outboundTransferRepository;
    private readonly ILogger<InboundTransferReceiptService> _logger;

    public InboundTransferReceiptService(
        IInboundReceiptRepository receiptRepository,
        IInboundReceiptLineRepository lineRepository,
        IOutboundTransferRepository outboundTransferRepository,
        ILogger<InboundTransferReceiptService> logger)
    {
        _receiptRepository = receiptRepository;
        _lineRepository = lineRepository;
        _outboundTransferRepository = outboundTransferRepository;
        _logger = logger;
    }

    public async Task<InboundReceiptListResponse> GetBySiteAsync(
        Guid siteId,
        int page = 1,
        int pageSize = 50,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var (receipts, total) = await _receiptRepository.GetBySiteAsync(siteId, page, pageSize, status, cancellationToken);
        return new InboundReceiptListResponse
        {
            Receipts = receipts
                .Select(r => r.ToDto(lines: Array.Empty<InboundTransferReceiptLine>()))
                .ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<InboundReceiptDto?> GetByIdAsync(Guid siteId, Guid receiptId, CancellationToken cancellationToken = default)
    {
        var receipt = await _receiptRepository.GetByIdAsync(siteId, receiptId, cancellationToken);
        if (receipt == null) return null;

        var lines = await _lineRepository.GetByReceiptIdAsync(siteId, receipt.Id, cancellationToken);
        return receipt.ToDto(lines);
    }

    public async Task<InboundReceiptDto> CreateDraftAsync(Guid siteId, CreateInboundReceiptRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        if (request.OutboundTransferId.HasValue)
        {
            var transfer = await _outboundTransferRepository.GetByIdAsync(siteId, request.OutboundTransferId.Value, cancellationToken);
            if (transfer == null) throw new InvalidOperationException("Outbound transfer not found.");
        }

        var receipt = InboundTransferReceipt.CreateDraft(
            siteId,
            request.OutboundTransferId,
            request.MetrcTransferId,
            request.MetrcTransferNumber,
            userId);

        receipt.SetNotes(request.Notes, userId);

        var lines = request.Lines.Select(l =>
            InboundTransferReceiptLine.Create(siteId, receipt.Id, l.PackageLabel, l.ReceivedQuantity, l.UnitOfMeasure, l.Accepted, l.RejectionReason)
        ).ToList();

        await _receiptRepository.AddAsync(receipt, cancellationToken);
        await _lineRepository.AddRangeAsync(lines, cancellationToken);

        await _receiptRepository.SaveChangesAsync(cancellationToken);
        await _lineRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created inbound receipt {ReceiptId} (transfer: {TransferId})", receipt.Id, request.OutboundTransferId);
        return receipt.ToDto(lines);
    }

    public async Task<InboundReceiptDto?> AcceptAsync(Guid siteId, Guid receiptId, AcceptInboundReceiptRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var receipt = await _receiptRepository.GetByIdAsync(siteId, receiptId, cancellationToken);
        if (receipt == null) return null;

        receipt.Accept(userId, request.Notes);
        await _receiptRepository.UpdateAsync(receipt, cancellationToken);
        await _receiptRepository.SaveChangesAsync(cancellationToken);

        var lines = await _lineRepository.GetByReceiptIdAsync(siteId, receiptId, cancellationToken);
        return receipt.ToDto(lines);
    }

    public async Task<InboundReceiptDto?> RejectAsync(Guid siteId, Guid receiptId, RejectInboundReceiptRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var receipt = await _receiptRepository.GetByIdAsync(siteId, receiptId, cancellationToken);
        if (receipt == null) return null;

        receipt.Reject(userId, request.Reason);
        await _receiptRepository.UpdateAsync(receipt, cancellationToken);
        await _receiptRepository.SaveChangesAsync(cancellationToken);

        var lines = await _lineRepository.GetByReceiptIdAsync(siteId, receiptId, cancellationToken);
        return receipt.ToDto(lines);
    }
}

