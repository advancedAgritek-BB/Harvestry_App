using Harvestry.Labor.Application.DTOs;
using Harvestry.Labor.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Harvestry.Labor.API.Controllers;

/// <summary>
/// API controller for Team management and member assignment
/// </summary>
[ApiController]
[Route("api/v1/sites/{siteId:guid}/teams")]
[Authorize]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _teamService;
    private readonly ILogger<TeamsController> _logger;

    public TeamsController(ITeamService teamService, ILogger<TeamsController> logger)
    {
        _teamService = teamService ?? throw new ArgumentNullException(nameof(teamService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all teams for a site
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TeamDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TeamDto>>> GetTeams(
        [FromRoute] Guid siteId,
        CancellationToken ct = default)
    {
        var teams = await _teamService.GetTeamsAsync(siteId, ct);
        return Ok(teams);
    }

    /// <summary>
    /// Get team details with members
    /// </summary>
    [HttpGet("{teamId:guid}")]
    [ProducesResponseType(typeof(TeamDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeamDetailDto>> GetTeam(
        [FromRoute] Guid siteId,
        [FromRoute] Guid teamId,
        CancellationToken ct = default)
    {
        var team = await _teamService.GetTeamDetailAsync(teamId, ct);
        if (team == null)
            return NotFound();

        return Ok(team);
    }

    /// <summary>
    /// Get teams the current user manages (is team lead or has manager/supervisor role)
    /// </summary>
    [HttpGet("managed")]
    [ProducesResponseType(typeof(IReadOnlyList<TeamDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TeamDto>>> GetManagedTeams(
        [FromRoute] Guid siteId,
        CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var teams = await _teamService.GetManagedTeamsAsync(userId, siteId, ct);
        return Ok(teams);
    }

    /// <summary>
    /// Get all members the current user can assign to tasks
    /// Returns members grouped by team
    /// </summary>
    [HttpGet("assignable-members")]
    [ProducesResponseType(typeof(AssignableMembersResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AssignableMembersResponse>> GetAssignableMembers(
        [FromRoute] Guid siteId,
        CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var members = await _teamService.GetAssignableMembersAsync(userId, siteId, ct);
        return Ok(members);
    }

    /// <summary>
    /// Create a new team
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TeamDto>> CreateTeam(
        [FromRoute] Guid siteId,
        [FromBody] CreateTeamRequest request,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetCurrentUserId();

        try
        {
            var team = await _teamService.CreateTeamAsync(siteId, request, userId, ct);
            return CreatedAtAction(nameof(GetTeam), new { siteId, teamId = team.Id }, team);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing team
    /// </summary>
    [HttpPut("{teamId:guid}")]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeamDto>> UpdateTeam(
        [FromRoute] Guid siteId,
        [FromRoute] Guid teamId,
        [FromBody] UpdateTeamRequest request,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetCurrentUserId();

        try
        {
            var team = await _teamService.UpdateTeamAsync(teamId, request, userId, ct);
            return Ok(team);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Archive (soft delete) a team
    /// </summary>
    [HttpDelete("{teamId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTeam(
        [FromRoute] Guid siteId,
        [FromRoute] Guid teamId,
        CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();

        try
        {
            await _teamService.DeleteTeamAsync(teamId, userId, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Add a member to a team
    /// </summary>
    [HttpPost("{teamId:guid}/members")]
    [ProducesResponseType(typeof(TeamMemberDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeamMemberDto>> AddMember(
        [FromRoute] Guid siteId,
        [FromRoute] Guid teamId,
        [FromBody] AddTeamMemberRequest request,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var member = await _teamService.AddMemberAsync(teamId, request, ct);
            return Created($"/api/v1/sites/{siteId}/teams/{teamId}/members/{request.UserId}", member);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Remove a member from a team
    /// </summary>
    [HttpDelete("{teamId:guid}/members/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(
        [FromRoute] Guid siteId,
        [FromRoute] Guid teamId,
        [FromRoute] Guid userId,
        CancellationToken ct = default)
    {
        var currentUserId = GetCurrentUserId();

        try
        {
            await _teamService.RemoveMemberAsync(teamId, userId, currentUserId, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found") || ex.Message.Contains("not an active member"))
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Set or remove team lead status for a member
    /// </summary>
    [HttpPatch("{teamId:guid}/members/{userId:guid}/lead")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetTeamLead(
        [FromRoute] Guid siteId,
        [FromRoute] Guid teamId,
        [FromRoute] Guid userId,
        [FromBody] SetTeamLeadRequest request,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await _teamService.SetTeamLeadAsync(teamId, userId, request, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found") || ex.Message.Contains("not an active member"))
        {
            return NotFound();
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("user_id");
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        _logger.LogWarning("Could not extract user ID from claims, using default");
        return Guid.Empty;
    }
}
