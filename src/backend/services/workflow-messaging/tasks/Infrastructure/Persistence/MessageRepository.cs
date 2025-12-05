using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DomainMessage = Harvestry.Tasks.Domain.Entities.Message;
using DomainMessageAttachment = Harvestry.Tasks.Domain.Entities.MessageAttachment;
using DomainMessageReadReceipt = Harvestry.Tasks.Domain.Entities.MessageReadReceipt;
using AttachmentTypeEnum = Harvestry.Tasks.Domain.Enums.MessageAttachmentType;

namespace Harvestry.Tasks.Infrastructure.Persistence;

public sealed class MessageRepository : IMessageRepository
{
    private readonly TasksDbContext _dbContext;
    private readonly ILogger<MessageRepository> _logger;

    public MessageRepository(TasksDbContext dbContext, ILogger<MessageRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task AddAsync(DomainMessage message, CancellationToken cancellationToken)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        _logger.LogDebug("Persisting message {MessageId} for conversation {ConversationId}", message.Id, message.ConversationId);
        var record = ToRecord(message);
        await _dbContext.Messages.AddAsync(record, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(DomainMessage message, CancellationToken cancellationToken)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var record = await _dbContext.Messages
            .Include(m => m.Attachments)
            .Include(m => m.ReadReceipts)
            .FirstOrDefaultAsync(m => m.MessageId == message.Id && m.SiteId == message.SiteId, cancellationToken)
            .ConfigureAwait(false);

        if (record is null)
        {
            throw new InvalidOperationException($"Message {message.Id} could not be found for update.");
        }

        ApplyScalarProperties(record, message);
        SyncAttachments(record, message);
        SyncReadReceipts(record, message);
    }

    public async Task<DomainMessage?> GetByIdAsync(Guid siteId, Guid messageId, CancellationToken cancellationToken)
    {
        var record = await QueryableMessages()
            .FirstOrDefaultAsync(m => m.MessageId == messageId && m.SiteId == siteId, cancellationToken)
            .ConfigureAwait(false);

        return record is null ? null : ToDomain(record);
    }

    public async Task<IReadOnlyList<DomainMessage>> GetByConversationAsync(
        Guid siteId,
        Guid conversationId,
        int? limit,
        DateTimeOffset? since,
        CancellationToken cancellationToken)
    {
        var query = QueryableMessages()
            .Where(m => m.SiteId == siteId && m.ConversationId == conversationId);

        if (since.HasValue)
        {
            query = query.Where(m => m.CreatedAt >= since.Value);
        }

        query = query.OrderByDescending(m => m.CreatedAt).ThenByDescending(m => m.MessageId);

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        var records = await query
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var domainMessages = records
            .Select(ToDomain)
            .OrderBy(m => m.CreatedAt)
            .ThenBy(m => m.Id)
            .ToArray();

        return domainMessages;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<MessageRecord> QueryableMessages()
    {
        return _dbContext.Messages
            .Include(m => m.Attachments)
            .Include(m => m.ReadReceipts)
            .AsNoTracking();
    }

    private static void ApplyScalarProperties(MessageRecord record, DomainMessage message)
    {
        record.SiteId = message.SiteId;
        record.ConversationId = message.ConversationId;
        record.ParentMessageId = message.ParentMessageId;
        record.SenderUserId = message.SenderUserId;
        record.Content = message.Content;
        record.IsEdited = message.IsEdited;
        record.EditedAt = message.EditedAt;
        record.IsDeleted = message.IsDeleted;
        record.DeletedAt = message.DeletedAt;
        record.MetadataJson = message.MetadataJson;
        record.CreatedAt = message.CreatedAt;
        record.UpdatedAt = message.UpdatedAt;
    }

    private static MessageRecord ToRecord(DomainMessage message)
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

    private static DomainMessage ToDomain(MessageRecord record)
    {
        var attachments = record.Attachments
            .Select(a => DomainMessageAttachment.FromPersistence(
                a.MessageAttachmentId,
                a.MessageId,
                (AttachmentTypeEnum)a.AttachmentType,
                a.FileUrl,
                a.FileName,
                a.FileSizeBytes,
                a.MimeType,
                a.MetadataJson,
                a.CreatedAt))
            .ToArray();

        var receipts = record.ReadReceipts
            .Select(r => DomainMessageReadReceipt.FromPersistence(
                r.MessageReadReceiptId,
                r.MessageId,
                r.SiteId,
                r.UserId,
                r.ReadAt))
            .ToArray();

        return DomainMessage.FromPersistence(
            record.MessageId,
            record.SiteId,
            record.ConversationId,
            record.SenderUserId,
            record.Content,
            record.ParentMessageId,
            record.IsEdited,
            record.EditedAt,
            record.IsDeleted,
            record.DeletedAt,
            record.MetadataJson,
            record.CreatedAt,
            record.UpdatedAt,
            attachments,
            receipts);
    }

    private static void SyncAttachments(MessageRecord record, DomainMessage message)
    {
        var domainById = message.Attachments.ToDictionary(a => a.Id);
        var tracked = record.Attachments.ToDictionary(a => a.MessageAttachmentId);

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

        foreach (var attachment in message.Attachments)
        {
            if (tracked.ContainsKey(attachment.Id))
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

    private static void SyncReadReceipts(MessageRecord record, DomainMessage message)
    {
        var domainById = message.ReadReceipts.ToDictionary(r => r.Id);
        var tracked = record.ReadReceipts.ToDictionary(r => r.MessageReadReceiptId);

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

        foreach (var receipt in message.ReadReceipts)
        {
            if (tracked.ContainsKey(receipt.Id))
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
