using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.DTOs;

namespace Harvestry.Tasks.Application.Interfaces;

public interface ISopService
{
    Task<SopResponse> CreateSopAsync(
        Guid orgId,
        CreateSopRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<SopResponse?> GetSopByIdAsync(
        Guid orgId,
        Guid sopId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SopSummaryResponse>> GetSopsByOrgAsync(
        Guid orgId,
        bool? activeOnly,
        string? category,
        CancellationToken cancellationToken = default);

    Task<SopResponse> UpdateSopAsync(
        Guid orgId,
        Guid sopId,
        UpdateSopRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<SopResponse> ActivateSopAsync(
        Guid orgId,
        Guid sopId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<SopResponse> DeactivateSopAsync(
        Guid orgId,
        Guid sopId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task DeleteSopAsync(
        Guid orgId,
        Guid sopId,
        Guid userId,
        CancellationToken cancellationToken = default);
}

