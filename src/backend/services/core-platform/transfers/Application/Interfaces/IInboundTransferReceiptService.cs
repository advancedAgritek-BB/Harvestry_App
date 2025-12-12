using Harvestry.Transfers.Application.DTOs;

namespace Harvestry.Transfers.Application.Interfaces;

public interface IInboundTransferReceiptService
{
    Task<InboundReceiptListResponse> GetBySiteAsync(Guid siteId, int page = 1, int pageSize = 50, string? status = null, CancellationToken cancellationToken = default);
    Task<InboundReceiptDto?> GetByIdAsync(Guid siteId, Guid receiptId, CancellationToken cancellationToken = default);
    Task<InboundReceiptDto> CreateDraftAsync(Guid siteId, CreateInboundReceiptRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<InboundReceiptDto?> AcceptAsync(Guid siteId, Guid receiptId, AcceptInboundReceiptRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<InboundReceiptDto?> RejectAsync(Guid siteId, Guid receiptId, RejectInboundReceiptRequest request, Guid userId, CancellationToken cancellationToken = default);
}

