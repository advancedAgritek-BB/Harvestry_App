using System;
using System.Linq;

using Harvestry.Telemetry.API.Models;
using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Application.Interfaces.Models;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Telemetry.API.Controllers;

/// <summary>
/// Provides real-time telemetry infrastructure diagnostics.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public sealed class RealtimeController : ControllerBase
{
    private readonly ITelemetrySubscriptionRegistry _subscriptionRegistry;
    private readonly ILogger<RealtimeController> _logger;

    public RealtimeController(
        ITelemetrySubscriptionRegistry subscriptionRegistry,
        ILogger<RealtimeController> logger)
    {
        _subscriptionRegistry = subscriptionRegistry ?? throw new ArgumentNullException(nameof(subscriptionRegistry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Returns the current SignalR subscription snapshot.
    /// </summary>
    /// <param name="top">Number of busiest streams to include (1-100).</param>
    [HttpGet("subscriptions")]
    [ProducesResponseType(typeof(TelemetrySubscriptionSnapshotDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<TelemetrySubscriptionSnapshotDto> GetSubscriptionSnapshot([FromQuery] int top = 10)
    {
        top = Math.Clamp(top, 1, 100);

        TelemetrySubscriptionSnapshot snapshot;
        try
        {
            snapshot = _subscriptionRegistry.GetSnapshot();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve subscription snapshot");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        var streams = snapshot.StreamCounts
            .OrderByDescending(kvp => kvp.Value)
            .ThenBy(kvp => kvp.Key)
            .Take(top)
            .Select(kvp => new StreamSubscriptionSummaryDto(kvp.Key, kvp.Value))
            .ToList();

        _logger.LogDebug(
            "Realtime snapshot requested (connections={Connections}, activeStreams={Streams}, totalSubscriptions={Subscriptions})",
            snapshot.TotalConnections,
            snapshot.ActiveStreamCount,
            snapshot.TotalSubscriptions);

        return Ok(new TelemetrySubscriptionSnapshotDto(
            snapshot.CapturedAt,
            snapshot.TotalConnections,
            snapshot.TotalSubscriptions,
            snapshot.ActiveStreamCount,
            streams));
    }
}
