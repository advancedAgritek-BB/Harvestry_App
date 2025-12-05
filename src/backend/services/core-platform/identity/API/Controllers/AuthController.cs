using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;

namespace Harvestry.Identity.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IBadgeAuthService _badgeAuthService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IBadgeAuthService badgeAuthService, ILogger<AuthController> logger)
    {
        _badgeAuthService = badgeAuthService ?? throw new ArgumentNullException(nameof(badgeAuthService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("badge-login")]
    [AllowAnonymous]
    [EnableRateLimiting("badge-login")]
    public async Task<IActionResult> BadgeLogin([FromBody] BadgeLoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userAgent = Request.Headers.UserAgent.ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await _badgeAuthService.LoginWithBadgeAsync(
            request.BadgeCode,
            request.SiteId,
            ipAddress,
            userAgent,
            cancellationToken);

        if (!result.Success)
        {
            return Unauthorized(new { error = result.ErrorMessage ?? "Invalid credentials" });
        }

        return Ok(new
        {
            result.SessionToken,
            result.ExpiresAt,
            result.UserId,
            result.SessionId
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        // Verify the session belongs to the authenticated user
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var activeSessions = await _badgeAuthService.GetActiveSessionsAsync(userId, cancellationToken);
        if (!activeSessions.Any(s => s.SessionId == request.SessionId))
        {
            return NotFound();
        }

        var success = await _badgeAuthService.RevokeSessionAsync(request.SessionId, request.Reason, cancellationToken);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var sessions = await _badgeAuthService.GetActiveSessionsAsync(userId, cancellationToken);
        return Ok(sessions);
    }

    private bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("user_id");
        return claim != null && Guid.TryParse(claim.Value, out userId);
    }

    public sealed class BadgeLoginRequest
    {
        public string BadgeCode { get; init; } = null!;

        public Guid SiteId { get; init; }
    }

    public sealed class LogoutRequest
    {
        public Guid SessionId { get; init; }

        public string? Reason { get; init; }
    }
}
