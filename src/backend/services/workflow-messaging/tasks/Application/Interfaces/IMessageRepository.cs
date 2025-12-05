using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DomainMessage = Harvestry.Tasks.Domain.Entities.Message;

namespace Harvestry.Tasks.Application.Interfaces;

public interface IMessageRepository
{
    Task AddAsync(DomainMessage message, CancellationToken cancellationToken);
    Task UpdateAsync(DomainMessage message, CancellationToken cancellationToken);
    Task<DomainMessage?> GetByIdAsync(Guid siteId, Guid messageId, CancellationToken cancellationToken);
    Task<IReadOnlyList<DomainMessage>> GetByConversationAsync(Guid siteId, Guid conversationId, int? limit, DateTimeOffset? since, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
