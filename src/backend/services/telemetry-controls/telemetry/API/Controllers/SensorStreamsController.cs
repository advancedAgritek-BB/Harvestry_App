using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Harvestry.Telemetry.Application.DTOs;
using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Telemetry.API.Controllers;

/// <summary>
/// Manages sensor stream configuration for a site.
/// </summary>
[ApiController]
[Route("api/v1/sites/{siteId:guid}/streams")]
public class SensorStreamsController : ControllerBase
{
    private readonly ISensorStreamRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<SensorStreamsController> _logger;

    public SensorStreamsController(
        ISensorStreamRepository repository,
        IMapper mapper,
        ILogger<SensorStreamsController> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all sensor streams for the site.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SensorStreamDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStreams(Guid siteId, CancellationToken cancellationToken)
    {
        var streams = await _repository.GetBySiteIdAsync(siteId, cancellationToken).ConfigureAwait(false);
        var response = _mapper.Map<List<SensorStreamDto>>(streams);
        return Ok(response);
    }

    /// <summary>
    /// Get a specific sensor stream.
    /// </summary>
    [HttpGet("{streamId:guid}")]
    [ProducesResponseType(typeof(SensorStreamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStream(Guid siteId, Guid streamId, CancellationToken cancellationToken)
    {
        var stream = await _repository.GetByIdAsync(streamId, cancellationToken).ConfigureAwait(false);
        if (stream == null || stream.SiteId != siteId)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<SensorStreamDto>(stream));
    }

    /// <summary>
    /// Create a new sensor stream.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SensorStreamDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateStream(
        Guid siteId,
        [FromBody] CreateSensorStreamRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request == null)
        {
            return BadRequest("Request body cannot be null");
        }

        try
        {
            var stream = SensorStream.Create(
                siteId,
                request.EquipmentId,
                request.StreamType,
                request.Unit,
                request.DisplayName,
                request.EquipmentChannelId,
                request.LocationId,
                request.RoomId,
                request.ZoneId);

            await _repository.CreateAsync(stream, cancellationToken).ConfigureAwait(false);

            var dto = _mapper.Map<SensorStreamDto>(stream);
            return CreatedAtAction(nameof(GetStream), new { siteId, streamId = stream.Id }, dto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid sensor stream create request for site {SiteId}", siteId);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update an existing sensor stream.
    /// </summary>
    [HttpPatch("{streamId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStream(
        Guid siteId,
        Guid streamId,
        [FromBody] UpdateSensorStreamRequestDto request,
        CancellationToken cancellationToken)
    {
        var stream = await _repository.GetByIdAsync(streamId, cancellationToken).ConfigureAwait(false);
        if (stream == null || stream.SiteId != siteId)
        {
            return NotFound();
        }

        if (request.DisplayName is { Length: > 0 })
        {
            stream.UpdateDisplayName(request.DisplayName);
        }

        if (request.LocationId.HasValue || request.RoomId.HasValue || request.ZoneId.HasValue)
        {
            stream.UpdateLocation(
                request.LocationId ?? stream.LocationId,
                request.RoomId ?? stream.RoomId,
                request.ZoneId ?? stream.ZoneId);
        }

        await _repository.UpdateAsync(stream, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Deactivate a sensor stream.
    /// </summary>
    [HttpPost("{streamId:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateStream(Guid siteId, Guid streamId, CancellationToken cancellationToken)
    {
        var stream = await _repository.GetByIdAsync(streamId, cancellationToken).ConfigureAwait(false);
        if (stream == null || stream.SiteId != siteId)
        {
            return NotFound();
        }

        if (stream.IsActive)
        {
            stream.Deactivate();
            await _repository.UpdateAsync(stream, cancellationToken).ConfigureAwait(false);
        }

        return NoContent();
    }

    /// <summary>
    /// Activate a sensor stream.
    /// </summary>
    [HttpPost("{streamId:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateStream(Guid siteId, Guid streamId, CancellationToken cancellationToken)
    {
        var stream = await _repository.GetByIdAsync(streamId, cancellationToken).ConfigureAwait(false);
        if (stream == null || stream.SiteId != siteId)
        {
            return NotFound();
        }

        if (!stream.IsActive)
        {
            stream.Activate();
            await _repository.UpdateAsync(stream, cancellationToken).ConfigureAwait(false);
        }

        return NoContent();
    }
}
