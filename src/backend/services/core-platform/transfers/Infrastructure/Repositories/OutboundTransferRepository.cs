using Harvestry.Transfers.Application.Interfaces;
using Harvestry.Transfers.Domain.Entities;
using Harvestry.Transfers.Domain.Enums;
using Harvestry.Transfers.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Transfers.Infrastructure.Repositories;

public sealed class OutboundTransferRepository : IOutboundTransferRepository
{
    private readonly TransfersDbContext _db;

    public OutboundTransferRepository(TransfersDbContext db)
    {
        _db = db;
    }

    public Task<OutboundTransfer?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default)
        => _db.OutboundTransfers.FirstOrDefaultAsync(t => t.SiteId == siteId && t.Id == id, cancellationToken);

    public async Task<(List<OutboundTransfer> Transfers, int TotalCount)> GetBySiteAsync(Guid siteId, int page = 1, int pageSize = 50, string? status = null, CancellationToken cancellationToken = default)
    {
        var query = _db.OutboundTransfers.Where(t => t.SiteId == siteId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OutboundTransferStatus>(status, true, out var st))
        {
            query = query.Where(t => t.Status == st);
        }

        var total = await query.CountAsync(cancellationToken);
        var results = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (results, total);
    }

    public async Task<OutboundTransfer> AddAsync(OutboundTransfer transfer, CancellationToken cancellationToken = default)
    {
        await _db.OutboundTransfers.AddAsync(transfer, cancellationToken);
        return transfer;
    }

    public Task<OutboundTransfer> UpdateAsync(OutboundTransfer transfer, CancellationToken cancellationToken = default)
    {
        _db.OutboundTransfers.Update(transfer);
        return Task.FromResult(transfer);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}

