using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Spatial.Application.DTOs;
using Harvestry.Spatial.Application.Exceptions;
using Harvestry.Spatial.Application.Interfaces;
using Harvestry.Spatial.Application.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Spatial.API.Controllers;

[ApiController]
[Route("api/v1/sites/{siteId:guid}/equipment/{equipmentId:guid}/calibrations")]
public sealed class CalibrationController : ControllerBase
{
    private readonly ICalibrationService _calibrationService;

    public CalibrationController(ICalibrationService calibrationService)
    {
        _calibrationService = calibrationService ?? throw new ArgumentNullException(nameof(calibrationService));
    }

    [HttpPost]
    public async Task<ActionResult<CalibrationResponse>> RecordCalibration(
        Guid siteId,
        Guid equipmentId,
        [FromBody] CreateCalibrationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest(CreateProblem("Missing user identifier", "Provide an X-User-Id header with a valid GUID."));
        }

        try
        {
            var command = request with
            {
                SiteId = siteId,
                EquipmentId = equipmentId,
                PerformedByUserId = userId
            };

            var response = await _calibrationService.RecordAsync(siteId, equipmentId, command, cancellationToken).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetLatestCalibration), new { siteId, equipmentId }, response);
        }
        catch (TenantMismatchException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, CreateProblem(
                "Site mismatch",
                $"Equipment belongs to site {ex.ActualSiteId} but request targeted site {ex.ExpectedSiteId}.",
                StatusCodes.Status403Forbidden));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CalibrationResponse>>> GetCalibrationHistory(
        Guid siteId,
        Guid equipmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var responses = await _calibrationService.GetHistoryAsync(siteId, equipmentId, cancellationToken).ConfigureAwait(false);
            return Ok(responses);
        }
        catch (TenantMismatchException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, CreateProblem(
                "Site mismatch",
                $"Equipment belongs to site {ex.ActualSiteId} but request targeted site {ex.ExpectedSiteId}.",
                StatusCodes.Status403Forbidden));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("latest")]
    public async Task<ActionResult<CalibrationResponse>> GetLatestCalibration(
        Guid siteId,
        Guid equipmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var calibration = await _calibrationService.GetLatestAsync(siteId, equipmentId, cancellationToken).ConfigureAwait(false);
            if (calibration is null)
            {
                return NotFound();
            }

            return Ok(calibration);
        }
        catch (TenantMismatchException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, CreateProblem(
                "Site mismatch",
                $"Equipment belongs to site {ex.ActualSiteId} but request targeted site {ex.ExpectedSiteId}.",
                StatusCodes.Status403Forbidden));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("~/api/sites/{siteId:guid}/calibrations/overdue")]
    public async Task<ActionResult<IReadOnlyList<CalibrationResponse>>> GetOverdueCalibrations(
        Guid siteId,
        [FromQuery] DateTime? dueBeforeUtc,
        CancellationToken cancellationToken)
    {
        var responses = await _calibrationService.GetOverdueAsync(siteId, dueBeforeUtc, cancellationToken).ConfigureAwait(false);
        return Ok(responses);
    }

    private Guid ResolveUserId()
    {
        if (Request.Headers.TryGetValue("X-User-Id", out var values) && Guid.TryParse(values.SingleOrDefault(), out var headerId))
        {
            return headerId;
        }

        var claim = User?.FindFirst("sub")?.Value ?? User?.FindFirst("oid")?.Value;
        return Guid.TryParse(claim, out var parsed) ? parsed : Guid.Empty;
    }

    private ProblemDetails CreateProblem(string title, string detail, int statusCode = StatusCodes.Status400BadRequest)
    {
        var problem = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = statusCode
        };

        if (HttpContext != null)
        {
            problem.Extensions["traceId"] = HttpContext.TraceIdentifier;
        }

        return problem;
    }
}
