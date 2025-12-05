using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SystemTask = System.Threading.Tasks.Task;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Application.Services;
using Harvestry.Tasks.Domain.Entities;
using Harvestry.Tasks.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using DomainConversation = Harvestry.Tasks.Domain.Entities.Conversation;
using DomainMessage = Harvestry.Tasks.Domain.Entities.Message;
using DomainMessageAttachment = Harvestry.Tasks.Domain.Entities.MessageAttachment;
using DomainMessageReadReceipt = Harvestry.Tasks.Domain.Entities.MessageReadReceipt;
using DomainParticipant = Harvestry.Tasks.Domain.Entities.ConversationParticipant;

namespace Harvestry.Tasks.Application.Tests;

public sealed class ConversationServiceTests
{
    private readonly Mock<IConversationRepository> _conversationRepositoryMock = new();
    private readonly Mock<IMessageRepository> _messageRepositoryMock = new();
    private readonly ConversationService _service;

    public ConversationServiceTests()
    {
        var logger = new Mock<ILogger<ConversationService>>();
        _service = new ConversationService(
            _conversationRepositoryMock.Object,
            _messageRepositoryMock.Object,
            logger.Object);
    }

    [Fact]
    public async SystemTask CreateConversationAsync_AddsOwnerAndParticipants()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var invitedUserId = Guid.NewGuid();
        DomainConversation? capturedConversation = null;

