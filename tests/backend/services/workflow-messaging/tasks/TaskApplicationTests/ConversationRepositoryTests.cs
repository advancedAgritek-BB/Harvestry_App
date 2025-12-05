using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Domain.Enums;
using Harvestry.Tasks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using DomainConversation = Harvestry.Tasks.Domain.Entities.Conversation;

namespace Harvestry.Tasks.Application.Tests;

public sealed class ConversationRepositoryTests
{
    [Fact]
    public async Task AddAndRetrieveConversation_RoundTripsParticipantsAndMessages()
    {
        var options = new DbContextOptionsBuilder<TasksDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new TasksDbContext(options);
        var repository = new ConversationRepository(context, NullLogger<ConversationRepository>.Instance);

        var siteId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var relatedEntityId = Guid.NewGuid();

        var conversation = DomainConversation.Create(
            siteId,
            ConversationType.Task,
            creatorId,
            "Task workflow thread",
            "task",
            relatedEntityId);

        conversation.AddParticipant(creatorId, ConversationParticipantRole.Owner);
        var participantId = Guid.NewGuid();
        conversation.AddParticipant(participantId, ConversationParticipantRole.Participant);

        var message = conversation.AddMessage(creatorId, "Initial task created");
        message.MarkRead(participantId);

        await repository.AddAsync(conversation, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        var retrieved = await repository.GetByIdAsync(siteId, conversation.Id, CancellationToken.None);
        Assert.NotNull(retrieved);
        Assert.Equal(2, retrieved!.Participants.Count);
        Assert.Single(retrieved.Messages);
        Assert.Contains(retrieved.Participants, p => p.UserId == participantId && p.Role == ConversationParticipantRole.Participant);
        Assert.Contains(retrieved.Messages.Single().ReadReceipts, r => r.UserId == participantId);
        Assert.Equal(conversation.LastMessageAt?.UtcDateTime, retrieved.LastMessageAt?.UtcDateTime);
    }
}
