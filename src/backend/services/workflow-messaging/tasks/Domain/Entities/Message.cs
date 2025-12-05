using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Harvestry.Shared.Kernel.Domain;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.Domain.Entities;

public sealed class Message : Entity<Guid>
{
    private readonly List<MessageAttachment> _attachments = new();
    private readonly List<MessageReadReceipt> _readReceipts = new();

    private Message(
        Guid id,
        Guid siteId,
        Guid conversationId,
        Guid senderUserId,
        string content,
        Guid? parentMessageId,
        bool isEdited,
        DateTimeOffset? editedAt,
        bool isDeleted,
        DateTimeOffset? deletedAt,
        string? metadataJson,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt) : base(id)
    {
        if (siteId == Guid.Empty)
        {
            throw new ArgumentException("Site identifier is required.", nameof(siteId));
        }

        if (conversationId == Guid.Empty)
        {
            throw new ArgumentException("Conversation identifier is required.", nameof(conversationId));
        }

        if (senderUserId == Guid.Empty)
        {
            throw new ArgumentException("Sender identifier is required.", nameof(senderUserId));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Message content is required.", nameof(content));
        }

        SiteId = siteId;
        ConversationId = conversationId;
        SenderUserId = senderUserId;
        Content = content.Trim();
        ParentMessageId = parentMessageId;
        IsEdited = isEdited;
        EditedAt = editedAt;
        IsDeleted = isDeleted;
        DeletedAt = deletedAt;
        MetadataJson = metadataJson;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid SiteId { get; }
    public Guid ConversationId { get; }
    public Guid SenderUserId { get; }
    public string Content { get; private set; }
    public Guid? ParentMessageId { get; private set; }
    public bool IsEdited { get; private set; }
    public DateTimeOffset? EditedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public string? MetadataJson { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<MessageAttachment> Attachments => _attachments.AsReadOnly();
    public IReadOnlyCollection<MessageReadReceipt> ReadReceipts => _readReceipts.AsReadOnly();

    public static Message Create(
        Guid siteId,
        Guid conversationId,
        Guid senderUserId,
        string content,
        Guid? parentMessageId = null,
        string? metadataJson = null,
        DateTimeOffset? createdAt = null)
    {
        var timestamp = createdAt ?? DateTimeOffset.UtcNow;
        return new Message(
            Guid.NewGuid(),
            siteId,
            conversationId,
            senderUserId,
            content,
            parentMessageId,
            isEdited: false,
            editedAt: null,
            isDeleted: false,
            deletedAt: null,
            metadataJson: string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson.Trim(),
            createdAt: timestamp,
            updatedAt: timestamp);
    }

    public static Message FromPersistence(
        Guid id,
        Guid siteId,
        Guid conversationId,
        Guid senderUserId,
        string content,
        Guid? parentMessageId,
        bool isEdited,
        DateTimeOffset? editedAt,
        bool isDeleted,
        DateTimeOffset? deletedAt,
        string? metadataJson,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        IEnumerable<MessageAttachment>? attachments,
        IEnumerable<MessageReadReceipt>? readReceipts)
    {
        var message = new Message(
            id,
            siteId,
            conversationId,
            senderUserId,
            content,
            parentMessageId,
            isEdited,
            editedAt,
            isDeleted,
            deletedAt,
            metadataJson,
            createdAt,
            updatedAt);

        if (attachments is not null)
        {
            message._attachments.AddRange(attachments.OrderBy(a => a.CreatedAt));
        }

        if (readReceipts is not null)
        {
            message._readReceipts.AddRange(readReceipts.OrderBy(r => r.ReadAt));
        }

        return message;
    }

    public void Edit(string content, string? metadataJson = null)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Message content is required.", nameof(content));
        }

        Content = content.Trim();
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson.Trim();
        IsEdited = true;
        EditedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkDeleted(string? reason = null)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;

        if (!string.IsNullOrWhiteSpace(reason))
        {
            var trimmedReason = reason.Trim();

            if (string.IsNullOrWhiteSpace(MetadataJson))
            {
                // No existing metadata, create new JSON object
                var metadata = new Dictionary<string, object>
                {
                    ["deletionReason"] = trimmedReason
                };
                MetadataJson = JsonSerializer.Serialize(metadata);
            }
            else
            {
                try
                {
                    // Try to parse existing metadata and add/overwrite deletion reason
                    var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(MetadataJson)
                        ?? new Dictionary<string, object>();
                    metadata["deletionReason"] = trimmedReason;
                    MetadataJson = JsonSerializer.Serialize(metadata);
                }
                catch (JsonException)
                {
                    // If parsing fails, wrap original string and reason into new JSON object
                    var metadata = new Dictionary<string, object>
                    {
                        ["originalMetadata"] = MetadataJson,
                        ["deletionReason"] = trimmedReason
                    };
                    MetadataJson = JsonSerializer.Serialize(metadata);
                }
            }
        }
    }

    public void Restore()
    {
        if (!IsDeleted)
        {
            return;
        }

        IsDeleted = false;
        DeletedAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public MessageAttachment AddAttachment(
        MessageAttachmentType attachmentType,
        string fileUrl,
        string? fileName = null,
        long? fileSizeBytes = null,
        string? mimeType = null,
        string? metadataJson = null)
    {
        var attachment = MessageAttachment.Create(
            Id,
            attachmentType,
            fileUrl,
            fileName,
            fileSizeBytes,
            mimeType,
            metadataJson);
        _attachments.Add(attachment);
        UpdatedAt = DateTimeOffset.UtcNow;
        return attachment;
    }

    public bool RemoveAttachment(Guid attachmentId)
    {
        var attachment = _attachments.FirstOrDefault(a => a.Id == attachmentId);
        if (attachment is null)
        {
            return false;
        }

        _attachments.Remove(attachment);
        UpdatedAt = DateTimeOffset.UtcNow;
        return true;
    }

    public MessageReadReceipt MarkRead(Guid userId, DateTimeOffset? readAt = null)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User identifier is required for marking message as read.", nameof(userId));
        }

        var existing = _readReceipts.FirstOrDefault(r => r.UserId == userId);
        if (existing is not null)
        {
            existing.UpdateReadAt(readAt ?? DateTimeOffset.UtcNow);
            return existing;
        }

        var receipt = MessageReadReceipt.Create(Id, SiteId, userId, readAt);
        _readReceipts.Add(receipt);
        return receipt;
    }
}
