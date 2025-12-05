using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.DTOs;

namespace Harvestry.Tasks.Application.Interfaces;

public interface ITaskLibraryService
{
    Task<TaskLibraryItemResponse> CreateItemAsync(
        Guid orgId,
        CreateTaskLibraryItemRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<TaskLibraryItemResponse?> GetItemByIdAsync(
        Guid orgId,
        Guid itemId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaskLibraryItemResponse>> GetItemsByOrgAsync(
        Guid orgId,
        bool? activeOnly,
        CancellationToken cancellationToken = default);

    Task<TaskLibraryItemResponse> UpdateItemAsync(
        Guid orgId,
        Guid itemId,
        UpdateTaskLibraryItemRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<TaskLibraryItemResponse> ActivateItemAsync(
        Guid orgId,
        Guid itemId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<TaskLibraryItemResponse> DeactivateItemAsync(
        Guid orgId,
        Guid itemId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task DeleteItemAsync(
        Guid orgId,
        Guid itemId,
        Guid userId,
        CancellationToken cancellationToken = default);
}

