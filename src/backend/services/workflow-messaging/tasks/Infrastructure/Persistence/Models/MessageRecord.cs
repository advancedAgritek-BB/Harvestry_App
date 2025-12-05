using System;
using System.Collections.Generic;

namespace Harvestry.Tasks.Infrastructure.Persistence.Models;

public sealed class MessageRecord
{
    public Guid MessageId { get; set; }
    public Guid SiteId { get; set; }
    public Guid ConversationId { get; set; }
    public Guid? ParentMessageId { get; set; }
    public Guid SenderUserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsEdited { get; set; }
    public DateTimeOffset? EditedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? MetadataJson { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ConversationRecord Conversation { get; set; } = null!;
    public MessageRecord? ParentMessage { get; set; }
    public ICollection<MessageAttachmentRecord> Attachments { get; set; } = new List<MessageAttachmentRecord>();
    public ICollection<MessageReadReceiptRecord> ReadReceipts { get; set; } = new List<MessageReadReceiptRecord>();
}
