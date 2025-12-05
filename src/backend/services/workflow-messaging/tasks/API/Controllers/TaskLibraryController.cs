using System;
using System.Collections.Generic;
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
[Route("api/v1/orgs/{orgId:guid}/task-library")]
public sealed class TaskLibraryController : ApiControllerBase
{
    private readonly ITaskLibraryService _libraryService;

    public TaskLibraryController(
        ITaskLibraryService libraryService,
        ILogger<TaskLibraryController> logger,
        IConfiguration configuration)
        : base(logger, configuration)
    {
        _libraryService = libraryService ?? throw new ArgumentNullException(nameof(libraryService));
    }

    [HttpPost]
    public async Task<ActionResult<TaskLibraryItemResponse>> CreateItem(
        Guid orgId,
        [FromBody] CreateTaskLibraryItemRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        try
        {
            var response = await _libraryService
                .CreateItemAsync(orgId, request, userId, cancellationToken)
                .ConfigureAwait(false);

            return CreatedAtAction(nameof(GetItem), new { orgId, itemId = response.Id }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(CreateProblem("Invalid request", ex.Message));
        }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaskLibraryItemResponse>>> GetItems(
        Guid orgId,
        [FromQuery] bool? activeOnly,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        var response = await _libraryService
            .GetItemsByOrgAsync(orgId, activeOnly, cancellationToken)
            .ConfigureAwait(false);

        return Ok(response);
    }

    [HttpGet("{itemId:guid}")]
    public async Task<ActionResult<TaskLibraryItemResponse>> GetItem(
        Guid orgId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        var item = await _libraryService
            .GetItemByIdAsync(orgId, itemId, cancellationToken)
            .ConfigureAwait(false);

        if (item is null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    [HttpPut("{itemId:guid}")]
    public async Task<ActionResult<TaskLibraryItemResponse>> UpdateItem(
        Guid orgId,
        Guid itemId,
        [FromBody] UpdateTaskLibraryItemRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        try
        {
            var response = await _libraryService
                .UpdateItemAsync(orgId, itemId, request, userId, cancellationToken)
                .ConfigureAwait(false);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem("Item not found", ex.Message, StatusCodes.Status404NotFound));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(CreateProblem("Invalid request", ex.Message));
        }
    }

    [HttpPost("{itemId:guid}/activate")]
    public async Task<ActionResult<TaskLibraryItemResponse>> ActivateItem(
        Guid orgId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        try
        {
            var response = await _libraryService
                .ActivateItemAsync(orgId, itemId, userId, cancellationToken)
                .ConfigureAwait(false);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem("Item not found", ex.Message, StatusCodes.Status404NotFound));
        }
    }

    [HttpPost("{itemId:guid}/deactivate")]
    public async Task<ActionResult<TaskLibraryItemResponse>> DeactivateItem(
        Guid orgId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        try
        {
            var response = await _libraryService
                .DeactivateItemAsync(orgId, itemId, userId, cancellationToken)
                .ConfigureAwait(false);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem("Item not found", ex.Message, StatusCodes.Status404NotFound));
        }
    }

    [HttpDelete("{itemId:guid}")]
    public async Task<ActionResult> DeleteItem(
        Guid orgId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        await _libraryService
            .DeleteItemAsync(orgId, itemId, userId, cancellationToken)
            .ConfigureAwait(false);

        return NoContent();
    }
}

