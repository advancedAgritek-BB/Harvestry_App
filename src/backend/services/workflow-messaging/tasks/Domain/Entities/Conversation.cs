using System;
using System.Collections.Generic;
using System.Linq;
using Harvestry.Shared.Kernel.Domain;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.Domain.Entities;

public sealed class Conversation : AggregateRoot<Guid>
{
    private readonly List<ConversationParticipant> _participants = new();
    private readonly List<Message> _messages = new();

    private Conversation(
        Guid id,
        Guid siteId,
        ConversationType type,
        string? title,
        string? relatedEntityType,
        Guid? relatedEntityId,
        ConversationStatus status,
        Guid createdByUserId,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        DateTimeOffset? lastMessageAt) : base(id)
    {
        if (siteId == Guid.Empty)
        {
            throw new ArgumentException("Site identifier is required.", nameof(siteId));
        }

        if (createdByUserId == Guid.Empty)
        {
            throw new ArgumentException("Created by identifier is required.", nameof(createdByUserId));
        }

        SiteId = siteId;
        Type = type == ConversationType.Undefined ? ConversationType.General : type;
        Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim();
        RelatedEntityType = string.IsNullOrWhiteSpace(relatedEntityType) ? null : relatedEntityType.Trim();
        RelatedEntityId = relatedEntityId;
        Status = status == ConversationStatus.Undefined ? ConversationStatus.Active : status;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        LastMessageAt = lastMessageAt;
    }

    public Guid SiteId { get; }
    public ConversationType Type { get; private set; }
    public string? Title { get; private set; }
    public string? RelatedEntityType { get; private set; }
    public Guid? RelatedEntityId { get; private set; }
    public ConversationStatus Status { get; private set; }
    public Guid CreatedByUserId { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? LastMessageAt { get; private set; }

    public IReadOnlyCollection<ConversationParticipant> Participants => _participants.AsReadOnly();
    public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();

    public static Conversation Create(
        Guid siteId,
        ConversationType type,
        Guid createdByUserId,
        string? title,
        string? relatedEntityType,
        Guid? relatedEntityId)
    {
        var now = DateTimeOffset.UtcNow;
        return new Conversation(
            Guid.NewGuid(),
            siteId,
            type,
            title,
            relatedEntityType,
            relatedEntityId,
            ConversationStatus.Active,
            createdByUserId,
            now,
            now,
            null);
    }

    public static Conversation FromPersistence(
        Guid id,
        Guid siteId,
        ConversationType type,
        string? title,
        string? relatedEntityType,
        Guid? relatedEntityId,
        ConversationStatus status,
        Guid createdByUserId,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        DateTimeOffset? lastMessageAt,
        IEnumerable<ConversationParticipant>? participants,
        IEnumerable<Message>? messages)
    {
        var conversation = new Conversation(
            id,
            siteId,
            type,
            title,
            relatedEntityType,
            relatedEntityId,
            status,
            createdByUserId,
            createdAt,
            updatedAt,
            lastMessageAt);

        if (participants is not null)
        {
            conversation._participants.AddRange(participants.OrderBy(p => p.JoinedAt));
        }

        if (messages is not null)
        {
            conversation._messages.AddRange(messages.OrderBy(m => m.CreatedAt));
        }

        return conversation;
    }

    public ConversationParticipant AddParticipant(
        Guid userId,
        ConversationParticipantRole role,
        DateTimeOffset? joinedAt = null)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("Participant identifier is required.", nameof(userId));
        }

        var existing = _participants.FirstOrDefault(p => p.UserId == userId);
        if (existing is not null)
        {
            // Note: Repository must enforce optimistic locking using participant.Version
            // to prevent lost updates under concurrent modifications
            if (role != ConversationParticipantRole.Undefined && existing.Role != role)
            {
                existing.UpdateRole(role);
            }

            if (joinedAt.HasValue)
            {
                existing.UpdateJoinedAt(joinedAt.Value);
            }

            return existing;
        }

        var participant = ConversationParticipant.Create(Id, SiteId, userId, role, joinedAt);
        _participants.Add(participant);
        UpdatedAt = DateTimeOffset.UtcNow;
        return participant;
    }

    public bool RemoveParticipant(Guid userId)
    {
        var participant = _participants.FirstOrDefault(p => p.UserId == userId);
        if (participant is null)
        {
            return false;
        }

        _participants.Remove(participant);
        UpdatedAt = DateTimeOffset.UtcNow;
        return true;
    }

    public Message AddMessage(
        Guid senderUserId,
        string content,
        Guid? parentMessageId = null,
        string? metadataJson = null,
        DateTimeOffset? createdAt = null)
    {
        var message = Message.Create(
            SiteId,
            Id,
            senderUserId,
            content,
            parentMessageId,
            metadataJson,
            createdAt);

        _messages.Add(message);
        LastMessageAt = message.CreatedAt;
        UpdatedAt = DateTimeOffset.UtcNow;
        return message;
    }

    public void SetTitle(string? title)
    {
        Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetRelatedEntity(string? entityType, Guid? entityId)
    {
        RelatedEntityType = string.IsNullOrWhiteSpace(entityType) ? null : entityType.Trim();
        RelatedEntityId = entityId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateType(ConversationType type)
    {
        if (type == ConversationType.Undefined)
        {
            throw new ArgumentException("Conversation type must be defined.", nameof(type));
        }

        Type = type;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Archive()
    {
        if (Status == ConversationStatus.Archived)
        {
            return;
        }

        Status = ConversationStatus.Archived;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        if (Status == ConversationStatus.Active)
        {
            return;
        }

        Status = ConversationStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Lock()
    {
        if (Status == ConversationStatus.Locked)
        {
            return;
        }

        Status = ConversationStatus.Locked;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Unlock()
    {
        if (Status != ConversationStatus.Locked)
        {
            return;
        }

        Status = ConversationStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsUserParticipant(Guid userId)
    {
        return _participants.Any(p => p.UserId == userId);
    }
}
