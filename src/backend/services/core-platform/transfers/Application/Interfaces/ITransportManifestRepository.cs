using Harvestry.Transfers.Domain.Entities;

namespace Harvestry.Transfers.Application.Interfaces;

public interface ITransportManifestRepository
{
    Task<TransportManifest?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default);
    Task<TransportManifest?> GetByTransferIdAsync(Guid siteId, Guid outboundTransferId, CancellationToken cancellationToken = default);

    Task<TransportManifest> AddAsync(TransportManifest manifest, CancellationToken cancellationToken = default);
    Task<TransportManifest> UpdateAsync(TransportManifest manifest, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

