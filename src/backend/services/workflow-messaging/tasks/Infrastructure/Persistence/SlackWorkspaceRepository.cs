using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.Interfaces;
using SlackWorkspaceEntity = Harvestry.Tasks.Domain.Entities.SlackWorkspace;
using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harvestry.Tasks.Infrastructure.Persistence;

public sealed class SlackWorkspaceRepository : ISlackWorkspaceRepository
{
    private readonly TasksDbContext _dbContext;
    private readonly ILogger<SlackWorkspaceRepository> _logger;

    public SlackWorkspaceRepository(TasksDbContext dbContext, ILogger<SlackWorkspaceRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task AddAsync(SlackWorkspaceEntity workspace, CancellationToken cancellationToken)
    {
        var record = ToRecord(workspace);
        await _dbContext.SlackWorkspaces.AddAsync(record, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(SlackWorkspaceEntity workspace, CancellationToken cancellationToken)
    {
        var record = await _dbContext.SlackWorkspaces
            .FirstOrDefaultAsync(x => x.SlackWorkspaceId == workspace.Id && x.SiteId == workspace.SiteId, cancellationToken)
            .ConfigureAwait(false);

        if (record is null)
        {
            throw new InvalidOperationException($"Slack workspace {workspace.Id} could not be found for update.");
        }

        ApplyScalarProperties(record, workspace);
    }

    public async Task<SlackWorkspaceEntity?> GetByIdAsync(Guid siteId, Guid slackWorkspaceId, CancellationToken cancellationToken)
    {
        var record = await _dbContext.SlackWorkspaces
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SiteId == siteId && x.SlackWorkspaceId == slackWorkspaceId, cancellationToken)
            .ConfigureAwait(false);

        return record is null ? null : ToDomain(record);
    }

    public async Task<SlackWorkspaceEntity?> GetByWorkspaceIdAsync(Guid siteId, string workspaceId, CancellationToken cancellationToken)
    {
        var record = await _dbContext.SlackWorkspaces
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SiteId == siteId && x.WorkspaceId == workspaceId, cancellationToken)
            .ConfigureAwait(false);

        return record is null ? null : ToDomain(record);
    }

    public async Task<IReadOnlyList<SlackWorkspaceEntity>> GetBySiteAsync(Guid siteId, CancellationToken cancellationToken)
    {
        var records = await _dbContext.SlackWorkspaces
            .AsNoTracking()
            .Where(x => x.SiteId == siteId)
            .OrderByDescending(x => x.InstalledAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return records.Select(ToDomain).ToArray();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static SlackWorkspaceEntity ToDomain(SlackWorkspaceRecord record)
    {
        return SlackWorkspaceEntity.FromPersistence(
            record.SlackWorkspaceId,
            record.SiteId,
            record.WorkspaceId,
            record.WorkspaceName,
            record.BotTokenEncrypted,
            record.RefreshTokenEncrypted,
            record.IsActive,
            record.InstalledByUserId,
            record.InstalledAt,
            record.LastVerifiedAt);
    }

    private static SlackWorkspaceRecord ToRecord(SlackWorkspaceEntity workspace)
    {
        return new SlackWorkspaceRecord
        {
            SlackWorkspaceId = workspace.Id,
            SiteId = workspace.SiteId,
            WorkspaceId = workspace.WorkspaceId,
            WorkspaceName = workspace.WorkspaceName,
            BotTokenEncrypted = workspace.EncryptedBotToken,
            RefreshTokenEncrypted = workspace.EncryptedRefreshToken,
            IsActive = workspace.IsActive,
            InstalledByUserId = workspace.InstalledByUserId,
            InstalledAt = workspace.InstalledAt,
            LastVerifiedAt = workspace.LastVerifiedAt
        };
    }

    private static void ApplyScalarProperties(SlackWorkspaceRecord record, SlackWorkspaceEntity workspace)
    {
        record.WorkspaceName = workspace.WorkspaceName;
        record.BotTokenEncrypted = workspace.EncryptedBotToken;
        record.RefreshTokenEncrypted = workspace.EncryptedRefreshToken;
        record.IsActive = workspace.IsActive;
        record.LastVerifiedAt = workspace.LastVerifiedAt;
    }
}
