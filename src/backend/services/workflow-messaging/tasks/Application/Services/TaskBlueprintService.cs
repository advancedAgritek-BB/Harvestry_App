using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Application.Mappers;
using Harvestry.Tasks.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Harvestry.Tasks.Application.Services;

public sealed class TaskBlueprintService : ITaskBlueprintService
{
    private readonly ITaskBlueprintRepository _blueprintRepository;
    private readonly ILogger<TaskBlueprintService> _logger;

    public TaskBlueprintService(
        ITaskBlueprintRepository blueprintRepository,
        ILogger<TaskBlueprintService> logger)
    {
        _blueprintRepository = blueprintRepository ?? throw new ArgumentNullException(nameof(blueprintRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TaskBlueprintResponse> CreateBlueprintAsync(
        Guid siteId,
        CreateTaskBlueprintRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        _logger.LogInformation(
            "Creating task blueprint '{Title}' for site {SiteId} by user {UserId}",
            request.Title,
            siteId,
            userId);

        var blueprint = TaskBlueprint.Create(
            siteId,
            request.Title,
            request.Description,
            request.GrowthPhase,
            request.RoomType,
            request.StrainId,
            request.Priority,
            TimeSpan.FromHours(request.TimeOffsetHours),
            request.AssignedToRole,
            request.RequiredSopIds,
            request.RequiredTrainingIds,
            userId);

        await _blueprintRepository.AddAsync(blueprint, cancellationToken).ConfigureAwait(false);
        await _blueprintRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Task blueprint {BlueprintId} created successfully", blueprint.Id);
        return TaskBlueprintMapper.ToResponse(blueprint);
    }

    public async Task<TaskBlueprintResponse?> GetBlueprintByIdAsync(
        Guid siteId,
        Guid blueprintId,
        CancellationToken cancellationToken = default)
    {
        var blueprint = await _blueprintRepository
            .GetByIdAsync(siteId, blueprintId, cancellationToken)
            .ConfigureAwait(false);

        return blueprint is null ? null : TaskBlueprintMapper.ToResponse(blueprint);
    }

    public async Task<IReadOnlyList<TaskBlueprintResponse>> GetBlueprintsBySiteAsync(
        Guid siteId,
        bool? activeOnly,
        CancellationToken cancellationToken = default)
    {
        var blueprints = await _blueprintRepository
            .GetBySiteAsync(siteId, activeOnly, cancellationToken)
            .ConfigureAwait(false);

        return blueprints.Select(TaskBlueprintMapper.ToResponse).ToArray();
    }

    public async Task<TaskBlueprintResponse> UpdateBlueprintAsync(
        Guid siteId,
        Guid blueprintId,
        UpdateTaskBlueprintRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var blueprint = await EnsureBlueprintAsync(siteId, blueprintId, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Updating task blueprint {BlueprintId} for site {SiteId} by user {UserId}",
            blueprintId,
            siteId,
            userId);

        blueprint.Update(
            request.Title,
            request.Description,
            request.GrowthPhase,
            request.RoomType,
            request.StrainId,
            request.Priority,
            TimeSpan.FromHours(request.TimeOffsetHours),
            request.AssignedToRole,
            request.RequiredSopIds,
            request.RequiredTrainingIds);

        await _blueprintRepository.UpdateAsync(blueprint, cancellationToken).ConfigureAwait(false);
        await _blueprintRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return TaskBlueprintMapper.ToResponse(blueprint);
    }

    public async Task<TaskBlueprintResponse> ActivateBlueprintAsync(
        Guid siteId,
        Guid blueprintId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var blueprint = await EnsureBlueprintAsync(siteId, blueprintId, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Activating task blueprint {BlueprintId} for site {SiteId} by user {UserId}",
            blueprintId,
            siteId,
            userId);

        blueprint.Activate();
        await _blueprintRepository.UpdateAsync(blueprint, cancellationToken).ConfigureAwait(false);
        await _blueprintRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return TaskBlueprintMapper.ToResponse(blueprint);
    }

    public async Task<TaskBlueprintResponse> DeactivateBlueprintAsync(
        Guid siteId,
        Guid blueprintId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var blueprint = await EnsureBlueprintAsync(siteId, blueprintId, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Deactivating task blueprint {BlueprintId} for site {SiteId} by user {UserId}",
            blueprintId,
            siteId,
            userId);

        blueprint.Deactivate();
        await _blueprintRepository.UpdateAsync(blueprint, cancellationToken).ConfigureAwait(false);
        await _blueprintRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return TaskBlueprintMapper.ToResponse(blueprint);
    }

    public async Task DeleteBlueprintAsync(
        Guid siteId,
        Guid blueprintId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Deleting task blueprint {BlueprintId} for site {SiteId} by user {UserId}",
            blueprintId,
            siteId,
            userId);

        await _blueprintRepository.DeleteAsync(siteId, blueprintId, cancellationToken).ConfigureAwait(false);
        await _blueprintRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<TaskBlueprint> EnsureBlueprintAsync(
        Guid siteId,
        Guid blueprintId,
        CancellationToken cancellationToken)
    {
        var blueprint = await _blueprintRepository
            .GetByIdAsync(siteId, blueprintId, cancellationToken)
            .ConfigureAwait(false);

        if (blueprint is null)
        {
            throw new KeyNotFoundException($"Task blueprint {blueprintId} was not found for site {siteId}.");
        }

        return blueprint;
    }
}

