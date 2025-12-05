using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Domain.Enums;
using DomainConversation = Harvestry.Tasks.Domain.Entities.Conversation;

namespace Harvestry.Tasks.Application.Interfaces;

public interface IConversationRepository
{
    Task AddAsync(DomainConversation conversation, CancellationToken cancellationToken);
    Task UpdateAsync(DomainConversation conversation, CancellationToken cancellationToken);
    Task<DomainConversation?> GetByIdAsync(Guid siteId, Guid conversationId, CancellationToken cancellationToken);
    Task<IReadOnlyList<DomainConversation>> GetBySiteAsync(Guid siteId, ConversationType? type, CancellationToken cancellationToken);
    Task<IReadOnlyList<DomainConversation>> GetByRelatedEntityAsync(Guid siteId, string relatedEntityType, Guid relatedEntityId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
