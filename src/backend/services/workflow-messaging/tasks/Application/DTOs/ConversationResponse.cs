using System;
using System.Collections.Generic;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.Application.DTOs;

public sealed class ConversationResponse
{
    public Guid ConversationId { get; init; }
    public Guid SiteId { get; init; }
    public ConversationType Type { get; init; }
    public string? Title { get; init; }
    public string? RelatedEntityType { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public ConversationStatus Status { get; init; }
    public Guid CreatedByUserId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public DateTimeOffset? LastMessageAt { get; init; }
    public IReadOnlyList<ConversationParticipantResponse> Participants { get; init; } = Array.Empty<ConversationParticipantResponse>();
    public IReadOnlyList<MessageResponse> Messages { get; init; } = Array.Empty<MessageResponse>();
}

public sealed class ConversationParticipantResponse
{
    public Guid ConversationParticipantId { get; init; }
    public Guid ConversationId { get; init; }
    public Guid UserId { get; init; }
    public ConversationParticipantRole Role { get; init; }
    public DateTimeOffset JoinedAt { get; init; }
    public DateTimeOffset? LastReadAt { get; init; }
}

public sealed class MessageResponse
{
    public Guid MessageId { get; init; }
    public Guid ConversationId { get; init; }
    public Guid SiteId { get; init; }
    public Guid SenderUserId { get; init; }
    public string Content { get; init; } = string.Empty;
    public Guid? ParentMessageId { get; init; }
    public bool IsEdited { get; init; }
    public DateTimeOffset? EditedAt { get; init; }
    public bool IsDeleted { get; init; }
    public DateTimeOffset? DeletedAt { get; init; }
    public string? MetadataJson { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public IReadOnlyList<MessageAttachmentResponse> Attachments { get; init; } = Array.Empty<MessageAttachmentResponse>();
    public IReadOnlyList<MessageReadReceiptResponse> ReadReceipts { get; init; } = Array.Empty<MessageReadReceiptResponse>();
}

public sealed class MessageAttachmentResponse
{
    public Guid MessageAttachmentId { get; init; }
    public MessageAttachmentType AttachmentType { get; init; }
    public string FileUrl { get; init; } = string.Empty;
    public string? FileName { get; init; }
    public long? FileSizeBytes { get; init; }
    public string? MimeType { get; init; }
    public string? MetadataJson { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed class MessageReadReceiptResponse
{
    public Guid MessageReadReceiptId { get; init; }
    public Guid MessageId { get; init; }
    public Guid UserId { get; init; }
    public DateTimeOffset ReadAt { get; init; }
}
