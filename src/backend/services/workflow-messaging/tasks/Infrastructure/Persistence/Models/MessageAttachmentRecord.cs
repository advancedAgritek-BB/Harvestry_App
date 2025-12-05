using System;

namespace Harvestry.Tasks.Infrastructure.Persistence.Models;

public sealed class MessageAttachmentRecord
{
    public Guid MessageAttachmentId { get; set; }
    public Guid MessageId { get; set; }
    public short AttachmentType { get; set; }
    public string? FileName { get; set; }
    public string FileUrl { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? MimeType { get; set; }
    public string? MetadataJson { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public MessageRecord Message { get; set; } = null!;
}
