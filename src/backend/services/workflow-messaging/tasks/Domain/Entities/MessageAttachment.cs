using System;
using Harvestry.Shared.Kernel.Domain;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.Domain.Entities;

public sealed class MessageAttachment : Entity<Guid>
{
    private MessageAttachment(
        Guid id,
        Guid messageId,
        MessageAttachmentType attachmentType,
        string fileUrl,
        string? fileName,
        long? fileSizeBytes,
        string? mimeType,
        string? metadataJson,
        DateTimeOffset createdAt) : base(id)
    {
        if (messageId == Guid.Empty)
        {
            throw new ArgumentException("Message identifier is required.", nameof(messageId));
        }

        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            throw new ArgumentException("File URL is required.", nameof(fileUrl));
        }

        MessageId = messageId;
        AttachmentType = attachmentType == MessageAttachmentType.Undefined ? MessageAttachmentType.File : attachmentType;
        FileUrl = fileUrl.Trim();
        FileName = string.IsNullOrWhiteSpace(fileName) ? null : fileName.Trim();
        FileSizeBytes = fileSizeBytes;
        MimeType = string.IsNullOrWhiteSpace(mimeType) ? null : mimeType.Trim();
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson.Trim();
        CreatedAt = createdAt;
    }

    public Guid MessageId { get; }
    public MessageAttachmentType AttachmentType { get; private set; }
    public string? FileName { get; private set; }
    public string FileUrl { get; private set; }
    public long? FileSizeBytes { get; private set; }
    public string? MimeType { get; private set; }
    public string? MetadataJson { get; private set; }
    public DateTimeOffset CreatedAt { get; }

    public static MessageAttachment Create(
        Guid messageId,
        MessageAttachmentType attachmentType,
        string fileUrl,
        string? fileName,
        long? fileSizeBytes,
        string? mimeType,
        string? metadataJson)
    {
        return new MessageAttachment(
            Guid.NewGuid(),
            messageId,
            attachmentType,
            fileUrl,
            fileName,
            fileSizeBytes,
            mimeType,
            metadataJson,
            DateTimeOffset.UtcNow);
    }

    public static MessageAttachment FromPersistence(
        Guid id,
        Guid messageId,
        MessageAttachmentType attachmentType,
        string fileUrl,
        string? fileName,
        long? fileSizeBytes,
        string? mimeType,
        string? metadataJson,
        DateTimeOffset createdAt)
    {
        return new MessageAttachment(
            id,
            messageId,
            attachmentType,
            fileUrl,
            fileName,
            fileSizeBytes,
            mimeType,
            metadataJson,
            createdAt);
    }

    public void UpdateMetadata(string? metadataJson)
    {
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson.Trim();
    }
}
