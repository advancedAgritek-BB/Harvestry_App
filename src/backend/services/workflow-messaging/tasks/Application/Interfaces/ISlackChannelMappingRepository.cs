using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SlackChannelMappingEntity = Harvestry.Tasks.Domain.Entities.SlackChannelMapping;

namespace Harvestry.Tasks.Application.Interfaces;

public interface ISlackChannelMappingRepository
{
    Task AddAsync(SlackChannelMappingEntity mapping, CancellationToken cancellationToken);
    Task UpdateAsync(SlackChannelMappingEntity mapping, CancellationToken cancellationToken);
    Task<SlackChannelMappingEntity?> GetByIdAsync(Guid siteId, Guid mappingId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SlackChannelMappingEntity>> GetBySiteAsync(Guid siteId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SlackChannelMappingEntity>> GetByWorkspaceAsync(Guid siteId, Guid workspaceId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
