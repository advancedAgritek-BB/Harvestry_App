using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Domain.Enums;
using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DomainConversation = Harvestry.Tasks.Domain.Entities.Conversation;
using DomainMessage = Harvestry.Tasks.Domain.Entities.Message;
using DomainMessageAttachment = Harvestry.Tasks.Domain.Entities.MessageAttachment;
using DomainMessageReadReceipt = Harvestry.Tasks.Domain.Entities.MessageReadReceipt;
using DomainParticipant = Harvestry.Tasks.Domain.Entities.ConversationParticipant;

namespace Harvestry.Tasks.Infrastructure.Persistence;

public sealed class ConversationRepository : IConversationRepository
{
    private readonly TasksDbContext _dbContext;
    private readonly ILogger<ConversationRepository> _logger;

    public ConversationRepository(TasksDbContext dbContext, ILogger<ConversationRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task AddAsync(DomainConversation conversation, CancellationToken cancellationToken)
    {
        if (conversation is null)
        {
            throw new ArgumentNullException(nameof(conversation));
        }

        var record = ToRecord(conversation);
        await _dbContext.Conversations.AddAsync(record, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(DomainConversation conversation, CancellationToken cancellationToken)
    {
        if (conversation is null)
        {
            throw new ArgumentNullException(nameof(conversation));
        }

        var record = await _dbContext.Conversations
            .Include(x => x.Participants)
            .Include(x => x.Messages)
                .ThenInclude(m => m.Attachments)
            .Include(x => x.Messages)
                .ThenInclude(m => m.ReadReceipts)
            .FirstOrDefaultAsync(x => x.ConversationId == conversation.Id && x.SiteId == conversation.SiteId, cancellationToken)
            .ConfigureAwait(false);

        if (record is null)
        {
            throw new InvalidOperationException($"Conversation {conversation.Id} could not be found for update.");
        }

        ApplyScalarProperties(record, conversation);
        SyncParticipants(record, conversation);
        SyncMessages(record, conversation);
    }

    public async Task<DomainConversation?> GetByIdAsync(Guid siteId, Guid conversationId, CancellationToken cancellationToken)
    {
        var record = await QueryableConversations()
            .FirstOrDefaultAsync(x => x.ConversationId == conversationId && x.SiteId == siteId, cancellationToken)
            .ConfigureAwait(false);

        return record is null ? null : ToDomain(record);
    }

    public async Task<IReadOnlyList<DomainConversation>> GetBySiteAsync(Guid siteId, ConversationType? type, CancellationToken cancellationToken)
    {
        var query = QueryableConversations().Where(x => x.SiteId == siteId);

        if (type.HasValue && type.Value != ConversationType.Undefined)
        {
            query = query.Where(x => x.ConversationType == (short)type.Value);
        }

        var records = await query
            .OrderByDescending(x => x.LastMessageAt ?? x.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return records.Select(ToDomain).ToArray();
    }

    public async Task<IReadOnlyList<DomainConversation>> GetByRelatedEntityAsync(
        Guid siteId,
        string relatedEntityType,
        Guid relatedEntityId,
        CancellationToken cancellationToken)
    {
        var query = QueryableConversations()
            .Where(x => x.SiteId == siteId && x.RelatedEntityId == relatedEntityId);

        if (!string.IsNullOrWhiteSpace(relatedEntityType))
        {
            query = query.Where(x => x.RelatedEntityType == relatedEntityType);
        }

        var records = await query
            .OrderByDescending(x => x.LastMessageAt ?? x.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return records.Select(ToDomain).ToArray();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<ConversationRecord> QueryableConversations()
    {
        return _dbContext.Conversations
            .Include(x => x.Participants)
            .Include(x => x.Messages)
                .ThenInclude(m => m.Attachments)
            .Include(x => x.Messages)
                .ThenInclude(m => m.ReadReceipts)
            .AsNoTracking();
    }

    private static DomainConversation ToDomain(ConversationRecord record)
    {
        var participants = record.Participants
            .Select(p => DomainParticipant.FromPersistence(
                p.ConversationParticipantId,
                p.ConversationId,
                p.SiteId,
                p.UserId,
                (ConversationParticipantRole)p.Role,
                p.JoinedAt,
                p.LastReadAt))
            .ToArray();

        var messages = record.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => DomainMessage.FromPersistence(
                m.MessageId,
                m.SiteId,
                m.ConversationId,
                m.SenderUserId,
                m.Content,
                m.ParentMessageId,
                m.IsEdited,
                m.EditedAt,
                m.IsDeleted,
                m.DeletedAt,
                m.MetadataJson,
                m.CreatedAt,
                m.UpdatedAt,
                m.Attachments
                    .Select(a => DomainMessageAttachment.FromPersistence(
                        a.MessageAttachmentId,
                        a.MessageId,
                        (MessageAttachmentType)a.AttachmentType,
                        a.FileUrl,
                        a.FileName,
                        a.FileSizeBytes,
                        a.MimeType,
                        a.MetadataJson,
                        a.CreatedAt))
                    .ToArray(),
                m.ReadReceipts
                    .Select(r => DomainMessageReadReceipt.FromPersistence(
                        r.MessageReadReceiptId,
                        r.MessageId,
                        r.SiteId,
                        r.UserId,
                        r.ReadAt))
                    .ToArray()))
            .ToArray();

        return DomainConversation.FromPersistence(
            record.ConversationId,
            record.SiteId,
            (ConversationType)record.ConversationType,
            record.Title,
            record.RelatedEntityType,
            record.RelatedEntityId,
            (ConversationStatus)record.Status,
            record.CreatedByUserId,
            record.CreatedAt,
            record.UpdatedAt,
            record.LastMessageAt,
            participants,
            messages);
    }

    private static ConversationRecord ToRecord(DomainConversation conversation)
    {
        var record = new ConversationRecord
        {
            ConversationId = conversation.Id,
            SiteId = conversation.SiteId,
            ConversationType = (short)conversation.Type,
            Title = conversation.Title,
            RelatedEntityType = conversation.RelatedEntityType,
            RelatedEntityId = conversation.RelatedEntityId,
            Status = (short)conversation.Status,
            CreatedByUserId = conversation.CreatedByUserId,
            CreatedAt = conversation.CreatedAt,
            UpdatedAt = conversation.UpdatedAt,
            LastMessageAt = conversation.LastMessageAt
        };

        foreach (var participant in conversation.Participants)
        {
            record.Participants.Add(new ConversationParticipantRecord
            {
                ConversationParticipantId = participant.Id,
                ConversationId = participant.ConversationId,
                SiteId = participant.SiteId,
                UserId = participant.UserId,
                Role = (short)participant.Role,
                JoinedAt = participant.JoinedAt,
                LastReadAt = participant.LastReadAt
            });
        }

        foreach (var message in conversation.Messages)
        {
            record.Messages.Add(MessageToRecord(message));
        }

        return record;
    }

    private static MessageRecord MessageToRecord(DomainMessage message)
    {
        var record = new MessageRecord
        {
            MessageId = message.Id,
            SiteId = message.SiteId,
            ConversationId = message.ConversationId,
            ParentMessageId = message.ParentMessageId,
            SenderUserId = message.SenderUserId,
            Content = message.Content,
            IsEdited = message.IsEdited,
            EditedAt = message.EditedAt,
            IsDeleted = message.IsDeleted,
            DeletedAt = message.DeletedAt,
            MetadataJson = message.MetadataJson,
            CreatedAt = message.CreatedAt,
            UpdatedAt = message.UpdatedAt
        };

        foreach (var attachment in message.Attachments)
        {
            record.Attachments.Add(new MessageAttachmentRecord
            {
                MessageAttachmentId = attachment.Id,
                MessageId = attachment.MessageId,
                AttachmentType = (short)attachment.AttachmentType,
                FileName = attachment.FileName,
                FileUrl = attachment.FileUrl,
                FileSizeBytes = attachment.FileSizeBytes,
                MimeType = attachment.MimeType,
                MetadataJson = attachment.MetadataJson,
                CreatedAt = attachment.CreatedAt
            });
        }

        foreach (var receipt in message.ReadReceipts)
        {
            record.ReadReceipts.Add(new MessageReadReceiptRecord
            {
                MessageReadReceiptId = receipt.Id,
                MessageId = receipt.MessageId,
                SiteId = receipt.SiteId,
                UserId = receipt.UserId,
                ReadAt = receipt.ReadAt
            });
        }

        return record;
    }

    private static void ApplyScalarProperties(ConversationRecord record, DomainConversation conversation)
    {
        record.SiteId = conversation.SiteId;
        record.ConversationType = (short)conversation.Type;
        record.Title = conversation.Title;
        record.RelatedEntityType = conversation.RelatedEntityType;
        record.RelatedEntityId = conversation.RelatedEntityId;
        record.Status = (short)conversation.Status;
        record.CreatedByUserId = conversation.CreatedByUserId;
        record.CreatedAt = conversation.CreatedAt;
        record.UpdatedAt = conversation.UpdatedAt;
        record.LastMessageAt = conversation.LastMessageAt;
    }

    private static void SyncParticipants(ConversationRecord record, DomainConversation conversation)
    {
        var domainById = conversation.Participants.ToDictionary(p => p.Id);
        var existingById = record.Participants.ToDictionary(p => p.ConversationParticipantId);

        foreach (var existing in record.Participants.ToArray())
        {
            if (!domainById.TryGetValue(existing.ConversationParticipantId, out var domain))
            {
                record.Participants.Remove(existing);
                continue;
            }

            existing.SiteId = domain.SiteId;
            existing.UserId = domain.UserId;
            existing.Role = (short)domain.Role;
            existing.JoinedAt = domain.JoinedAt;
            existing.LastReadAt = domain.LastReadAt;
        }

        foreach (var domain in conversation.Participants)
        {
            if (existingById.ContainsKey(domain.Id))
            {
                continue;
            }

            record.Participants.Add(new ConversationParticipantRecord
            {
                ConversationParticipantId = domain.Id,
                ConversationId = domain.ConversationId,
                SiteId = domain.SiteId,
                UserId = domain.UserId,
                Role = (short)domain.Role,
                JoinedAt = domain.JoinedAt,
                LastReadAt = domain.LastReadAt
            });
        }
    }

    private static void SyncMessages(ConversationRecord record, DomainConversation conversation)
    {
        var domainById = conversation.Messages.ToDictionary(m => m.Id);
        var existingById = record.Messages.ToDictionary(m => m.MessageId);

        foreach (var existing in record.Messages.ToArray())
        {
            if (!domainById.TryGetValue(existing.MessageId, out var domain))
            {
                record.Messages.Remove(existing);
                continue;
            }

            existing.SiteId = domain.SiteId;
            existing.ConversationId = domain.ConversationId;
            existing.ParentMessageId = domain.ParentMessageId;
            existing.SenderUserId = domain.SenderUserId;
            existing.Content = domain.Content;
            existing.IsEdited = domain.IsEdited;
            existing.EditedAt = domain.EditedAt;
            existing.IsDeleted = domain.IsDeleted;
            existing.DeletedAt = domain.DeletedAt;
            existing.MetadataJson = domain.MetadataJson;
            existing.CreatedAt = domain.CreatedAt;
            existing.UpdatedAt = domain.UpdatedAt;

            SyncAttachments(existing, domain);
            SyncReadReceipts(existing, domain);
        }

        foreach (var domain in conversation.Messages)
        {
            if (existingById.ContainsKey(domain.Id))
            {
                continue;
            }

            record.Messages.Add(MessageToRecord(domain));
        }
    }

    private static void SyncAttachments(MessageRecord record, DomainMessage domain)
    {
        var domainById = domain.Attachments.ToDictionary(a => a.Id);
        var existingById = record.Attachments.ToDictionary(a => a.MessageAttachmentId);

        foreach (var existing in record.Attachments.ToArray())
        {
            if (!domainById.TryGetValue(existing.MessageAttachmentId, out var attachment))
            {
                record.Attachments.Remove(existing);
                continue;
            }

            existing.AttachmentType = (short)attachment.AttachmentType;
            existing.FileName = attachment.FileName;
            existing.FileUrl = attachment.FileUrl;
            existing.FileSizeBytes = attachment.FileSizeBytes;
            existing.MimeType = attachment.MimeType;
            existing.MetadataJson = attachment.MetadataJson;
            existing.CreatedAt = attachment.CreatedAt;
        }

        foreach (var attachment in domain.Attachments)
        {
            if (existingById.ContainsKey(attachment.Id))
            {
                continue;
            }

            record.Attachments.Add(new MessageAttachmentRecord
            {
                MessageAttachmentId = attachment.Id,
                MessageId = attachment.MessageId,
                AttachmentType = (short)attachment.AttachmentType,
                FileName = attachment.FileName,
                FileUrl = attachment.FileUrl,
                FileSizeBytes = attachment.FileSizeBytes,
                MimeType = attachment.MimeType,
                MetadataJson = attachment.MetadataJson,
                CreatedAt = attachment.CreatedAt
            });
        }
    }

    private static void SyncReadReceipts(MessageRecord record, DomainMessage domain)
    {
        var domainById = domain.ReadReceipts.ToDictionary(r => r.Id);
        var existingById = record.ReadReceipts.ToDictionary(r => r.MessageReadReceiptId);

        foreach (var existing in record.ReadReceipts.ToArray())
        {
            if (!domainById.TryGetValue(existing.MessageReadReceiptId, out var receipt))
            {
                record.ReadReceipts.Remove(existing);
                continue;
            }

            existing.SiteId = receipt.SiteId;
            existing.UserId = receipt.UserId;
            existing.ReadAt = receipt.ReadAt;
        }

        foreach (var receipt in domain.ReadReceipts)
        {
            if (existingById.ContainsKey(receipt.Id))
            {
                continue;
            }

            record.ReadReceipts.Add(new MessageReadReceiptRecord
            {
                MessageReadReceiptId = receipt.Id,
                MessageId = receipt.MessageId,
                SiteId = receipt.SiteId,
                UserId = receipt.UserId,
                ReadAt = receipt.ReadAt
            });
        }
    }
}
