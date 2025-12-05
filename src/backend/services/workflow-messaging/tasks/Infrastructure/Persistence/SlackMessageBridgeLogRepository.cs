using System;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Domain.Enums;
using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using SlackMessageBridgeLogEntity = Harvestry.Tasks.Domain.Entities.SlackMessageBridgeLog;

namespace Harvestry.Tasks.Infrastructure.Persistence;

public sealed class SlackMessageBridgeLogRepository : ISlackMessageBridgeLogRepository
{
    private readonly TasksDbContext _dbContext;

    public SlackMessageBridgeLogRepository(TasksDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<SlackMessageBridgeLogEntity?> GetByRequestIdAsync(string requestId, Guid slackWorkspaceId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(requestId))
        {
            throw new ArgumentException("Request identifier is required.", nameof(requestId));
        }

        if (slackWorkspaceId == Guid.Empty)
        {
            throw new ArgumentException("Slack workspace identifier is required.", nameof(slackWorkspaceId));
        }

        var record = await _dbContext.SlackMessageBridgeLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.RequestId == requestId && x.SlackWorkspaceId == slackWorkspaceId,
                cancellationToken)
            .ConfigureAwait(false);

        return record is null ? null : ToDomain(record);
    }

    public async Task AddAsync(SlackMessageBridgeLogEntity logEntry, CancellationToken cancellationToken)
    {
        if (logEntry is null)
        {
            throw new ArgumentNullException(nameof(logEntry));
        }

        await _dbContext.SlackMessageBridgeLogs
            .AddAsync(ToRecord(logEntry), cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task UpdateAsync(SlackMessageBridgeLogEntity logEntry, CancellationToken cancellationToken)
    {
        if (logEntry is null)
        {
            throw new ArgumentNullException(nameof(logEntry));
        }

        var record = await _dbContext.SlackMessageBridgeLogs
            .FirstOrDefaultAsync(x => x.SlackMessageBridgeLogId == logEntry.Id, cancellationToken)
            .ConfigureAwait(false);

        if (record is null)
        {
            throw new InvalidOperationException($"Slack message bridge log {logEntry.Id} could not be found for update.");
        }

        ApplyScalarProperties(record, logEntry);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static SlackMessageBridgeLogEntity ToDomain(SlackMessageBridgeLogRecord record)
    {
        return SlackMessageBridgeLogEntity.FromPersistence(
            record.SlackMessageBridgeLogId,
            record.SiteId,
            record.SlackWorkspaceId,
            record.InternalMessageId,
            record.InternalMessageType,
            record.SlackChannelId,
            record.SlackMessageTs,
            record.SlackThreadTs,
            record.RequestId,
            record.Status,
            record.AttemptCount,
            record.CreatedAt,
            record.LastAttemptAt,
            record.ErrorMessage,
            record.SentAt);
    }

    private static SlackMessageBridgeLogRecord ToRecord(SlackMessageBridgeLogEntity entity)
    {
        return new SlackMessageBridgeLogRecord
        {
            SlackMessageBridgeLogId = entity.Id,
            SiteId = entity.SiteId,
            SlackWorkspaceId = entity.SlackWorkspaceId,
            InternalMessageId = entity.InternalMessageId,
            InternalMessageType = entity.InternalMessageType,
            SlackChannelId = entity.SlackChannelId,
            SlackMessageTs = entity.SlackMessageTs,
            SlackThreadTs = entity.SlackThreadTs,
            RequestId = entity.RequestId,
            Status = entity.Status,
            AttemptCount = entity.AttemptCount,
            CreatedAt = entity.CreatedAt,
            LastAttemptAt = entity.LastAttemptAt,
            ErrorMessage = entity.ErrorMessage,
            SentAt = entity.SentAt
        };
    }

    private static void ApplyScalarProperties(SlackMessageBridgeLogRecord record, SlackMessageBridgeLogEntity entity)
    {
        record.SiteId = entity.SiteId;
        record.SlackWorkspaceId = entity.SlackWorkspaceId;
        record.InternalMessageId = entity.InternalMessageId;
        record.InternalMessageType = entity.InternalMessageType;
        record.SlackChannelId = entity.SlackChannelId;
        record.SlackMessageTs = entity.SlackMessageTs;
        record.SlackThreadTs = entity.SlackThreadTs;
        record.RequestId = entity.RequestId;
        record.Status = entity.Status;
        record.AttemptCount = entity.AttemptCount;
        record.CreatedAt = entity.CreatedAt;
        record.LastAttemptAt = entity.LastAttemptAt;
        record.ErrorMessage = entity.ErrorMessage;
        record.SentAt = entity.SentAt;
    }
}
