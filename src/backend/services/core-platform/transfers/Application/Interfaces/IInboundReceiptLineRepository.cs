using Harvestry.Transfers.Domain.Entities;

namespace Harvestry.Transfers.Application.Interfaces;

public interface IInboundReceiptLineRepository
{
    Task<List<InboundTransferReceiptLine>> GetByReceiptIdAsync(Guid siteId, Guid inboundReceiptId, CancellationToken cancellationToken = default);
    Task<List<InboundTransferReceiptLine>> AddRangeAsync(IEnumerable<InboundTransferReceiptLine> lines, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

