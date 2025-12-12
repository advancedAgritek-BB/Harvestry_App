using Harvestry.Transfers.Domain.Entities;

namespace Harvestry.Transfers.Application.Interfaces;

public interface IInboundReceiptRepository
{
    Task<(List<InboundTransferReceipt> Receipts, int TotalCount)> GetBySiteAsync(Guid siteId, int page = 1, int pageSize = 50, string? status = null, CancellationToken cancellationToken = default);
    Task<InboundTransferReceipt?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default);
    Task<InboundTransferReceipt> AddAsync(InboundTransferReceipt receipt, CancellationToken cancellationToken = default);
    Task<InboundTransferReceipt> UpdateAsync(InboundTransferReceipt receipt, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

