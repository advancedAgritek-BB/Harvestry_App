using Harvestry.Transfers.Application.DTOs;

namespace Harvestry.Transfers.Application.Interfaces;

public interface ITransportManifestService
{
    Task<TransportManifestDto?> GetByTransferIdAsync(Guid siteId, Guid outboundTransferId, CancellationToken cancellationToken = default);
    Task<TransportManifestDto> CreateOrUpdateAsync(Guid siteId, Guid outboundTransferId, UpsertTransportManifestRequest request, Guid userId, CancellationToken cancellationToken = default);
}

