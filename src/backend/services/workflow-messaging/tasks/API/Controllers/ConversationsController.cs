using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Harvestry.Tasks.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/sites/{siteId:guid}/conversations")]
public sealed class ConversationsController : ApiControllerBase
{
    private readonly IConversationService _conversationService;
    private readonly ISiteAuthorizationService _siteAuthorizationService;

    public ConversationsController(
        IConversationService conversationService,
        ISiteAuthorizationService siteAuthorizationService,
        ILogger<ConversationsController> logger,
        IConfiguration configuration)
        : base(logger, configuration)
    {
        _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
        _siteAuthorizationService = siteAuthorizationService ?? throw new ArgumentNullException(nameof(siteAuthorizationService));
    }

    [HttpPost]
    public async Task<ActionResult<ConversationResponse>> CreateConversation(
        Guid siteId,
        [FromBody] CreateConversationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        // Check if user has access to the site
        if (!await _siteAuthorizationService.HasSiteAccessAsync(userId, siteId, cancellationToken))
        {
            return Forbid();
        }

        try
        {
            var response = await _conversationService
                .CreateConversationAsync(siteId, request, userId, cancellationToken)
                .ConfigureAwait(false);

            return CreatedAtAction(nameof(GetConversation), new { siteId, conversationId = response.ConversationId }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(CreateProblem("Invalid request", ex.Message));
        }
    }

    [HttpGet("{conversationId:guid}")]
    public async Task<ActionResult<ConversationResponse>> GetConversation(
        Guid siteId,
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        // Check if user has access to the site
        if (!await _siteAuthorizationService.HasSiteAccessAsync(userId, siteId, cancellationToken))
        {
            return Forbid();
        }

        // Check if user is a participant in the conversation
        if (!await _conversationService.IsUserParticipantAsync(siteId, conversationId, userId, cancellationToken))
        {
            return Forbid();
        }

        var conversation = await _conversationService
            .GetConversationByIdAsync(siteId, conversationId, cancellationToken)
            .ConfigureAwait(false);

        if (conversation is null)
        {
            return NotFound();
        }

        return Ok(conversation);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ConversationResponse>>> GetConversationsByEntity(
        Guid siteId,
        [FromQuery] string? relatedEntityType,
        [FromQuery] Guid? relatedEntityId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        if (string.IsNullOrWhiteSpace(relatedEntityType) || !relatedEntityId.HasValue || relatedEntityId == Guid.Empty)
        {
            return BadRequest(CreateProblem(
                "Invalid request",
                "Provide relatedEntityType and relatedEntityId query parameters."));
        }

        // Check if user has access to the site
        if (!await _siteAuthorizationService.HasSiteAccessAsync(userId, siteId, cancellationToken))
        {
            return Forbid();
        }

        var responses = await _conversationService
            .GetConversationsByEntityAsync(siteId, relatedEntityType!, relatedEntityId!.Value, cancellationToken)
            .ConfigureAwait(false);

        return Ok(responses);
    }

    [HttpPost("{conversationId:guid}/messages")]
    public async Task<ActionResult<MessageResponse>> PostMessage(
        Guid siteId,
        Guid conversationId,
        [FromBody] PostMessageRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        // Check if user has access to the site
        if (!await _siteAuthorizationService.HasSiteAccessAsync(userId, siteId, cancellationToken))
        {
            return Forbid();
        }

        // Check if user is a participant in the conversation
        if (!await _conversationService.IsUserParticipantAsync(siteId, conversationId, userId, cancellationToken))
        {
            return Forbid();
        }

        try
        {
            var response = await _conversationService
                .PostMessageAsync(siteId, conversationId, request, userId, cancellationToken)
                .ConfigureAwait(false);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem("Conversation not found", ex.Message, StatusCodes.Status404NotFound));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(CreateProblem("Invalid request", ex.Message));
        }
    }

    [HttpGet("{conversationId:guid}/messages")]
    public async Task<ActionResult<IReadOnlyList<MessageResponse>>> GetMessages(
        Guid siteId,
        Guid conversationId,
        [FromQuery] int? limit,
        [FromQuery] DateTimeOffset? since,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        if (limit.HasValue && limit.Value <= 0)
        {
            return BadRequest(CreateProblem("Invalid request", "Limit must be greater than zero when provided."));
        }

        // Check if user has access to the site
        if (!await _siteAuthorizationService.HasSiteAccessAsync(userId, siteId, cancellationToken))
        {
            return Forbid();
        }

        // Check if user is a participant in the conversation
        if (!await _conversationService.IsUserParticipantAsync(siteId, conversationId, userId, cancellationToken))
        {
            return Forbid();
        }

        try
        {
            var response = await _conversationService
                .GetMessagesAsync(siteId, conversationId, limit, since, cancellationToken)
                .ConfigureAwait(false);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem("Conversation not found", ex.Message, StatusCodes.Status404NotFound));
        }
    }

    [HttpPost("messages/{messageId:guid}/read")]
    public async Task<IActionResult> MarkMessageRead(
        Guid siteId,
        Guid messageId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        // Check if user has access to the site
        if (!await _siteAuthorizationService.HasSiteAccessAsync(userId, siteId, cancellationToken))
        {
            return Forbid();
        }

        try
        {
            await _conversationService
                .MarkMessageReadAsync(siteId, messageId, userId, cancellationToken)
                .ConfigureAwait(false);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem("Message not found", ex.Message, StatusCodes.Status404NotFound));
        }
    }

    [HttpPost("{conversationId:guid}/participants")]
    public async Task<IActionResult> AddParticipant(
        Guid siteId,
        Guid conversationId,
        [FromBody] ModifyParticipantRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null || request.UserId == Guid.Empty)
        {
            return BadRequest(CreateProblem("Invalid request", "Participant user identifier is required."));
        }

        var addedBy = ResolveUserId();
        if (addedBy == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        // Check if user has access to the site
        if (!await _siteAuthorizationService.HasSiteAccessAsync(addedBy, siteId, cancellationToken))
        {
            return Forbid();
        }

        // Check if user is admin in the site for administrative actions
        if (!await _siteAuthorizationService.IsSiteAdminAsync(addedBy, siteId, cancellationToken))
        {
            return Forbid();
        }

        try
        {
            await _conversationService
                .AddParticipantAsync(siteId, conversationId, request.UserId, addedBy, cancellationToken)
                .ConfigureAwait(false);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem("Conversation not found", ex.Message, StatusCodes.Status404NotFound));
        }
    }

    [HttpDelete("{conversationId:guid}/participants/{participantUserId:guid}")]
    public async Task<IActionResult> RemoveParticipant(
        Guid siteId,
        Guid conversationId,
        Guid participantUserId,
        CancellationToken cancellationToken)
    {
        var removedBy = ResolveUserId();
        if (removedBy == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        // Check if user has access to the site
        if (!await _siteAuthorizationService.HasSiteAccessAsync(removedBy, siteId, cancellationToken))
        {
            return Forbid();
        }

        // Check if user is admin in the site for administrative actions
        if (!await _siteAuthorizationService.IsSiteAdminAsync(removedBy, siteId, cancellationToken))
        {
            return Forbid();
        }

        try
        {
            await _conversationService
                .RemoveParticipantAsync(siteId, conversationId, participantUserId, removedBy, cancellationToken)
                .ConfigureAwait(false);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem("Conversation not found", ex.Message, StatusCodes.Status404NotFound));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(CreateProblem("Invalid operation", ex.Message));
        }
    }

}

