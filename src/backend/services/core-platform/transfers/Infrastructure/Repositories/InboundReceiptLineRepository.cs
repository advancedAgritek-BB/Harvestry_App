using Harvestry.Transfers.Application.Interfaces;
using Harvestry.Transfers.Domain.Entities;
using Harvestry.Transfers.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Transfers.Infrastructure.Repositories;

public sealed class InboundReceiptLineRepository : IInboundReceiptLineRepository
{
    private readonly TransfersDbContext _db;

    public InboundReceiptLineRepository(TransfersDbContext db)
    {
        _db = db;
    }

    public Task<List<InboundTransferReceiptLine>> GetByReceiptIdAsync(Guid siteId, Guid inboundReceiptId, CancellationToken cancellationToken = default)
        => _db.InboundReceiptLines
            .Where(l => l.SiteId == siteId && l.InboundReceiptId == inboundReceiptId)
            .ToListAsync(cancellationToken);

    public async Task<List<InboundTransferReceiptLine>> AddRangeAsync(IEnumerable<InboundTransferReceiptLine> lines, CancellationToken cancellationToken = default)
    {
        var list = lines.ToList();
        await _db.InboundReceiptLines.AddRangeAsync(list, cancellationToken);
        return list;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}

