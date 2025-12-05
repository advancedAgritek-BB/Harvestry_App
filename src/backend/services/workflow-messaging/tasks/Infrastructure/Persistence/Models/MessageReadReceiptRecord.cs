using System;

namespace Harvestry.Tasks.Infrastructure.Persistence.Models;

public sealed class MessageReadReceiptRecord
{
    public Guid MessageReadReceiptId { get; set; }
    public Guid MessageId { get; set; }
    public Guid SiteId { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset ReadAt { get; set; }

    public MessageRecord Message { get; set; } = null!;
}
