using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Harvestry.Tasks.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/notifications")]
public sealed class NotificationsController : ApiControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(
        INotificationService notificationService,
        ILogger<NotificationsController> logger,
        IConfiguration configuration)
        : base(logger, configuration)
    {
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserNotificationResponse>>> GetNotifications(
        [FromQuery] bool? unreadOnly,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        var response = await _notificationService
            .GetNotificationsAsync(userId, unreadOnly, Math.Min(limit, 100), cancellationToken)
            .ConfigureAwait(false);

        return Ok(response);
    }

    [HttpGet("count")]
    public async Task<ActionResult<NotificationCountResponse>> GetUnreadCount(
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        var count = await _notificationService
            .GetUnreadCountAsync(userId, cancellationToken)
            .ConfigureAwait(false);

        return Ok(new NotificationCountResponse { UnreadCount = count });
    }

    [HttpPost("{notificationId:guid}/read")]
    public async Task<ActionResult> MarkAsRead(
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        await _notificationService
            .MarkAsReadAsync(userId, notificationId, cancellationToken)
            .ConfigureAwait(false);

        return NoContent();
    }

    [HttpPost("read-all")]
    public async Task<ActionResult> MarkAllAsRead(
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        await _notificationService
            .MarkAllAsReadAsync(userId, cancellationToken)
            .ConfigureAwait(false);

        return NoContent();
    }
}

