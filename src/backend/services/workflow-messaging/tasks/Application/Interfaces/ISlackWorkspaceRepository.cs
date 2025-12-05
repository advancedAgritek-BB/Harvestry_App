using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SlackWorkspaceEntity = Harvestry.Tasks.Domain.Entities.SlackWorkspace;

namespace Harvestry.Tasks.Application.Interfaces;

public interface ISlackWorkspaceRepository
{
    Task AddAsync(SlackWorkspaceEntity workspace, CancellationToken cancellationToken);
    Task UpdateAsync(SlackWorkspaceEntity workspace, CancellationToken cancellationToken);
    Task<SlackWorkspaceEntity?> GetByIdAsync(Guid siteId, Guid slackWorkspaceId, CancellationToken cancellationToken);
    Task<SlackWorkspaceEntity?> GetByWorkspaceIdAsync(Guid siteId, string workspaceId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SlackWorkspaceEntity>> GetBySiteAsync(Guid siteId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
