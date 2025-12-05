using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Application.Mappers;
using Harvestry.Tasks.Domain.Enums;
using Microsoft.Extensions.Logging;
using DomainConversation = Harvestry.Tasks.Domain.Entities.Conversation;

namespace Harvestry.Tasks.Application.Services;

public sealed class ConversationService : IConversationService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly ILogger<ConversationService> _logger;

    public ConversationService(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        ILogger<ConversationService> logger)
    {
        _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ConversationResponse> CreateConversationAsync(
        Guid siteId,
        CreateConversationRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (siteId == Guid.Empty)
        {
            throw new ArgumentException("Site identifier is required.", nameof(siteId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User identifier is required.", nameof(userId));
        }

        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        _logger.LogDebug("Creating conversation for site {SiteId} by user {UserId}", siteId, userId);

        var conversation = DomainConversation.Create(
            siteId,
            request.Type,
            userId,
            request.Title,
            request.RelatedEntityType,
            request.RelatedEntityId);

        conversation.AddParticipant(userId, ConversationParticipantRole.Owner);

        if (request.ParticipantUserIds is not null)
        {
            foreach (var participantId in request.ParticipantUserIds.Distinct())
            {
                if (participantId == Guid.Empty || participantId == userId)
                {
                    continue;
                }

                conversation.AddParticipant(participantId, ConversationParticipantRole.Participant);
            }
        }

        await _conversationRepository.AddAsync(conversation, cancellationToken).ConfigureAwait(false);
        await _conversationRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return ConversationMapper.ToResponse(conversation);
    }

    public async Task<ConversationResponse?> GetConversationByIdAsync(
        Guid siteId,
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        if (siteId == Guid.Empty)
        {
            throw new ArgumentException("Site identifier is required.", nameof(siteId));
        }

        if (conversationId == Guid.Empty)
        {
            throw new ArgumentException("Conversation identifier is required.", nameof(conversationId));
        }

        var conversation = await _conversationRepository
            .GetByIdAsync(siteId, conversationId, cancellationToken)
            .ConfigureAwait(false);

        return conversation is null ? null : ConversationMapper.ToResponse(conversation);
    }

    public async Task<IReadOnlyList<ConversationResponse>> GetConversationsByEntityAsync(
        Guid siteId,
        string relatedEntityType,
        Guid relatedEntityId,
        CancellationToken cancellationToken = default)
    {
        if (siteId == Guid.Empty)
        {
            throw new ArgumentException("Site identifier is required.", nameof(siteId));
        }

        if (string.IsNullOrWhiteSpace(relatedEntityType))
        {
            throw new ArgumentException("Related entity type is required.", nameof(relatedEntityType));
        }

        if (relatedEntityId == Guid.Empty)
        {
            throw new ArgumentException("Related entity identifier is required.", nameof(relatedEntityId));
        }

        var conversations = await _conversationRepository
            .GetByRelatedEntityAsync(siteId, relatedEntityType, relatedEntityId, cancellationToken)
            .ConfigureAwait(false);

        return conversations
            .Select(conversation => ConversationMapper.ToResponse(conversation, includeMessages: false))
            .ToArray();
    }

    public async Task<MessageResponse> PostMessageAsync(
        Guid siteId,
        Guid conversationId,
        PostMessageRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (siteId == Guid.Empty)
        {
            throw new ArgumentException("Site identifier is required.", nameof(siteId));
        }

        if (conversationId == Guid.Empty)
        {
            throw new ArgumentException("Conversation identifier is required.", nameof(conversationId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User identifier is required.", nameof(userId));
        }

        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException("Message content is required.", nameof(request));
        }

        var conversation = await _conversationRepository
            .GetByIdAsync(siteId, conversationId, cancellationToken)
            .ConfigureAwait(false);

        if (conversation is null)
        {
            throw new KeyNotFoundException($"Conversation {conversationId} was not found for site {siteId}.");
        }

        var participant = conversation.Participants.FirstOrDefault(p => p.UserId == userId);
        if (participant is null)
        {
            conversation.AddParticipant(userId, ConversationParticipantRole.Participant);
        }

        var message = conversation.AddMessage(
            userId,
            request.Content,
            request.ParentMessageId,
            request.MetadataJson);

        if (request.Attachments is not null)
        {
            foreach (var attachment in request.Attachments)
            {
                if (attachment is null)
                {
                    continue;
                }

                message.AddAttachment(
                    attachment.AttachmentType,
                    attachment.FileUrl,
                    attachment.FileName,
                    attachment.FileSizeBytes,
                    attachment.MimeType,
                    attachment.MetadataJson);
            }
        }

        await _conversationRepository.UpdateAsync(conversation, cancellationToken).ConfigureAwait(false);
        await _conversationRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return ConversationMapper.ToMessageResponse(message);
    }

    public async Task<IReadOnlyList<MessageResponse>> GetMessagesAsync(
        Guid siteId,
        Guid conversationId,
        int? limit,
        DateTimeOffset? since,
        CancellationToken cancellationToken = default)
    {
        if (siteId == Guid.Empty)
        {
            throw new ArgumentException("Site identifier is required.", nameof(siteId));
        }

        if (conversationId == Guid.Empty)
        {
            throw new ArgumentException("Conversation identifier is required.", nameof(conversationId));
        }

        if (limit.HasValue && limit.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be greater than zero when provided.");
        }

        var messages = await _messageRepository
            .GetByConversationAsync(siteId, conversationId, limit, since, cancellationToken)
            .ConfigureAwait(false);

        if (messages.Count == 0)
        {
            var conversation = await _conversationRepository
                .GetByIdAsync(siteId, conversationId, cancellationToken)
                .ConfigureAwait(false);

            if (conversation is null)
            {
                throw new KeyNotFoundException($"Conversation {conversationId} was not found for site {siteId}.");
            }

            return Array.Empty<MessageResponse>();
        }

        return ConversationMapper.ToMessageResponses(messages);
    }

    public async Task MarkMessageReadAsync(
        Guid siteId,
        Guid messageId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (siteId == Guid.Empty)
        {
            throw new ArgumentException("Site identifier is required.", nameof(siteId));
        }

        if (messageId == Guid.Empty)
        {
            throw new ArgumentException("Message identifier is required.", nameof(messageId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User identifier is required.", nameof(userId));
        }

        var message = await _messageRepository
            .GetByIdAsync(siteId, messageId, cancellationToken)
            .ConfigureAwait(false);

        if (message is null)
        {
            throw new KeyNotFoundException($"Message {messageId} was not found for site {siteId}.");
        }

        var receipt = message.MarkRead(userId);
        await _messageRepository.UpdateAsync(message, cancellationToken).ConfigureAwait(false);

        var conversation = await _conversationRepository
            .GetByIdAsync(siteId, message.ConversationId, cancellationToken)
            .ConfigureAwait(false);

        if (conversation is not null)
        {
            var participant = conversation.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant is null)
            {
                participant = conversation.AddParticipant(userId, ConversationParticipantRole.Participant, receipt.ReadAt);
            }

            participant.MarkRead(receipt.ReadAt);

            await _conversationRepository.UpdateAsync(conversation, cancellationToken).ConfigureAwait(false);
        }

        // Persist through the shared DbContext once all aggregates are synchronized.
        await _conversationRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task AddParticipantAsync(
        Guid siteId,
        Guid conversationId,
        Guid userId,
        Guid addedByUserId,
        CancellationToken cancellationToken = default)
    {
        if (siteId == Guid.Empty)
        {
            throw new ArgumentException("Site identifier is required.", nameof(siteId));
        }

        if (conversationId == Guid.Empty)
        {
            throw new ArgumentException("Conversation identifier is required.", nameof(conversationId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User identifier is required.", nameof(userId));
        }

        if (addedByUserId == Guid.Empty)
        {
            throw new ArgumentException("Added-by identifier is required.", nameof(addedByUserId));
        }

        var conversation = await _conversationRepository
            .GetByIdAsync(siteId, conversationId, cancellationToken)
            .ConfigureAwait(false);

        if (conversation is null)
        {
            throw new KeyNotFoundException($"Conversation {conversationId} was not found for site {siteId}.");
        }

        if (conversation.Participants.Any(p => p.UserId == userId))
        {
            return;
        }

        conversation.AddParticipant(userId, ConversationParticipantRole.Participant);
        await _conversationRepository.UpdateAsync(conversation, cancellationToken).ConfigureAwait(false);
        await _conversationRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveParticipantAsync(
        Guid siteId,
        Guid conversationId,
        Guid userId,
        Guid removedByUserId,
        CancellationToken cancellationToken = default)
    {
        if (siteId == Guid.Empty)
        {
            throw new ArgumentException("Site identifier is required.", nameof(siteId));
        }

        if (conversationId == Guid.Empty)
        {
            throw new ArgumentException("Conversation identifier is required.", nameof(conversationId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User identifier is required.", nameof(userId));
        }

        if (removedByUserId == Guid.Empty)
        {
            throw new ArgumentException("Removed-by identifier is required.", nameof(removedByUserId));
        }

        var conversation = await _conversationRepository
            .GetByIdAsync(siteId, conversationId, cancellationToken)
            .ConfigureAwait(false);

        if (conversation is null)
        {
            throw new KeyNotFoundException($"Conversation {conversationId} was not found for site {siteId}.");
        }

        var participant = conversation.Participants.FirstOrDefault(p => p.UserId == userId);
        if (participant is null)
        {
            return;
        }

        if (participant.Role == ConversationParticipantRole.Owner)
        {
            throw new InvalidOperationException("Cannot remove the owner from the conversation.");
        }

        if (!conversation.RemoveParticipant(userId))
        {
            return;
        }

        await _conversationRepository.UpdateAsync(conversation, cancellationToken).ConfigureAwait(false);
        await _conversationRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> IsUserParticipantAsync(Guid siteId, Guid conversationId, Guid userId, CancellationToken cancellationToken = default)
    {
        if (siteId == Guid.Empty)
        {
            throw new ArgumentException("Site identifier is required.", nameof(siteId));
        }

        if (conversationId == Guid.Empty)
        {
            throw new ArgumentException("Conversation identifier is required.", nameof(conversationId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User identifier is required.", nameof(userId));
        }

        var conversation = await _conversationRepository
            .GetByIdAsync(siteId, conversationId, cancellationToken)
            .ConfigureAwait(false);
        if (conversation is null)
        {
            return false;
        }

        return conversation.IsUserParticipant(userId);
    }
}
