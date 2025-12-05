using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Genetics.API.Controllers;

/// <summary>
/// Controller for managing strains
/// </summary>
[ApiController]
[Route("api/v1/sites/{siteId:guid}/strains")]
[Authorize]
public sealed class StrainsController : ControllerBase
{
    private readonly IGeneticsManagementService _geneticsService;

    public StrainsController(IGeneticsManagementService geneticsService)
    {
        _geneticsService = geneticsService ?? throw new ArgumentNullException(nameof(geneticsService));
    }

    /// <summary>
    /// Get all strains for a site
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StrainResponse>>> GetStrains(
        Guid siteId,
        CancellationToken cancellationToken)
    {
        var strains = await _geneticsService.GetStrainsBySiteAsync(siteId, cancellationToken);
        return Ok(strains);
    }

    /// <summary>
    /// Get strains by genetics ID
    /// </summary>
    [HttpGet("by-genetics/{geneticsId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StrainResponse>>> GetStrainsByGenetics(
        Guid siteId,
        Guid geneticsId,
        CancellationToken cancellationToken)
    {
        var strains = await _geneticsService.GetStrainsByGeneticsAsync(geneticsId, cancellationToken);
        return Ok(strains);
    }

    /// <summary>
    /// Get strain by ID
    /// </summary>
    [HttpGet("{strainId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StrainResponse>> GetStrainById(
        Guid siteId,
        Guid strainId,
        CancellationToken cancellationToken)
    {
        var strain = await _geneticsService.GetStrainByIdAsync(siteId, strainId, cancellationToken);
        
        if (strain == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Strain not found",
                Detail = $"Strain with ID {strainId} was not found for site {siteId}."
            });
        }

        return Ok(strain);
    }

    /// <summary>
    /// Create new strain
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StrainResponse>> CreateStrain(
        Guid siteId,
        [FromBody] CreateStrainRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Missing user identifier",
                Detail = "Provide an X-User-Id header with a valid GUID."
            });
        }

        try
        {
            var strain = await _geneticsService.CreateStrainAsync(siteId, request, userId, cancellationToken);
            return CreatedAtAction(nameof(GetStrainById), new { siteId, strainId = strain.Id }, strain);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Failed to create strain",
                Detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Update strain
    /// </summary>
    [HttpPut("{strainId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StrainResponse>> UpdateStrain(
        Guid siteId,
        Guid strainId,
        [FromBody] UpdateStrainRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Missing user identifier",
                Detail = "Provide an X-User-Id header with a valid GUID."
            });
        }

        try
        {
            var strain = await _geneticsService.UpdateStrainAsync(siteId, strainId, request, userId, cancellationToken);
            return Ok(strain);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Failed to update strain",
                Detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Delete strain
    /// </summary>
    [HttpDelete("{strainId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStrain(
        Guid siteId,
        Guid strainId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Missing user identifier",
                Detail = "Provide an X-User-Id header with a valid GUID."
            });
        }

        try
        {
            await _geneticsService.DeleteStrainAsync(siteId, strainId, userId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Failed to delete strain",
                Detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Check if strain can be deleted
    /// </summary>
    [HttpGet("{strainId:guid}/can-delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> CanDeleteStrain(
        Guid siteId,
        Guid strainId,
        CancellationToken cancellationToken)
    {
        var canDelete = await _geneticsService.CanDeleteStrainAsync(strainId, cancellationToken);
        return Ok(new { canDelete });
    }

    // ===== Helper Methods =====

    private Guid ResolveUserId()
    {
        if (Request.Headers.TryGetValue("X-User-Id", out var userIdString) &&
            Guid.TryParse(userIdString, out var userId))
        {
            return userId;
        }

        return Guid.Empty;
    }
}
