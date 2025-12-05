using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.DTOs;

namespace Harvestry.Tasks.Application.Interfaces;

public interface ISlackConfigurationService
{
    Task<IReadOnlyList<SlackWorkspaceResponse>> GetWorkspacesAsync(Guid siteId, CancellationToken cancellationToken);
    Task<SlackWorkspaceResponse> CreateOrUpdateWorkspaceAsync(Guid siteId, SlackWorkspaceRequest request, Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SlackChannelMappingResponse>> GetChannelMappingsAsync(Guid siteId, Guid slackWorkspaceId, CancellationToken cancellationToken);
    Task<SlackChannelMappingResponse> CreateChannelMappingAsync(Guid siteId, Guid slackWorkspaceId, SlackChannelMappingRequest request, Guid userId, CancellationToken cancellationToken);
    Task DeleteChannelMappingAsync(Guid siteId, Guid mappingId, Guid userId, CancellationToken cancellationToken);
}
