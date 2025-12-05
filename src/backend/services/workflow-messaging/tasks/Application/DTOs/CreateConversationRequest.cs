using System;
using System.Collections.Generic;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.Application.DTOs;

public sealed class CreateConversationRequest
{
    public ConversationType Type { get; init; }
    public string? Title { get; init; }
    public string? RelatedEntityType { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public IReadOnlyCollection<Guid>? ParticipantUserIds { get; init; }
}
