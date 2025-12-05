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

public sealed class TaskLibraryService : ITaskLibraryService
{
    private readonly ITaskLibraryRepository _libraryRepository;
    private readonly ILogger<TaskLibraryService> _logger;

    public TaskLibraryService(
        ITaskLibraryRepository libraryRepository,
        ILogger<TaskLibraryService> logger)
    {
        _libraryRepository = libraryRepository ?? throw new ArgumentNullException(nameof(libraryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TaskLibraryItemResponse> CreateItemAsync(
        Guid orgId,
        CreateTaskLibraryItemRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        _logger.LogInformation(
            "Creating task library item '{Title}' for org {OrgId} by user {UserId}",
            request.Title,
            orgId,
            userId);

        var item = TaskLibraryItem.Create(
            orgId,
            request.Title,
            request.Description,
            request.DefaultPriority,
            request.TaskType,
            request.CustomTaskType,
            request.DefaultAssignedToRole,
            request.DefaultDueDaysOffset,
            request.DefaultSopIds,
            userId);

        await _libraryRepository.AddAsync(item, cancellationToken).ConfigureAwait(false);
        await _libraryRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Task library item {ItemId} created successfully", item.Id);
        return TaskLibraryItemMapper.ToResponse(item);
    }

    public async Task<TaskLibraryItemResponse?> GetItemByIdAsync(
        Guid orgId,
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        var item = await _libraryRepository
            .GetByIdAsync(orgId, itemId, cancellationToken)
            .ConfigureAwait(false);

        return item is null ? null : TaskLibraryItemMapper.ToResponse(item);
    }

    public async Task<IReadOnlyList<TaskLibraryItemResponse>> GetItemsByOrgAsync(
        Guid orgId,
        bool? activeOnly,
        CancellationToken cancellationToken = default)
    {
        var items = await _libraryRepository
            .GetByOrgAsync(orgId, activeOnly, cancellationToken)
            .ConfigureAwait(false);

        return items.Select(TaskLibraryItemMapper.ToResponse).ToArray();
    }

    public async Task<TaskLibraryItemResponse> UpdateItemAsync(
        Guid orgId,
        Guid itemId,
        UpdateTaskLibraryItemRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var item = await EnsureItemAsync(orgId, itemId, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Updating task library item {ItemId} for org {OrgId} by user {UserId}",
            itemId,
            orgId,
            userId);

        item.Update(
            request.Title,
            request.Description,
            request.DefaultPriority,
            request.TaskType,
            request.CustomTaskType,
            request.DefaultAssignedToRole,
            request.DefaultDueDaysOffset,
            request.DefaultSopIds);

        await _libraryRepository.UpdateAsync(item, cancellationToken).ConfigureAwait(false);
        await _libraryRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return TaskLibraryItemMapper.ToResponse(item);
    }

    public async Task<TaskLibraryItemResponse> ActivateItemAsync(
        Guid orgId,
        Guid itemId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var item = await EnsureItemAsync(orgId, itemId, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Activating task library item {ItemId} for org {OrgId} by user {UserId}",
            itemId,
            orgId,
            userId);

        item.Activate();
        await _libraryRepository.UpdateAsync(item, cancellationToken).ConfigureAwait(false);
        await _libraryRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return TaskLibraryItemMapper.ToResponse(item);
    }

    public async Task<TaskLibraryItemResponse> DeactivateItemAsync(
        Guid orgId,
        Guid itemId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var item = await EnsureItemAsync(orgId, itemId, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Deactivating task library item {ItemId} for org {OrgId} by user {UserId}",
            itemId,
            orgId,
            userId);

        item.Deactivate();
        await _libraryRepository.UpdateAsync(item, cancellationToken).ConfigureAwait(false);
        await _libraryRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return TaskLibraryItemMapper.ToResponse(item);
    }

    public async Task DeleteItemAsync(
        Guid orgId,
        Guid itemId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Deleting task library item {ItemId} for org {OrgId} by user {UserId}",
            itemId,
            orgId,
            userId);

        await _libraryRepository.DeleteAsync(orgId, itemId, cancellationToken).ConfigureAwait(false);
        await _libraryRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<TaskLibraryItem> EnsureItemAsync(
        Guid orgId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var item = await _libraryRepository
            .GetByIdAsync(orgId, itemId, cancellationToken)
            .ConfigureAwait(false);

        if (item is null)
        {
            throw new KeyNotFoundException($"Task library item {itemId} was not found for organization {orgId}.");
        }

        return item;
    }
}

