using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Harvestry.Tasks.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/sites/{siteId:guid}/slack")]
public sealed class SlackController : ApiControllerBase
{
    private readonly ISlackConfigurationService _slackConfigurationService;

    public SlackController(
        ISlackConfigurationService slackConfigurationService,
        ILogger<SlackController> logger,
        IConfiguration configuration)
        : base(logger, configuration)
    {
        _slackConfigurationService = slackConfigurationService ?? throw new ArgumentNullException(nameof(slackConfigurationService));
    }

    [HttpGet("workspaces")]
    public async Task<ActionResult<IReadOnlyList<SlackWorkspaceResponse>>> GetWorkspaces(
        Guid siteId,
        CancellationToken cancellationToken)
    {
        var workspaces = await _slackConfigurationService
            .GetWorkspacesAsync(siteId, cancellationToken)
            .ConfigureAwait(false);
        return Ok(workspaces);
    }

    [HttpPost("workspaces")]
    public async Task<ActionResult<SlackWorkspaceResponse>> UpsertWorkspace(
        Guid siteId,
        [FromBody] SlackWorkspaceRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        try
        {
            var workspace = await _slackConfigurationService
                .CreateOrUpdateWorkspaceAsync(siteId, request, userId, cancellationToken)
                .ConfigureAwait(false);
            return Ok(workspace);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(CreateProblem("Invalid request", ex.Message));
        }
    }

    [HttpGet("workspaces/{slackWorkspaceId:guid}/channels")]
    public async Task<ActionResult<IReadOnlyList<SlackChannelMappingResponse>>> GetChannelMappings(
        Guid siteId,
        Guid slackWorkspaceId,
        CancellationToken cancellationToken)
    {
        var mappings = await _slackConfigurationService
            .GetChannelMappingsAsync(siteId, slackWorkspaceId, cancellationToken)
            .ConfigureAwait(false);
        return Ok(mappings);
    }

    [HttpPost("workspaces/{slackWorkspaceId:guid}/channels")]
    public async Task<ActionResult<SlackChannelMappingResponse>> CreateChannelMapping(
        Guid siteId,
        Guid slackWorkspaceId,
        [FromBody] SlackChannelMappingRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        try
        {
            var mapping = await _slackConfigurationService
                .CreateChannelMappingAsync(siteId, slackWorkspaceId, request, userId, cancellationToken)
                .ConfigureAwait(false);
            return Ok(mapping);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem("Workspace not found", ex.Message, StatusCodes.Status404NotFound));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(CreateProblem("Invalid request", ex.Message));
        }
    }

    [HttpDelete("channels/{slackChannelMappingId:guid}")]
    public async Task<IActionResult> DeleteChannelMapping(
        Guid siteId,
        Guid slackChannelMappingId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        try
        {
            await _slackConfigurationService
                .DeleteChannelMappingAsync(siteId, slackChannelMappingId, userId, cancellationToken)
                .ConfigureAwait(false);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem("Channel mapping not found", ex.Message, StatusCodes.Status404NotFound));
        }
    }
}
