using Harvestry.Transfers.Domain.Entities;

namespace Harvestry.Transfers.Application.Interfaces;

public interface IOutboundTransferRepository
{
    Task<OutboundTransfer?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default);
    Task<(List<OutboundTransfer> Transfers, int TotalCount)> GetBySiteAsync(Guid siteId, int page = 1, int pageSize = 50, string? status = null, CancellationToken cancellationToken = default);

    Task<OutboundTransfer> AddAsync(OutboundTransfer transfer, CancellationToken cancellationToken = default);
    Task<OutboundTransfer> UpdateAsync(OutboundTransfer transfer, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

