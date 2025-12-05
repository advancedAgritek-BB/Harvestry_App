using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Domain.Enums;
using SlackChannelMappingEntity = Harvestry.Tasks.Domain.Entities.SlackChannelMapping;
using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harvestry.Tasks.Infrastructure.Persistence;

public sealed class SlackChannelMappingRepository : ISlackChannelMappingRepository
{
    private readonly TasksDbContext _dbContext;
    private readonly ILogger<SlackChannelMappingRepository> _logger;

    public SlackChannelMappingRepository(TasksDbContext dbContext, ILogger<SlackChannelMappingRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task AddAsync(SlackChannelMappingEntity mapping, CancellationToken cancellationToken)
    {
        var record = ToRecord(mapping);
        await _dbContext.SlackChannelMappings.AddAsync(record, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(SlackChannelMappingEntity mapping, CancellationToken cancellationToken)
    {
        var record = await _dbContext.SlackChannelMappings
            .FirstOrDefaultAsync(x => x.SlackChannelMappingId == mapping.Id && x.SiteId == mapping.SiteId, cancellationToken)
            .ConfigureAwait(false);

        if (record is null)
        {
            throw new InvalidOperationException($"Slack channel mapping {mapping.Id} could not be found for update.");
        }

        ApplyScalarProperties(record, mapping);
    }

    public async Task<SlackChannelMappingEntity?> GetByIdAsync(Guid siteId, Guid mappingId, CancellationToken cancellationToken)
    {
        var record = await _dbContext.SlackChannelMappings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SiteId == siteId && x.SlackChannelMappingId == mappingId, cancellationToken)
            .ConfigureAwait(false);

        return record is null ? null : ToDomain(record);
    }

    public async Task<IReadOnlyList<SlackChannelMappingEntity>> GetBySiteAsync(Guid siteId, CancellationToken cancellationToken)
    {
        var records = await _dbContext.SlackChannelMappings
            .AsNoTracking()
            .Where(x => x.SiteId == siteId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return records.Select(ToDomain).ToArray();
    }

    public async Task<IReadOnlyList<SlackChannelMappingEntity>> GetByWorkspaceAsync(Guid siteId, Guid workspaceId, CancellationToken cancellationToken)
    {
        var records = await _dbContext.SlackChannelMappings
            .AsNoTracking()
            .Where(x => x.SiteId == siteId && x.SlackWorkspaceId == workspaceId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return records.Select(ToDomain).ToArray();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static SlackChannelMappingEntity ToDomain(SlackChannelMappingRecord record)
    {
        return SlackChannelMappingEntity.FromPersistence(
            record.SlackChannelMappingId,
            record.SiteId,
            record.SlackWorkspaceId,
            record.ChannelId,
            record.ChannelName,
            record.NotificationType.ToString().ToLowerInvariant(),
            record.IsActive,
            record.CreatedByUserId,
            record.CreatedAt);
    }

    private static SlackChannelMappingRecord ToRecord(SlackChannelMappingEntity mapping)
    {
        return new SlackChannelMappingRecord
        {
            SlackChannelMappingId = mapping.Id,
            SiteId = mapping.SiteId,
            SlackWorkspaceId = mapping.SlackWorkspaceId,
            ChannelId = mapping.ChannelId,
            ChannelName = mapping.ChannelName,
            NotificationType = Enum.Parse<NotificationType>(mapping.NotificationType, true),
            IsActive = mapping.IsActive,
            CreatedByUserId = mapping.CreatedByUserId,
            CreatedAt = mapping.CreatedAt
        };
    }

    private static void ApplyScalarProperties(SlackChannelMappingRecord record, SlackChannelMappingEntity mapping)
    {
        record.SlackWorkspaceId = mapping.SlackWorkspaceId;
        record.ChannelId = mapping.ChannelId;
        record.ChannelName = mapping.ChannelName;
        record.NotificationType = Enum.Parse<NotificationType>(mapping.NotificationType, true);
        record.IsActive = mapping.IsActive;
    }
}
