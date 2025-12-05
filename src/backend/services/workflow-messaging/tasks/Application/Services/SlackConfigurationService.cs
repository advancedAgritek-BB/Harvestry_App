using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Domain.Enums;
using SlackChannelMappingEntity = Harvestry.Tasks.Domain.Entities.SlackChannelMapping;
using SlackWorkspaceEntity = Harvestry.Tasks.Domain.Entities.SlackWorkspace;
using Microsoft.Extensions.Logging;

namespace Harvestry.Tasks.Application.Services;

public sealed class SlackConfigurationService : ISlackConfigurationService
{
    private readonly ISlackWorkspaceRepository _workspaceRepository;
    private readonly ISlackChannelMappingRepository _channelMappingRepository;
    private readonly ILogger<SlackConfigurationService> _logger;

    public SlackConfigurationService(
        ISlackWorkspaceRepository workspaceRepository,
        ISlackChannelMappingRepository channelMappingRepository,
        ILogger<SlackConfigurationService> logger)
    {
        _workspaceRepository = workspaceRepository ?? throw new ArgumentNullException(nameof(workspaceRepository));
        _channelMappingRepository = channelMappingRepository ?? throw new ArgumentNullException(nameof(channelMappingRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<SlackWorkspaceResponse>> GetWorkspacesAsync(Guid siteId, CancellationToken cancellationToken)
    {
        var workspaces = await _workspaceRepository.GetBySiteAsync(siteId, cancellationToken).ConfigureAwait(false);
        return workspaces.Select(ToWorkspaceResponse).ToArray();
    }

    public async Task<SlackWorkspaceResponse> CreateOrUpdateWorkspaceAsync(Guid siteId, SlackWorkspaceRequest request, Guid userId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User identifier is required.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(request.WorkspaceId))
        {
            throw new ArgumentException("WorkspaceId is required.", nameof(request));
        }

        var existing = await _workspaceRepository
            .GetByWorkspaceIdAsync(siteId, request.WorkspaceId, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            _logger.LogInformation("Creating Slack workspace {WorkspaceId} for site {SiteId}", request.WorkspaceId, siteId);
            var workspace = SlackWorkspaceEntity.Create(
                siteId,
                request.WorkspaceId,
                request.WorkspaceName,
                request.BotToken,
                request.RefreshToken,
                userId);

            await _workspaceRepository.AddAsync(workspace, cancellationToken).ConfigureAwait(false);
            await _workspaceRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return ToWorkspaceResponse(workspace);
        }

        _logger.LogInformation("Updating Slack workspace {WorkspaceId} for site {SiteId}", request.WorkspaceId, siteId);
        existing.UpdateMetadata(request.WorkspaceName, request.IsActive);
        if (!string.IsNullOrWhiteSpace(request.BotToken) || !string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            existing.UpdateSecrets(
                !string.IsNullOrWhiteSpace(request.BotToken) ? request.BotToken : null,
                !string.IsNullOrWhiteSpace(request.RefreshToken) ? request.RefreshToken : null);
        }

        await _workspaceRepository.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
        await _workspaceRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return ToWorkspaceResponse(existing);
    }

    public async Task<IReadOnlyList<SlackChannelMappingResponse>> GetChannelMappingsAsync(Guid siteId, Guid slackWorkspaceId, CancellationToken cancellationToken)
    {
        var mappings = await _channelMappingRepository
            .GetByWorkspaceAsync(siteId, slackWorkspaceId, cancellationToken)
            .ConfigureAwait(false);

        return mappings.Select(ToChannelMappingResponse).ToArray();
    }

    public async Task<SlackChannelMappingResponse> CreateChannelMappingAsync(Guid siteId, Guid slackWorkspaceId, SlackChannelMappingRequest request, Guid userId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User identifier is required.", nameof(userId));
        }

        var workspace = await _workspaceRepository
            .GetByIdAsync(siteId, slackWorkspaceId, cancellationToken)
            .ConfigureAwait(false);

        if (workspace is null)
        {
            throw new KeyNotFoundException($"Slack workspace {slackWorkspaceId} was not found for site {siteId}.");
        }

        var mapping = SlackChannelMappingEntity.Create(
            siteId,
            slackWorkspaceId,
            request.ChannelId,
            request.ChannelName,
            ToNotificationKey(request.NotificationType),
            userId);

        await _channelMappingRepository.AddAsync(mapping, cancellationToken).ConfigureAwait(false);
        await _channelMappingRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return ToChannelMappingResponse(mapping);
    }

    public async Task DeleteChannelMappingAsync(Guid siteId, Guid mappingId, Guid userId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User identifier is required.", nameof(userId));
        }

        var mapping = await _channelMappingRepository
            .GetByIdAsync(siteId, mappingId, cancellationToken)
            .ConfigureAwait(false);

        if (mapping is null)
        {
            throw new KeyNotFoundException($"Slack channel mapping {mappingId} was not found for site {siteId}.");
        }

        mapping.Update(mapping.ChannelName, isActive: false);
        await _channelMappingRepository.UpdateAsync(mapping, cancellationToken).ConfigureAwait(false);
        await _channelMappingRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static SlackWorkspaceResponse ToWorkspaceResponse(SlackWorkspaceEntity workspace)
    {
        return new SlackWorkspaceResponse
        {
            SlackWorkspaceId = workspace.Id,
            SiteId = workspace.SiteId,
            WorkspaceId = workspace.WorkspaceId,
            WorkspaceName = workspace.WorkspaceName,
            IsActive = workspace.IsActive,
            InstalledAt = workspace.InstalledAt,
            InstalledBy = workspace.InstalledByUserId,
            LastVerifiedAt = workspace.LastVerifiedAt
        };
    }

    private static SlackChannelMappingResponse ToChannelMappingResponse(SlackChannelMappingEntity mapping)
    {
        return new SlackChannelMappingResponse
        {
            SlackChannelMappingId = mapping.Id,
            SlackWorkspaceId = mapping.SlackWorkspaceId,
            SiteId = mapping.SiteId,
            ChannelId = mapping.ChannelId,
            ChannelName = mapping.ChannelName,
            NotificationType = FromNotificationKey(mapping.NotificationType),
            IsActive = mapping.IsActive,
            CreatedAt = mapping.CreatedAt,
            CreatedBy = mapping.CreatedByUserId
        };
    }

    private static string ToNotificationKey(NotificationType notificationType) => notificationType switch
    {
        NotificationType.TaskCreated => "task_created",
        NotificationType.TaskAssigned => "task_assigned",
        NotificationType.TaskCompleted => "task_completed",
        NotificationType.TaskOverdue => "task_overdue",
        NotificationType.TaskBlocked => "task_blocked",
        NotificationType.ConversationMention => "conversation_mention",
        NotificationType.AlertCritical => "alert_critical",
        NotificationType.AlertWarning => "alert_warning",
        _ => "task_created"
    };

    private static NotificationType FromNotificationKey(string key) => key?.ToLowerInvariant() switch
    {
        "task_created" => NotificationType.TaskCreated,
        "task_assigned" => NotificationType.TaskAssigned,
        "task_completed" => NotificationType.TaskCompleted,
        "task_overdue" => NotificationType.TaskOverdue,
        "task_blocked" => NotificationType.TaskBlocked,
        "conversation_mention" => NotificationType.ConversationMention,
        "alert_critical" => NotificationType.AlertCritical,
        "alert_warning" => NotificationType.AlertWarning,
        _ => NotificationType.Undefined
    };
}
