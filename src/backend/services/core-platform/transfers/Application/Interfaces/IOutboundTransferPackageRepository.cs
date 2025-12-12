using Harvestry.Transfers.Domain.Entities;

namespace Harvestry.Transfers.Application.Interfaces;

public interface IOutboundTransferPackageRepository
{
    Task<List<OutboundTransferPackage>> GetByTransferIdAsync(Guid siteId, Guid outboundTransferId, CancellationToken cancellationToken = default);
    Task<List<OutboundTransferPackage>> AddRangeAsync(IEnumerable<OutboundTransferPackage> packages, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

