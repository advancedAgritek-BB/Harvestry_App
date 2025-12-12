using Harvestry.Transfers.Domain.Entities;

namespace Harvestry.Transfers.Application.Interfaces;

public interface ITransferEventRepository
{
    Task<TransferEvent> AddAsync(TransferEvent evt, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

