using Harvestry.Transfers.Application.Interfaces;
using Harvestry.Transfers.Domain.Entities;
using Harvestry.Transfers.Infrastructure.Persistence;

namespace Harvestry.Transfers.Infrastructure.Repositories;

public sealed class TransferEventRepository : ITransferEventRepository
{
    private readonly TransfersDbContext _db;

    public TransferEventRepository(TransfersDbContext db)
    {
        _db = db;
    }

    public async Task<TransferEvent> AddAsync(TransferEvent evt, CancellationToken cancellationToken = default)
    {
        await _db.TransferEvents.AddAsync(evt, cancellationToken);
        return evt;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}

