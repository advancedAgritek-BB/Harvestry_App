using Harvestry.Telemetry.Application.DeviceAdapters;
using Harvestry.Telemetry.Application.DTOs;
using Harvestry.Telemetry.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Harvestry.Telemetry.API.Controllers;

/// <summary>
/// API controller for telemetry ingestion and query operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class TelemetryController : ControllerBase
{
    private readonly ITelemetryIngestService _ingestService;
    private readonly IHttpIngestAdapter _httpIngestAdapter;
    private readonly ILogger<TelemetryController> _logger;
    
    public TelemetryController(
        ITelemetryIngestService ingestService,
        IHttpIngestAdapter httpIngestAdapter,
        ILogger<TelemetryController> logger)
    {
        _ingestService = ingestService ?? throw new ArgumentNullException(nameof(ingestService));
        _httpIngestAdapter = httpIngestAdapter ?? throw new ArgumentNullException(nameof(httpIngestAdapter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// Ingest a batch of telemetry readings.
    /// </summary>
    /// <param name="request">The telemetry ingestion request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Ingestion result with acceptance/rejection counts</returns>
    [HttpPost("ingest")]
    [EnableRateLimiting("ingest")]
    [ProducesResponseType(typeof(IngestResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> IngestBatch(
        [FromBody] IngestTelemetryRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (request == null)
            {
                return BadRequest("Request body cannot be null");
            }
            
            if (request.Readings == null || request.Readings.Count == 0)
            {
                return BadRequest("Readings list cannot be empty");
            }
            
            var result = await _ingestService.IngestBatchAsync(request.SiteId, request, cancellationToken);
            
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for telemetry ingestion");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing telemetry ingestion");
            return StatusCode(500, "An error occurred processing your request");
        }
    }

    /// <summary>
    /// Ingest telemetry readings via HTTP adapter.
    /// </summary>
    /// <param name="equipmentId">Equipment identifier</param>
    /// <param name="request">HTTP ingest payload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpPost("ingest/http/{equipmentId:guid}")]
    [EnableRateLimiting("burst")]
    [ProducesResponseType(typeof(IngestResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> IngestViaHttp(
        Guid equipmentId,
        [FromBody] HttpIngestRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (request == null)
            {
                return BadRequest("Request body cannot be null");
            }

            var result = await _httpIngestAdapter.HandleAsync(equipmentId, request, cancellationToken).ConfigureAwait(false);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid HTTP ingest request for equipment {EquipmentId}", equipmentId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing HTTP ingest for equipment {EquipmentId}", equipmentId);
            return StatusCode(500, "An error occurred processing your request");
        }
    }
    
    /// <summary>
    /// Health check endpoint.
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow });
    }
}
