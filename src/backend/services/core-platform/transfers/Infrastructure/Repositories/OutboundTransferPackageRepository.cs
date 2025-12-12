using Harvestry.Transfers.Application.Interfaces;
using Harvestry.Transfers.Domain.Entities;
using Harvestry.Transfers.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Transfers.Infrastructure.Repositories;

public sealed class OutboundTransferPackageRepository : IOutboundTransferPackageRepository
{
    private readonly TransfersDbContext _db;

    public OutboundTransferPackageRepository(TransfersDbContext db)
    {
        _db = db;
    }

    public Task<List<OutboundTransferPackage>> GetByTransferIdAsync(Guid siteId, Guid outboundTransferId, CancellationToken cancellationToken = default)
        => _db.OutboundTransferPackages
            .Where(p => p.SiteId == siteId && p.OutboundTransferId == outboundTransferId)
            .ToListAsync(cancellationToken);

    public async Task<List<OutboundTransferPackage>> AddRangeAsync(IEnumerable<OutboundTransferPackage> packages, CancellationToken cancellationToken = default)
    {
        var list = packages.ToList();
        await _db.OutboundTransferPackages.AddRangeAsync(list, cancellationToken);
        return list;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}

