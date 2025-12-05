using System;
using System.Collections.Generic;
using System.Linq;
using Harvestry.Tasks.Application.DTOs;
using DomainConversation = Harvestry.Tasks.Domain.Entities.Conversation;
using DomainMessage = Harvestry.Tasks.Domain.Entities.Message;
using DomainParticipant = Harvestry.Tasks.Domain.Entities.ConversationParticipant;

namespace Harvestry.Tasks.Application.Mappers;

public static class ConversationMapper
{
    public static ConversationResponse ToResponse(DomainConversation conversation, bool includeMessages = true)
    {
        if (conversation is null)
        {
            throw new ArgumentNullException(nameof(conversation));
        }

        var participants = conversation.Participants
            .Select(ToParticipantResponse)
            .ToArray();

        var messages = includeMessages
            ? conversation.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(ToMessageResponse)
                .ToArray()
            : Array.Empty<MessageResponse>();

        return new ConversationResponse
        {
            ConversationId = conversation.Id,
            SiteId = conversation.SiteId,
            Type = conversation.Type,
            Title = conversation.Title,
            RelatedEntityType = conversation.RelatedEntityType,
            RelatedEntityId = conversation.RelatedEntityId,
            Status = conversation.Status,
            CreatedByUserId = conversation.CreatedByUserId,
            CreatedAt = conversation.CreatedAt,
            UpdatedAt = conversation.UpdatedAt,
            LastMessageAt = conversation.LastMessageAt,
            Participants = participants,
            Messages = messages
        };
    }

    public static ConversationParticipantResponse ToParticipantResponse(DomainParticipant participant)
    {
        if (participant is null)
        {
            throw new ArgumentNullException(nameof(participant));
        }

        return new ConversationParticipantResponse
        {
            ConversationParticipantId = participant.Id,
            ConversationId = participant.ConversationId,
            UserId = participant.UserId,
            Role = participant.Role,
            JoinedAt = participant.JoinedAt,
            LastReadAt = participant.LastReadAt
        };
    }

    public static MessageResponse ToMessageResponse(DomainMessage message)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        return new MessageResponse
        {
            MessageId = message.Id,
            ConversationId = message.ConversationId,
            SiteId = message.SiteId,
            SenderUserId = message.SenderUserId,
            Content = message.Content,
            ParentMessageId = message.ParentMessageId,
            IsEdited = message.IsEdited,
            EditedAt = message.EditedAt,
            IsDeleted = message.IsDeleted,
            DeletedAt = message.DeletedAt,
            MetadataJson = message.MetadataJson,
            CreatedAt = message.CreatedAt,
            UpdatedAt = message.UpdatedAt,
            Attachments = message.Attachments
                .Select(a => new MessageAttachmentResponse
                {
                    MessageAttachmentId = a.Id,
                    AttachmentType = a.AttachmentType,
                    FileUrl = a.FileUrl,
                    FileName = a.FileName,
                    FileSizeBytes = a.FileSizeBytes,
                    MimeType = a.MimeType,
                    MetadataJson = a.MetadataJson,
                    CreatedAt = a.CreatedAt
                })
                .ToArray(),
            ReadReceipts = message.ReadReceipts
                .Select(r => new MessageReadReceiptResponse
                {
                    MessageReadReceiptId = r.Id,
                    MessageId = r.MessageId,
                    UserId = r.UserId,
                    ReadAt = r.ReadAt
                })
                .ToArray()
        };
    }

    public static IReadOnlyList<MessageResponse> ToMessageResponses(IEnumerable<DomainMessage> messages)
    {
        if (messages is null)
        {
            throw new ArgumentNullException(nameof(messages));
        }

        return messages.Select(ToMessageResponse).ToArray();
    }
}