        _conversationRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<DomainConversation>(), It.IsAny<CancellationToken>()))
            .Callback<DomainConversation, CancellationToken>((conversation, _) => capturedConversation = conversation)
            .Returns(SystemTask.CompletedTask);

        _conversationRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(SystemTask.CompletedTask);

        var request = new CreateConversationRequest
        {
            Type = ConversationType.General,
            Title = "Operations",
            ParticipantUserIds = new[] { invitedUserId }
        };

        // Act
        var response = await _service.CreateConversationAsync(siteId, request, ownerUserId, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedConversation);
        Assert.Equal(response.ConversationId, capturedConversation!.Id);
        Assert.Equal(siteId, capturedConversation.SiteId);
        Assert.Equal(2, capturedConversation.Participants.Count);
        Assert.Contains(capturedConversation.Participants, p => p.UserId == ownerUserId && p.Role == ConversationParticipantRole.Owner);
        Assert.Contains(capturedConversation.Participants, p => p.UserId == invitedUserId && p.Role == ConversationParticipantRole.Participant);
        _conversationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<DomainConversation>(), It.IsAny<CancellationToken>()), Times.Once);
        _conversationRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async SystemTask PostMessageAsync_PersistsMessageAndAddsSenderParticipant()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var senderUserId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var ownerParticipant = DomainParticipant.FromPersistence(
            Guid.NewGuid(),
            conversationId,
            siteId,
            ownerUserId,
            ConversationParticipantRole.Owner,
            now.AddMinutes(-5),
            lastReadAt: null);

        var conversation = DomainConversation.FromPersistence(
            conversationId,
            siteId,
            ConversationType.General,
            "Ops",
            relatedEntityType: null,
            relatedEntityId: null,
            ConversationStatus.Active,
            ownerUserId,
            createdAt: now.AddHours(-1),
            updatedAt: now.AddHours(-1),
            lastMessageAt: null,
            participants: new[] { ownerParticipant },
            messages: Array.Empty<DomainMessage>());

        DomainConversation? updatedConversation = null;

        _conversationRepositoryMock
            .Setup(r => r.GetByIdAsync(siteId, conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        _conversationRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<DomainConversation>(), It.IsAny<CancellationToken>()))
            .Callback<DomainConversation, CancellationToken>((c, _) => updatedConversation = c)
            .Returns(SystemTask.CompletedTask);

        _conversationRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(SystemTask.CompletedTask);

        var request = new PostMessageRequest
        {
            Content = "Need calibration results",
            MetadataJson = "{\"priority\":\"high\"}",
            Attachments = new[]
            {
                new PostMessageAttachmentRequest
                {
                    AttachmentType = MessageAttachmentType.File,
                    FileUrl = "https://files.example.com/report.pdf",
                    FileName = "report.pdf",
                    MimeType = "application/pdf",
                    FileSizeBytes = 2048
                }
            }
        };

        // Act
        var response = await _service.PostMessageAsync(siteId, conversationId, request, senderUserId, CancellationToken.None);

        // Assert
        Assert.NotNull(updatedConversation);
        Assert.Contains(updatedConversation!.Participants, p => p.UserId == senderUserId);
        Assert.Single(updatedConversation.Messages);
        Assert.Equal(request.Content, response.Content);
        Assert.Single(response.Attachments);
        _conversationRepositoryMock.Verify(r => r.UpdateAsync(conversation, It.IsAny<CancellationToken>()), Times.Once);
        _conversationRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async SystemTask GetMessagesAsync_ReturnsProjectedResponses()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow.AddMinutes(-10);

        var attachments = new[]
        {
            DomainMessageAttachment.FromPersistence(
                Guid.NewGuid(),
                messageId,
                MessageAttachmentType.Image,
                "https://files.example.com/photo.jpg",
                "photo.jpg",
                1024,
                "image/jpeg",
                metadataJson: null,
                createdAt: createdAt)
        };

        var receipts = new[]
        {
            DomainMessageReadReceipt.FromPersistence(
                Guid.NewGuid(),
                messageId,
                siteId,
                Guid.NewGuid(),
                createdAt.AddMinutes(5))
        };

        var domainMessage = DomainMessage.FromPersistence(
            messageId,
            siteId,
            conversationId,
            senderUserId: Guid.NewGuid(),
            content: "Checklist complete",
            parentMessageId: null,
            isEdited: false,
            editedAt: null,
            isDeleted: false,
            deletedAt: null,
            metadataJson: null,
            createdAt,
            updatedAt: createdAt,
            attachments,
            receipts);

        _messageRepositoryMock
            .Setup(r => r.GetByConversationAsync(siteId, conversationId, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainMessage> { domainMessage });

        // Act
        var responses = await _service.GetMessagesAsync(siteId, conversationId, limit: null, since: null, CancellationToken.None);

        // Assert
        var response = Assert.Single(responses);
        Assert.Equal(domainMessage.Content, response.Content);
        Assert.Single(response.Attachments);
        Assert.Single(response.ReadReceipts);
        _messageRepositoryMock.Verify(r => r.GetByConversationAsync(siteId, conversationId, null, null, It.IsAny<CancellationToken>()), Times.Once);
        _conversationRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async SystemTask MarkMessageReadAsync_AddsReceiptAndUpdatesConversation()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var readerUserId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow.AddMinutes(-20);

        var domainMessage = DomainMessage.FromPersistence(
            messageId,
            siteId,
            conversationId,
            senderUserId: ownerUserId,
            content: "Please review",
            parentMessageId: null,
            isEdited: false,
            editedAt: null,
            isDeleted: false,
            deletedAt: null,
            metadataJson: null,
            createdAt,
            updatedAt: createdAt,
            attachments: Array.Empty<DomainMessageAttachment>(),
            readReceipts: Array.Empty<DomainMessageReadReceipt>());

        var ownerParticipant = DomainParticipant.FromPersistence(
            Guid.NewGuid(),
            conversationId,
            siteId,
            ownerUserId,
            ConversationParticipantRole.Owner,
            createdAt,
            lastReadAt: null);

        var conversation = DomainConversation.FromPersistence(
            conversationId,
            siteId,
            ConversationType.General,
            "Calibrations",
            relatedEntityType: null,
            relatedEntityId: null,
            ConversationStatus.Active,
            ownerUserId,
            createdAt,
            updatedAt: createdAt,
            lastMessageAt: createdAt,
            participants: new[] { ownerParticipant },
            messages: new[] { domainMessage });

        _messageRepositoryMock
            .Setup(r => r.GetByIdAsync(siteId, messageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(domainMessage);

        _messageRepositoryMock
            .Setup(r => r.UpdateAsync(domainMessage, It.IsAny<CancellationToken>()))
            .Returns(SystemTask.CompletedTask);

        _conversationRepositoryMock
            .Setup(r => r.GetByIdAsync(siteId, conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        _conversationRepositoryMock
            .Setup(r => r.UpdateAsync(conversation, It.IsAny<CancellationToken>()))
            .Returns(SystemTask.CompletedTask);

        _conversationRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(SystemTask.CompletedTask);

        // Act
        await _service.MarkMessageReadAsync(siteId, messageId, readerUserId, CancellationToken.None);

        // Assert
        Assert.Single(domainMessage.ReadReceipts);
        Assert.Equal(readerUserId, domainMessage.ReadReceipts.First().UserId);
        Assert.Contains(conversation.Participants, p => p.UserId == readerUserId && p.LastReadAt.HasValue);
        _messageRepositoryMock.Verify(r => r.UpdateAsync(domainMessage, It.IsAny<CancellationToken>()), Times.Once);
        _conversationRepositoryMock.Verify(r => r.UpdateAsync(conversation, It.IsAny<CancellationToken>()), Times.Once);
        _conversationRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
