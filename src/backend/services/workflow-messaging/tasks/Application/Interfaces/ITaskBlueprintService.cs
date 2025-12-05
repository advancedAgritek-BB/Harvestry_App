using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.DTOs;

namespace Harvestry.Tasks.Application.Interfaces;

public interface ITaskBlueprintService
{
    Task<TaskBlueprintResponse> CreateBlueprintAsync(
        Guid siteId,
        CreateTaskBlueprintRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<TaskBlueprintResponse?> GetBlueprintByIdAsync(
        Guid siteId,
        Guid blueprintId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaskBlueprintResponse>> GetBlueprintsBySiteAsync(
        Guid siteId,
        bool? activeOnly,
        CancellationToken cancellationToken = default);

    Task<TaskBlueprintResponse> UpdateBlueprintAsync(
        Guid siteId,
        Guid blueprintId,
        UpdateTaskBlueprintRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<TaskBlueprintResponse> ActivateBlueprintAsync(
        Guid siteId,
        Guid blueprintId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<TaskBlueprintResponse> DeactivateBlueprintAsync(
        Guid siteId,
        Guid blueprintId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task DeleteBlueprintAsync(
        Guid siteId,
        Guid blueprintId,
        Guid userId,
        CancellationToken cancellationToken = default);
}

