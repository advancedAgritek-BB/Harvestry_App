using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Telemetry.Application.DTOs;
using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Telemetry.API.Controllers;

/// <summary>
/// Query endpoints for telemetry readings and rollups.
/// </summary>
[ApiController]
[Route("api/v1/sites/{siteId:guid}/streams/{streamId:guid}/query")]
public class TelemetryQueryController : ControllerBase
{
    private readonly ITelemetryQueryRepository _queryRepository;
    private readonly ILogger<TelemetryQueryController> _logger;

    public TelemetryQueryController(
        ITelemetryQueryRepository queryRepository,
        ILogger<TelemetryQueryController> logger)
    {
        _queryRepository = queryRepository ?? throw new ArgumentNullException(nameof(queryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Query raw telemetry readings for a stream within a time range.
    /// </summary>
    [HttpGet("readings")]
    [ProducesResponseType(typeof(IEnumerable<TelemetryReadingResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetReadings(
        Guid siteId,
        Guid streamId,
        [FromQuery] DateTimeOffset start,
        [FromQuery] DateTimeOffset end,
        [FromQuery] int? limit,
        CancellationToken cancellationToken)
    {
        if (end <= start)
        {
            return BadRequest("End must be greater than start");
        }

        var readings = await _queryRepository
            .GetReadingsAsync(streamId, start, end, limit, cancellationToken)
            .ConfigureAwait(false);

        var response = readings
            .Select(r => new TelemetryReadingResponseDto(r.Time, r.Value, r.QualityCode))
            .ToList();

        return Ok(response);
    }

    /// <summary>
    /// Get the latest telemetry reading for a stream.
    /// </summary>
    [HttpGet("latest")]
    [ProducesResponseType(typeof(LatestReadingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLatest(Guid siteId, Guid streamId, CancellationToken cancellationToken)
    {
        var reading = await _queryRepository
            .GetLatestReadingAsync(streamId, cancellationToken)
            .ConfigureAwait(false);

        if (reading == null)
        {
            return NotFound();
        }

        var age = DateTimeOffset.UtcNow - reading.Time;
        var response = new LatestReadingDto(reading.StreamId, reading.Time, reading.Value, reading.QualityCode, age);
        return Ok(response);
    }

    /// <summary>
    /// Query aggregated rollups for a stream.
    /// </summary>
    [HttpGet("rollups")]
    [ProducesResponseType(typeof(IEnumerable<TelemetryRollupResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRollups(
        Guid siteId,
        Guid streamId,
        [FromQuery] DateTimeOffset start,
        [FromQuery] DateTimeOffset end,
        [FromQuery] RollupInterval interval,
        CancellationToken cancellationToken)
    {
        if (interval == RollupInterval.Raw)
        {
            return BadRequest("Use the readings endpoint for raw data");
        }

        if (end <= start)
        {
            return BadRequest("End must be greater than start");
        }

        try
        {
            var rollups = await _queryRepository
                .GetRollupsAsync(streamId, start, end, interval, cancellationToken)
                .ConfigureAwait(false);

            var response = rollups
                .Select(r => new TelemetryRollupResponseDto(
                    r.Bucket,
                    r.SampleCount,
                    r.AvgValue,
                    r.MinValue,
                    r.MaxValue,
                    r.MedianValue,
                    r.StdDevValue))
                .ToList();

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid rollup interval {Interval} requested for stream {StreamId}", interval, streamId);
            return BadRequest(ex.Message);
        }
    }
}
