using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Genetics.API.Controllers;

/// <summary>
/// API surface for managing mother plants and their health records.
/// </summary>
[ApiController]
[Route("api/v1/genetics/mother-plants")]
[Authorize]
public sealed class MotherPlantsController : ControllerBase
{
    private readonly IMotherHealthService _motherHealthService;

    public MotherPlantsController(IMotherHealthService motherHealthService)
    {
        _motherHealthService = motherHealthService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(MotherPlantResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<MotherPlantResponse>> CreateMotherPlant(
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        [FromBody] CreateMotherPlantRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _motherHealthService.CreateMotherPlantAsync(siteId, request, userId, cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetMotherPlantById), new { motherPlantId = response.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MotherPlantResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MotherPlantResponse>>> GetMotherPlants(
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromQuery] MotherPlantStatus? status,
        CancellationToken cancellationToken)
    {
        var results = await _motherHealthService.GetMotherPlantsAsync(siteId, status, cancellationToken).ConfigureAwait(false);
        return Ok(results);
    }

    [HttpGet("overdue")]
    [ProducesResponseType(typeof(IReadOnlyList<MotherPlantResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MotherPlantResponse>>> GetOverdueMotherPlants(
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        var results = await _motherHealthService.GetOverdueForHealthCheckAsync(siteId, cancellationToken).ConfigureAwait(false);
        return Ok(results);
    }

    [HttpGet("{motherPlantId}")]
    [ProducesResponseType(typeof(MotherPlantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MotherPlantResponse>> GetMotherPlantById(
        Guid motherPlantId,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        var mother = await _motherHealthService.GetMotherPlantByIdAsync(siteId, motherPlantId, cancellationToken).ConfigureAwait(false);
        if (mother is null)
        {
            return NotFound();
        }

        return Ok(mother);
    }

    [HttpPut("{motherPlantId}")]
    [ProducesResponseType(typeof(MotherPlantResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MotherPlantResponse>> UpdateMotherPlant(
        Guid motherPlantId,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        [FromBody] UpdateMotherPlantRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _motherHealthService.UpdateMotherPlantAsync(siteId, motherPlantId, request, userId, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("{motherPlantId}/health-logs")]
    [ProducesResponseType(typeof(MotherPlantResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MotherPlantResponse>> RecordHealthLog(
        Guid motherPlantId,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        [FromBody] MotherPlantHealthLogRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _motherHealthService.RecordHealthLogAsync(siteId, motherPlantId, request, userId, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("{motherPlantId}/health-logs")]
    [ProducesResponseType(typeof(IReadOnlyList<MotherHealthLogResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MotherHealthLogResponse>>> GetHealthLogs(
        Guid motherPlantId,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        var logs = await _motherHealthService.GetHealthLogsAsync(siteId, motherPlantId, cancellationToken).ConfigureAwait(false);
        return Ok(logs);
    }

    [HttpGet("{motherPlantId}/health-summary")]
    [ProducesResponseType(typeof(MotherPlantHealthSummaryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MotherPlantHealthSummaryResponse>> GetHealthSummary(
        Guid motherPlantId,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        var summary = await _motherHealthService.GetHealthSummaryAsync(siteId, motherPlantId, cancellationToken).ConfigureAwait(false);
        return Ok(summary);
    }

    [HttpPost("{motherPlantId}/propagation")]
    [ProducesResponseType(typeof(MotherPlantResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MotherPlantResponse>> RegisterPropagation(
        Guid motherPlantId,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        [FromBody] RegisterPropagationRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _motherHealthService.RegisterPropagationAsync(siteId, motherPlantId, request, userId, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }
}
