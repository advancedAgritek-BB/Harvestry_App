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

public sealed class SopService : ISopService
{
    private readonly ISopRepository _sopRepository;
    private readonly ILogger<SopService> _logger;

    public SopService(ISopRepository sopRepository, ILogger<SopService> logger)
    {
        _sopRepository = sopRepository ?? throw new ArgumentNullException(nameof(sopRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SopResponse> CreateSopAsync(
        Guid orgId,
        CreateSopRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        _logger.LogInformation(
            "Creating SOP '{Title}' for org {OrgId} by user {UserId}",
            request.Title,
            orgId,
            userId);

        var sop = StandardOperatingProcedure.Create(
            orgId,
            request.Title,
            request.Content,
            request.Category,
            userId);

        await _sopRepository.AddAsync(sop, cancellationToken).ConfigureAwait(false);
        await _sopRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("SOP {SopId} created successfully", sop.Id);
        return SopMapper.ToResponse(sop);
    }

    public async Task<SopResponse?> GetSopByIdAsync(
        Guid orgId,
        Guid sopId,
        CancellationToken cancellationToken = default)
    {
        var sop = await _sopRepository
            .GetByIdAsync(orgId, sopId, cancellationToken)
            .ConfigureAwait(false);

        return sop is null ? null : SopMapper.ToResponse(sop);
    }

    public async Task<IReadOnlyList<SopSummaryResponse>> GetSopsByOrgAsync(
        Guid orgId,
        bool? activeOnly,
        string? category,
        CancellationToken cancellationToken = default)
    {
        var sops = await _sopRepository
            .GetByOrgAsync(orgId, activeOnly, category, cancellationToken)
            .ConfigureAwait(false);

        return sops.Select(SopMapper.ToSummary).ToArray();
    }

    public async Task<SopResponse> UpdateSopAsync(
        Guid orgId,
        Guid sopId,
        UpdateSopRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var sop = await EnsureSopAsync(orgId, sopId, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Updating SOP {SopId} for org {OrgId} by user {UserId}",
            sopId,
            orgId,
            userId);

        sop.Update(request.Title, request.Content, request.Category);

        await _sopRepository.UpdateAsync(sop, cancellationToken).ConfigureAwait(false);
        await _sopRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return SopMapper.ToResponse(sop);
    }

    public async Task<SopResponse> ActivateSopAsync(
        Guid orgId,
        Guid sopId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var sop = await EnsureSopAsync(orgId, sopId, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Activating SOP {SopId} for org {OrgId} by user {UserId}",
            sopId,
            orgId,
            userId);

        sop.Activate();
        await _sopRepository.UpdateAsync(sop, cancellationToken).ConfigureAwait(false);
        await _sopRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return SopMapper.ToResponse(sop);
    }

    public async Task<SopResponse> DeactivateSopAsync(
        Guid orgId,
        Guid sopId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var sop = await EnsureSopAsync(orgId, sopId, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Deactivating SOP {SopId} for org {OrgId} by user {UserId}",
            sopId,
            orgId,
            userId);

        sop.Deactivate();
        await _sopRepository.UpdateAsync(sop, cancellationToken).ConfigureAwait(false);
        await _sopRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return SopMapper.ToResponse(sop);
    }

    public async Task DeleteSopAsync(
        Guid orgId,
        Guid sopId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Deleting SOP {SopId} for org {OrgId} by user {UserId}",
            sopId,
            orgId,
            userId);

        await _sopRepository.DeleteAsync(orgId, sopId, cancellationToken).ConfigureAwait(false);
        await _sopRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<StandardOperatingProcedure> EnsureSopAsync(
        Guid orgId,
        Guid sopId,
        CancellationToken cancellationToken)
    {
        var sop = await _sopRepository
            .GetByIdAsync(orgId, sopId, cancellationToken)
            .ConfigureAwait(false);

        if (sop is null)
        {
            throw new KeyNotFoundException($"SOP {sopId} was not found for organization {orgId}.");
        }

        return sop;
    }
}

