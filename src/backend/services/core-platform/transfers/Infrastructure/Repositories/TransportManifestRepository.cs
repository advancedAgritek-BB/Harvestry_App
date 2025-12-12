using Harvestry.Transfers.Application.Interfaces;
using Harvestry.Transfers.Domain.Entities;
using Harvestry.Transfers.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Transfers.Infrastructure.Repositories;

public sealed class TransportManifestRepository : ITransportManifestRepository
{
    private readonly TransfersDbContext _db;

    public TransportManifestRepository(TransfersDbContext db)
    {
        _db = db;
    }

    public Task<TransportManifest?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default)
        => _db.TransportManifests.FirstOrDefaultAsync(m => m.SiteId == siteId && m.Id == id, cancellationToken);

    public Task<TransportManifest?> GetByTransferIdAsync(Guid siteId, Guid outboundTransferId, CancellationToken cancellationToken = default)
        => _db.TransportManifests.FirstOrDefaultAsync(m => m.SiteId == siteId && m.OutboundTransferId == outboundTransferId, cancellationToken);

    public async Task<TransportManifest> AddAsync(TransportManifest manifest, CancellationToken cancellationToken = default)
    {
        await _db.TransportManifests.AddAsync(manifest, cancellationToken);
        return manifest;
    }

    public Task<TransportManifest> UpdateAsync(TransportManifest manifest, CancellationToken cancellationToken = default)
    {
        _db.TransportManifests.Update(manifest);
        return Task.FromResult(manifest);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}

