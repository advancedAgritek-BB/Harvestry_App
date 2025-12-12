using Harvestry.Compliance.Metrc.Application.DTOs;
using Harvestry.Compliance.Metrc.Application.Interfaces;
using Harvestry.Compliance.Metrc.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Harvestry.Compliance.Metrc.API.Controllers;

/// <summary>
/// API controller for METRC synchronization operations
/// </summary>
[ApiController]
[Route("api/v1/metrc/sync")]
[Authorize]
public sealed class MetrcSyncController : ControllerBase
{
    private readonly IMetrcSyncService _syncService;
    private readonly ILogger<MetrcSyncController> _logger;

    public MetrcSyncController(
        IMetrcSyncService syncService,
        ILogger<MetrcSyncController> logger)
    {
        _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Starts a new sync job
    /// </summary>
    [HttpPost("start")]
    [ProducesResponseType(typeof(StartSyncResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StartSyncResponse>> StartSync(
        [FromBody] StartSyncRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        _logger.LogInformation(
            "Starting METRC sync for license {LicenseNumber} (direction: {Direction}, user: {UserId})",
            request.LicenseNumber, request.Direction, userId);

        var response = await _syncService.StartSyncAsync(request, userId, cancellationToken);

        if (response.Status == SyncStatus.Failed)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Gets sync jobs for a site
    /// </summary>
    [HttpGet("jobs/site/{siteId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<SyncJobDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SyncJobDto>>> GetSyncJobs(
        Guid siteId,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var jobs = await _syncService.GetSyncJobsAsync(siteId, limit, cancellationToken);
        return Ok(jobs);
    }

    /// <summary>
    /// Gets a sync job by ID
    /// </summary>
    [HttpGet("jobs/{jobId:guid}")]
    [ProducesResponseType(typeof(SyncJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SyncJobDto>> GetSyncJob(
        Guid jobId,
        CancellationToken cancellationToken)
    {
        var job = await _syncService.GetSyncJobAsync(jobId, cancellationToken);
        if (job == null)
        {
            return NotFound();
        }
        return Ok(job);
    }

    /// <summary>
    /// Cancels a running sync job
    /// </summary>
    [HttpPost("jobs/{jobId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CancelSyncJob(
        Guid jobId,
        [FromBody] CancelSyncRequest? request,
        CancellationToken cancellationToken)
    {
        var success = await _syncService.CancelSyncJobAsync(
            jobId, request?.Reason, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        _logger.LogInformation("Cancelled sync job {JobId}: {Reason}",
            jobId, request?.Reason ?? "No reason provided");

        return Ok(new { message = "Sync job cancelled" });
    }

    /// <summary>
    /// Gets queue items for a sync job
    /// </summary>
    [HttpGet("jobs/{jobId:guid}/items")]
    [ProducesResponseType(typeof(IReadOnlyList<QueueItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<QueueItemDto>>> GetQueueItems(
        Guid jobId,
        [FromQuery] SyncStatus? status = null,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var items = await _syncService.GetQueueItemsAsync(jobId, status, limit, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Retries failed items for a sync job
    /// </summary>
    [HttpPost("jobs/{jobId:guid}/retry-failed")]
    [ProducesResponseType(typeof(RetryResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<RetryResult>> RetryFailedItems(
        Guid jobId,
        CancellationToken cancellationToken)
    {
        var retriedCount = await _syncService.RetryFailedItemsAsync(jobId, cancellationToken);

        _logger.LogInformation("Retried {Count} failed items for sync job {JobId}",
            retriedCount, jobId);

        return Ok(new RetryResult
        {
            RetriedCount = retriedCount,
            Message = retriedCount > 0
                ? $"Scheduled {retriedCount} items for retry"
                : "No items to retry"
        });
    }

    /// <summary>
    /// Gets sync status summary for a license
    /// </summary>
    [HttpGet("status/{licenseId:guid}")]
    [ProducesResponseType(typeof(SyncStatusSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SyncStatusSummaryDto>> GetSyncStatus(
        Guid licenseId,
        CancellationToken cancellationToken)
    {
        var status = await _syncService.GetSyncStatusAsync(licenseId, cancellationToken);
        if (status == null)
        {
            return NotFound();
        }
        return Ok(status);
    }

    /// <summary>
    /// Performs reconciliation between Harvestry and METRC
    /// </summary>
    [HttpPost("reconcile")]
    [ProducesResponseType(typeof(ReconciliationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReconciliationResponseDto>> Reconcile(
        [FromBody] ReconciliationRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting reconciliation for license {LicenseId} ({EntityTypes})",
            request.LicenseId,
            request.EntityTypes.Count > 0 ? string.Join(", ", request.EntityTypes) : "all");

        var result = await _syncService.ReconcileAsync(request, cancellationToken);
        return Ok(result);
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst("sub") ?? User.FindFirst("user_id");
        return claim != null && Guid.TryParse(claim.Value, out var userId) ? userId : null;
    }
}

/// <summary>
/// Request to cancel a sync job
/// </summary>
public sealed record CancelSyncRequest
{
    public string? Reason { get; init; }
}

/// <summary>
/// Result of a retry operation
/// </summary>
public sealed record RetryResult
{
    public int RetriedCount { get; init; }
    public string Message { get; init; } = string.Empty;
}
