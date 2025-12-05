using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.DTOs;

namespace Harvestry.Tasks.Application.Interfaces;

public interface IConversationService
{
    Task<ConversationResponse> CreateConversationAsync(Guid siteId, CreateConversationRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<ConversationResponse?> GetConversationByIdAsync(Guid siteId, Guid conversationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ConversationResponse>> GetConversationsByEntityAsync(Guid siteId, string relatedEntityType, Guid relatedEntityId, CancellationToken cancellationToken = default);
    Task<MessageResponse> PostMessageAsync(Guid siteId, Guid conversationId, PostMessageRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MessageResponse>> GetMessagesAsync(Guid siteId, Guid conversationId, int? limit, DateTimeOffset? since, CancellationToken cancellationToken = default);
    Task MarkMessageReadAsync(Guid siteId, Guid messageId, Guid userId, CancellationToken cancellationToken = default);
    Task AddParticipantAsync(Guid siteId, Guid conversationId, Guid userId, Guid addedByUserId, CancellationToken cancellationToken = default);
    Task RemoveParticipantAsync(Guid siteId, Guid conversationId, Guid userId, Guid removedByUserId, CancellationToken cancellationToken = default);
    Task<bool> IsUserParticipantAsync(Guid siteId, Guid conversationId, Guid userId, CancellationToken cancellationToken = default);
}
