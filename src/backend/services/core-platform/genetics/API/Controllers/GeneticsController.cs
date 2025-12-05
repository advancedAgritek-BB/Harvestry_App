using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Genetics.API.Controllers;

/// <summary>
/// Controller for managing genetics and phenotypes
/// </summary>
[ApiController]
[Route("api/v1/sites/{siteId:guid}/genetics")]
[Authorize]
public sealed class GeneticsController : ControllerBase
{
    private readonly IGeneticsManagementService _geneticsService;

    public GeneticsController(IGeneticsManagementService geneticsService)
    {
        _geneticsService = geneticsService ?? throw new ArgumentNullException(nameof(geneticsService));
    }

    // ===== Genetics Endpoints =====

    /// <summary>
    /// Get all genetics for a site
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<GeneticsResponse>>> GetGenetics(
        Guid siteId,
        CancellationToken cancellationToken)
    {
        var genetics = await _geneticsService.GetGeneticsBySiteAsync(siteId, cancellationToken);
        return Ok(genetics);
    }

    /// <summary>
    /// Get genetics by ID
    /// </summary>
    [HttpGet("{geneticsId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GeneticsResponse>> GetGeneticsById(
        Guid siteId,
        Guid geneticsId,
        CancellationToken cancellationToken)
    {
        var genetics = await _geneticsService.GetGeneticsByIdAsync(siteId, geneticsId, cancellationToken);
        
        if (genetics == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Genetics not found",
                Detail = $"Genetics with ID {geneticsId} was not found for site {siteId}."
            });
        }

        return Ok(genetics);
    }

    /// <summary>
    /// Create new genetics
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GeneticsResponse>> CreateGenetics(
        Guid siteId,
        [FromBody] CreateGeneticsRequest request,
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
            var genetics = await _geneticsService.CreateGeneticsAsync(siteId, request, userId, cancellationToken);
            return CreatedAtAction(nameof(GetGeneticsById), new { siteId, geneticsId = genetics.Id }, genetics);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Failed to create genetics",
                Detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Update genetics
    /// </summary>
    [HttpPut("{geneticsId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GeneticsResponse>> UpdateGenetics(
        Guid siteId,
        Guid geneticsId,
        [FromBody] UpdateGeneticsRequest request,
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
            var genetics = await _geneticsService.UpdateGeneticsAsync(siteId, geneticsId, request, userId, cancellationToken);
            return Ok(genetics);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Failed to update genetics",
                Detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Delete genetics
    /// </summary>
    [HttpDelete("{geneticsId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteGenetics(
        Guid siteId,
        Guid geneticsId,
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
            await _geneticsService.DeleteGeneticsAsync(siteId, geneticsId, userId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Failed to delete genetics",
                Detail = ex.Message
            });
        }
    }

    // ===== Phenotype Endpoints =====

    /// <summary>
    /// Get phenotypes for a genetics
    /// </summary>
    [HttpGet("{geneticsId:guid}/phenotypes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PhenotypeResponse>>> GetPhenotypesByGenetics(
        Guid siteId,
        Guid geneticsId,
        CancellationToken cancellationToken)
    {
        var phenotypes = await _geneticsService.GetPhenotypesByGeneticsAsync(geneticsId, cancellationToken);
        return Ok(phenotypes);
    }

    /// <summary>
    /// Get phenotype by ID
    /// </summary>
    [HttpGet("phenotypes/{phenotypeId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PhenotypeResponse>> GetPhenotypeById(
        Guid siteId,
        Guid phenotypeId,
        CancellationToken cancellationToken)
    {
        var phenotype = await _geneticsService.GetPhenotypeByIdAsync(siteId, phenotypeId, cancellationToken);
        
        if (phenotype == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Phenotype not found",
                Detail = $"Phenotype with ID {phenotypeId} was not found for site {siteId}."
            });
        }

        return Ok(phenotype);
    }

    /// <summary>
    /// Create new phenotype
    /// </summary>
    [HttpPost("phenotypes")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PhenotypeResponse>> CreatePhenotype(
        Guid siteId,
        [FromBody] CreatePhenotypeRequest request,
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
            var phenotype = await _geneticsService.CreatePhenotypeAsync(siteId, request, userId, cancellationToken);
            return CreatedAtAction(nameof(GetPhenotypeById), new { siteId, phenotypeId = phenotype.Id }, phenotype);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Failed to create phenotype",
                Detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Update phenotype
    /// </summary>
    [HttpPut("phenotypes/{phenotypeId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PhenotypeResponse>> UpdatePhenotype(
        Guid siteId,
        Guid phenotypeId,
        [FromBody] UpdatePhenotypeRequest request,
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
            var phenotype = await _geneticsService.UpdatePhenotypeAsync(siteId, phenotypeId, request, userId, cancellationToken);
            return Ok(phenotype);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Failed to update phenotype",
                Detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Delete phenotype
    /// </summary>
    [HttpDelete("phenotypes/{phenotypeId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePhenotype(
        Guid siteId,
        Guid phenotypeId,
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
            await _geneticsService.DeletePhenotypeAsync(siteId, phenotypeId, userId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Failed to delete phenotype",
                Detail = ex.Message
            });
        }
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
