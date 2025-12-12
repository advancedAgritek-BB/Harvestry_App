using Harvestry.AiModels.Application.Interfaces;
using Harvestry.AiModels.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.AiModels.API.Controllers;

/// <summary>
/// API controller for graph-based anomaly detection.
/// Exposes anomaly scores and explanations for movements, irrigation, and other domains.
/// </summary>
[ApiController]
[Route("api/v1/ai/anomalies")]
[Authorize]
public sealed class AnomalyDetectionController : ControllerBase
{
    private readonly IAnomalyDetectionService _anomalyService;
    private readonly IMovementAnomalyDetector _movementDetector;
    private readonly IIrrigationAnomalyDetector _irrigationDetector;
    private readonly ILogger<AnomalyDetectionController> _logger;

    public AnomalyDetectionController(
        IAnomalyDetectionService anomalyService,
        IMovementAnomalyDetector movementDetector,
        IIrrigationAnomalyDetector irrigationDetector,
        ILogger<AnomalyDetectionController> logger)
    {
        _anomalyService = anomalyService;
        _movementDetector = movementDetector;
        _irrigationDetector = irrigationDetector;
        _logger = logger;
    }

    /// <summary>
    /// Get top anomalies for a site
    /// </summary>
    [HttpGet("site/{siteId}")]
    [ProducesResponseType(typeof(AnomalyListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopAnomalies(
        [FromRoute] Guid siteId,
        [FromQuery] int limit = 50,
        [FromQuery] string? nodeType = null,
        CancellationToken cancellationToken = default)
    {
        GraphNodeType? parsedNodeType = null;
        if (!string.IsNullOrEmpty(nodeType) && 
            Enum.TryParse<GraphNodeType>(nodeType, true, out var parsed))
        {
            parsedNodeType = parsed;
        }

        var anomalies = await _anomalyService.GetTopAnomaliesAsync(
            siteId, limit, parsedNodeType, cancellationToken);

        return Ok(new AnomalyListResponse
        {
            SiteId = siteId,
            Anomalies = anomalies.Select(MapToDto).ToList(),
            TotalCount = anomalies.Count
        });
    }

    /// <summary>
    /// Score anomaly for a specific node
    /// </summary>
    [HttpGet("node/{nodeId}")]
    [ProducesResponseType(typeof(AnomalyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScoreNode(
        [FromRoute] string nodeId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _anomalyService.ScoreNodeAnomalyAsync(nodeId, cancellationToken);
            return Ok(MapToDto(result));
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = "Node not found" });
        }
    }

    /// <summary>
    /// Detect movement anomalies for a site
    /// </summary>
    [HttpPost("site/{siteId}/movements/detect")]
    [ProducesResponseType(typeof(AnomalyBatchResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DetectMovementAnomalies(
        [FromRoute] Guid siteId,
        [FromQuery] DateTime? since = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Detecting movement anomalies for site {SiteId}", siteId);

        var anomalies = await _movementDetector.DetectMovementAnomaliesAsync(
            siteId, since, cancellationToken);

        return Ok(new AnomalyBatchResponse
        {
            SiteId = siteId,
            AnomalyType = "movement",
            DetectedCount = anomalies.Count,
            Anomalies = anomalies.Select(MapToDto).ToList()
        });
    }

    /// <summary>
    /// Score a specific movement for anomaly
    /// </summary>
    [HttpGet("movements/{movementId}/score")]
    [ProducesResponseType(typeof(AnomalyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScoreMovement(
        [FromRoute] Guid movementId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _movementDetector.ScoreMovementAsync(movementId, cancellationToken);
            return Ok(MapToDto(result));
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = "Movement not found in graph" });
        }
    }

    /// <summary>
    /// Detect irrigation response anomalies for a site
    /// </summary>
    [HttpPost("site/{siteId}/irrigation/detect")]
    [ProducesResponseType(typeof(AnomalyBatchResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DetectIrrigationAnomalies(
        [FromRoute] Guid siteId,
        [FromQuery] DateTime? since = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Detecting irrigation anomalies for site {SiteId}", siteId);

        var anomalies = await _irrigationDetector.DetectIrrigationAnomaliesAsync(
            siteId, since, cancellationToken);

        return Ok(new AnomalyBatchResponse
        {
            SiteId = siteId,
            AnomalyType = "irrigation_response",
            DetectedCount = anomalies.Count,
            Anomalies = anomalies.Select(MapToDto).ToList()
        });
    }

    /// <summary>
    /// Score a specific irrigation run for response anomaly
    /// </summary>
    [HttpGet("irrigation/{runId}/score")]
    [ProducesResponseType(typeof(AnomalyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ScoreIrrigationRun(
        [FromRoute] Guid runId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _irrigationDetector.ScoreIrrigationRunAsync(runId, cancellationToken);
            return Ok(MapToDto(result));
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = "Irrigation run not found in graph" });
        }
    }

    /// <summary>
    /// Acknowledge an anomaly (mark as reviewed)
    /// </summary>
    [HttpPost("anomalies/{anomalyId}/acknowledge")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AcknowledgeAnomaly(
        [FromRoute] Guid anomalyId,
        [FromBody] AcknowledgeAnomalyRequest request,
        CancellationToken cancellationToken = default)
    {
        // Get user ID from claims
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            await _anomalyService.AcknowledgeAnomalyAsync(
                anomalyId, userId, request.ResolutionNotes, cancellationToken);

            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = "Anomaly not found" });
        }
    }

    private static AnomalyDto MapToDto(AnomalyResult result)
    {
        return new AnomalyDto
        {
            Id = result.Id,
            NodeId = result.NodeId,
            EdgeId = result.EdgeId,
            NodeType = result.NodeType.ToString(),
            AnomalyType = result.AnomalyType,
            Score = result.Score,
            Severity = GetSeverity(result.Score),
            Explanation = result.Explanation,
            FeatureAttributions = result.FeatureAttributions
                .Select(kv => new FeatureAttributionDto
                {
                    Feature = kv.Key,
                    Contribution = kv.Value
                })
                .OrderByDescending(f => f.Contribution)
                .ToList(),
            DetectedAt = result.DetectedAt,
            ModelVersion = result.ModelVersion,
            IsAcknowledged = result.IsAcknowledged
        };
    }

    private static string GetSeverity(float score)
    {
        return score switch
        {
            >= 0.9f => "Critical",
            >= 0.7f => "High",
            >= 0.5f => "Medium",
            _ => "Low"
        };
    }
}

#region DTOs

public sealed class AnomalyListResponse
{
    public Guid SiteId { get; init; }
    public IReadOnlyList<AnomalyDto> Anomalies { get; init; } = Array.Empty<AnomalyDto>();
    public int TotalCount { get; init; }
}

public sealed class AnomalyBatchResponse
{
    public Guid SiteId { get; init; }
    public string AnomalyType { get; init; } = string.Empty;
    public int DetectedCount { get; init; }
    public IReadOnlyList<AnomalyDto> Anomalies { get; init; } = Array.Empty<AnomalyDto>();
}

public sealed class AnomalyDto
{
    public Guid? Id { get; init; }
    public string NodeId { get; init; } = string.Empty;
    public string? EdgeId { get; init; }
    public string NodeType { get; init; } = string.Empty;
    public string AnomalyType { get; init; } = string.Empty;
    public float Score { get; init; }
    public string Severity { get; init; } = string.Empty;
    public string Explanation { get; init; } = string.Empty;
    public IReadOnlyList<FeatureAttributionDto> FeatureAttributions { get; init; } 
        = Array.Empty<FeatureAttributionDto>();
    public DateTime DetectedAt { get; init; }
    public string ModelVersion { get; init; } = string.Empty;
    public bool IsAcknowledged { get; init; }
}

public sealed class FeatureAttributionDto
{
    public string Feature { get; init; } = string.Empty;
    public float Contribution { get; init; }
}

public sealed class AcknowledgeAnomalyRequest
{
    public string? ResolutionNotes { get; init; }
}

#endregion
