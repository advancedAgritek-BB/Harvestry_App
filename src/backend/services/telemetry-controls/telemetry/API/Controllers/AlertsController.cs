using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Telemetry.Application.DTOs;
using Harvestry.Telemetry.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Telemetry.API.Controllers;

/// <summary>
/// Exposes alert instance query and acknowledgment endpoints.
/// </summary>
[ApiController]
[Route("api/v1/sites/{siteId:guid}/alerts")]
public class AlertsController : ControllerBase
{
    private readonly IAlertEvaluationService _alertEvaluationService;
    private readonly ITelemetryRlsContextAccessor _rlsContextAccessor;
    private readonly ILogger<AlertsController> _logger;

    public AlertsController(
        IAlertEvaluationService alertEvaluationService,
        ITelemetryRlsContextAccessor rlsContextAccessor,
        ILogger<AlertsController> logger)
    {
        _alertEvaluationService = alertEvaluationService ?? throw new ArgumentNullException(nameof(alertEvaluationService));
        _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all active alerts for the specified site.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<AlertInstanceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveAlerts(Guid siteId, CancellationToken cancellationToken)
    {
        var alerts = await _alertEvaluationService
            .GetActiveAlertsAsync(siteId, cancellationToken)
            .ConfigureAwait(false);

        return Ok(alerts);
    }

    /// <summary>
    /// Acknowledges an alert instance.
    /// </summary>
    [HttpPost("{alertId:guid}/acknowledge")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AcknowledgeAlert(
        Guid siteId,
        Guid alertId,
        [FromBody] AcknowledgeAlertRequestDto request,
        CancellationToken cancellationToken)
    {
        var context = _rlsContextAccessor.Current;
        var userId = context.UserId;
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("User ID is required for alert acknowledgment");
            return BadRequest("User ID is required for alert acknowledgment");
        }

        var success = await _alertEvaluationService
            .AcknowledgeAlertAsync(siteId, alertId, userId, request?.Notes, cancellationToken)
            .ConfigureAwait(false);

        if (!success)
        {
            _logger.LogWarning("Unable to acknowledge alert {AlertId} for site {SiteId}", alertId, siteId);
            return NotFound();
        }

        return NoContent();
    }
}
