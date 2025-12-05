using System;
using Harvestry.Shared.Kernel.Domain;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.Domain.Entities;

public sealed class ConversationParticipant : Entity<Guid>
{
    private ConversationParticipant(
        Guid id,
        Guid conversationId,
        Guid siteId,
        Guid userId,
        ConversationParticipantRole role,
        DateTimeOffset joinedAt,
        DateTimeOffset? lastReadAt) : base(id)
    {
        if (conversationId == Guid.Empty)
        {
            throw new ArgumentException("Conversation identifier is required.", nameof(conversationId));
        }

        if (siteId == Guid.Empty)
        {
            throw new ArgumentException("Site identifier is required.", nameof(siteId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User identifier is required.", nameof(userId));
        }

        ConversationId = conversationId;
        SiteId = siteId;
        UserId = userId;
        Role = role == ConversationParticipantRole.Undefined ? ConversationParticipantRole.Participant : role;
        JoinedAt = joinedAt;
        LastReadAt = lastReadAt;
        Version = 1;
    }

    public Guid ConversationId { get; }
    public Guid SiteId { get; }
    public Guid UserId { get; }
    public ConversationParticipantRole Role { get; private set; }
    public DateTimeOffset JoinedAt { get; private set; }
    public DateTimeOffset? LastReadAt { get; private set; }
    public long Version { get; private set; }

    public static ConversationParticipant Create(
        Guid conversationId,
        Guid siteId,
        Guid userId,
        ConversationParticipantRole role,
        DateTimeOffset? joinedAt = null)
    {
        var timestamp = joinedAt ?? DateTimeOffset.UtcNow;
        return new ConversationParticipant(
            Guid.NewGuid(),
            conversationId,
            siteId,
            userId,
            role,
            timestamp,
            null);
    }

    public static ConversationParticipant FromPersistence(
        Guid id,
        Guid conversationId,
        Guid siteId,
        Guid userId,
        ConversationParticipantRole role,
        DateTimeOffset joinedAt,
        DateTimeOffset? lastReadAt)
    {
        return new ConversationParticipant(
            id,
            conversationId,
            siteId,
            userId,
            role,
            joinedAt,
            lastReadAt);
    }

    public void UpdateRole(ConversationParticipantRole role)
    {
        // Mirror constructor behavior: normalize Undefined to Participant
        Role = role == ConversationParticipantRole.Undefined ? ConversationParticipantRole.Participant : role;
        Version++;
    }

    public void MarkRead(DateTimeOffset readAt)
    {
        LastReadAt = readAt;
        Version++;
    }

    public void UpdateJoinedAt(DateTimeOffset joinedAt)
    {
        JoinedAt = joinedAt;
        Version++;
    }
}
