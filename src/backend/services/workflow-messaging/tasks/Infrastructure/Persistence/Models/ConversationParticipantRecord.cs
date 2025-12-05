using System;

namespace Harvestry.Tasks.Infrastructure.Persistence.Models;

public sealed class ConversationParticipantRecord
{
    public Guid ConversationParticipantId { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SiteId { get; set; }
    public Guid UserId { get; set; }
    public short Role { get; set; }
    public DateTimeOffset JoinedAt { get; set; }
    public DateTimeOffset? LastReadAt { get; set; }

    public ConversationRecord Conversation { get; set; } = null!;
}
