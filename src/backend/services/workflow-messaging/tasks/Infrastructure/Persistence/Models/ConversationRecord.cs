using System;
using System.Collections.Generic;

namespace Harvestry.Tasks.Infrastructure.Persistence.Models;

public sealed class ConversationRecord
{
    public Guid ConversationId { get; set; }
    public Guid SiteId { get; set; }
    public short ConversationType { get; set; }
    public string? Title { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public short Status { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? LastMessageAt { get; set; }

    public ICollection<ConversationParticipantRecord> Participants { get; set; } = new List<ConversationParticipantRecord>();
    public ICollection<MessageRecord> Messages { get; set; } = new List<MessageRecord>();
}
