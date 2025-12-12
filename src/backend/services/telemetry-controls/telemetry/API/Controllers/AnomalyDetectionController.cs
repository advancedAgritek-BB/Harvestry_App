using Harvestry.Telemetry.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Telemetry.API.Controllers;

/// <summary>
/// API controller for anomaly detection and recommendations
/// </summary>
[ApiController]
[Route("api/v1/telemetry/anomalies")]
[Authorize]
public sealed class AnomalyDetectionController : ControllerBase
{
    private readonly IAnomalyDetectionService _anomalyService;
    private readonly ILogger<AnomalyDetectionController> _logger;

    public AnomalyDetectionController(
        IAnomalyDetectionService anomalyService,
        ILogger<AnomalyDetectionController> logger)
    {
        _anomalyService = anomalyService ?? throw new ArgumentNullException(nameof(anomalyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Analyzes a specific sensor stream for anomalies
    /// </summary>
    [HttpGet("stream/{streamId:guid}")]
    [ProducesResponseType(typeof(AnomalyAnalysisResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AnomalyAnalysisResult>> AnalyzeStream(
        Guid streamId,
        [FromQuery] int? windowHours = null,
        CancellationToken cancellationToken = default)
    {
        var window = windowHours.HasValue 
            ? TimeSpan.FromHours(windowHours.Value) 
            : (TimeSpan?)null;

        var result = await _anomalyService.AnalyzeStreamAsync(streamId, window, cancellationToken);

        _logger.LogDebug(
            "Anomaly analysis for stream {StreamId}: {Count} anomalies detected",
            streamId, result.AnomaliesDetected);

        return Ok(result);
    }

    /// <summary>
    /// Generates a site-wide anomaly report with recommendations
    /// </summary>
    [HttpGet("site/{siteId:guid}/report")]
    [ProducesResponseType(typeof(SiteAnomalyReport), StatusCodes.Status200OK)]
    public async Task<ActionResult<SiteAnomalyReport>> GetSiteReport(
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating anomaly report for site {SiteId}", siteId);

        var report = await _anomalyService.AnalyzeSiteAsync(siteId, cancellationToken);

        _logger.LogInformation(
            "Anomaly report for site {SiteId}: {StreamsWithAnomalies}/{StreamsAnalyzed} streams with anomalies, {TotalAnomalies} total",
            siteId, report.StreamsWithAnomalies, report.StreamsAnalyzed, report.TotalAnomalies);

        return Ok(report);
    }

    /// <summary>
    /// Gets top recommendations for a site based on detected anomalies
    /// </summary>
    [HttpGet("site/{siteId:guid}/recommendations")]
    [ProducesResponseType(typeof(IReadOnlyList<AnomalyRecommendation>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AnomalyRecommendation>>> GetRecommendations(
        Guid siteId,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var report = await _anomalyService.AnalyzeSiteAsync(siteId, cancellationToken);

        var recommendations = report.TopRecommendations
            .Take(limit)
            .ToList();

        return Ok(recommendations);
    }
}
