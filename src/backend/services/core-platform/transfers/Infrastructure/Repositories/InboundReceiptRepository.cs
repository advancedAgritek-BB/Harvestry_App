using Harvestry.Transfers.Application.Interfaces;
using Harvestry.Transfers.Domain.Entities;
using Harvestry.Transfers.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Transfers.Infrastructure.Repositories;

public sealed class InboundReceiptRepository : IInboundReceiptRepository
{
    private readonly TransfersDbContext _db;

    public InboundReceiptRepository(TransfersDbContext db)
    {
        _db = db;
    }

    public async Task<(List<InboundTransferReceipt> Receipts, int TotalCount)> GetBySiteAsync(
        Guid siteId,
        int page = 1,
        int pageSize = 50,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Max(pageSize, 1);

        var query = _db.InboundReceipts.AsNoTracking().Where(r => r.SiteId == siteId);
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(r => r.Status.ToString() == status);
        }

        var total = await query.CountAsync(cancellationToken);
        var receipts = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        return (receipts, total);
    }

    public Task<InboundTransferReceipt?> GetByIdAsync(Guid siteId, Guid id, CancellationToken cancellationToken = default)
        => _db.InboundReceipts.FirstOrDefaultAsync(r => r.SiteId == siteId && r.Id == id, cancellationToken);

    public async Task<InboundTransferReceipt> AddAsync(InboundTransferReceipt receipt, CancellationToken cancellationToken = default)
    {
        await _db.InboundReceipts.AddAsync(receipt, cancellationToken);
        return receipt;
    }

    public Task<InboundTransferReceipt> UpdateAsync(InboundTransferReceipt receipt, CancellationToken cancellationToken = default)
    {
        _db.InboundReceipts.Update(receipt);
        return Task.FromResult(receipt);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}

