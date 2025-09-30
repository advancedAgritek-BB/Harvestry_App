using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Application.Interfaces;
using Harvestry.Identity.Domain.Entities;
using Harvestry.Identity.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Harvestry.Identity.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class BadgesController : ControllerBase
{
    private readonly IBadgeRepository _badgeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBadgeAuthService _badgeAuthService;
    private readonly IPolicyEvaluationService _policyEvaluationService;
    private readonly ILogger<BadgesController> _logger;

    public BadgesController(
        IBadgeRepository badgeRepository,
        IUserRepository userRepository,
        IBadgeAuthService badgeAuthService,
        IPolicyEvaluationService policyEvaluationService,
        ILogger<BadgesController> logger)
    {
        _badgeRepository = badgeRepository ?? throw new ArgumentNullException(nameof(badgeRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _badgeAuthService = badgeAuthService ?? throw new ArgumentNullException(nameof(badgeAuthService));
        _policyEvaluationService = policyEvaluationService ?? throw new ArgumentNullException(nameof(policyEvaluationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> IssueBadge([FromBody] IssueBadgeRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return NotFound(new { error = "User not found" });
        }

        var siteId = request.SiteId;
        var authorization = await AuthorizeAsync(siteId, "badges:create", "badge", cancellationToken);
        if (authorization != null)
        {
            return authorization;
        }

        var badgeCode = string.IsNullOrWhiteSpace(request.BadgeCode)
            ? BadgeCode.Create(Guid.NewGuid().ToString("N").ToUpperInvariant())
            : BadgeCode.Create(request.BadgeCode);

        var badge = Badge.Create(request.UserId, request.SiteId, badgeCode, request.BadgeType, request.ExpiresAt);
        await _badgeRepository.AddAsync(badge, cancellationToken);

        return CreatedAtAction(nameof(GetBadgeById), new { siteId = request.SiteId, id = badge.Id }, BadgeResponse.From(badge));
    }

    [HttpPut("{id:guid}/revoke")]
    [Authorize]
    public async Task<IActionResult> RevokeBadge(Guid id, [FromBody] RevokeBadgeRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var badge = await _badgeRepository.GetByIdAsync(id, cancellationToken);
        if (badge == null)
        {
            return NotFound();
        }

        var authorization = await AuthorizeAsync(badge.SiteId, "badges:revoke", "badge", cancellationToken);
        if (authorization != null)
        {
            return authorization;
        }

        var success = await _badgeAuthService.RevokeBadgeAsync(id, request.RevokedBy, request.Reason, cancellationToken);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("site/{siteId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetBadgesForSite(Guid siteId, CancellationToken cancellationToken)
    {
        var authorization = await AuthorizeAsync(siteId, "badges:read", "badge", cancellationToken);
        if (authorization != null)
        {
            return authorization;
        }

        var badges = await _badgeRepository.GetActiveBySiteIdAsync(siteId, cancellationToken);
        return Ok(badges.Select(BadgeResponse.From));
    }

    [HttpGet("site/{siteId:guid}/badges/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetBadgeById(Guid siteId, Guid id, CancellationToken cancellationToken)
    {
        var authorization = await AuthorizeAsync(siteId, "badges:read", "badge", cancellationToken);
        if (authorization != null)
        {
            return authorization;
        }

        var badge = await _badgeRepository.GetByIdAsync(id, cancellationToken);
        if (badge == null || badge.SiteId != siteId)
        {
            return NotFound();
        }

        return Ok(BadgeResponse.From(badge));
    }

    private bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("user_id");
        return claim != null && Guid.TryParse(claim.Value, out userId);
    }

    private async Task<IActionResult?> AuthorizeAsync(Guid siteId, string action, string resourceType, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        var result = await _policyEvaluationService.EvaluatePermissionAsync(
            currentUserId,
            action,
            resourceType,
            siteId,
            null,
            cancellationToken);

        if (!result.IsGranted)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = result.DenyReason ?? "Permission denied" });
        }

        return null;
    }

    public sealed class IssueBadgeRequest
    {
        [Required]
        public Guid UserId { get; init; }

        [Required]
        public Guid SiteId { get; init; }

        [StringLength(100, MinimumLength = 4)]
        public string? BadgeCode { get; init; }

        public BadgeType BadgeType { get; init; } = BadgeType.Physical;

        public DateTime? ExpiresAt { get; init; }
    }

    public sealed class RevokeBadgeRequest
    {
        [Required]
        public Guid RevokedBy { get; init; }

        [Required]
        [StringLength(512, MinimumLength = 3)]
        public string Reason { get; init; } = null!;
    }

    public sealed class BadgeResponse
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public Guid SiteId { get; init; }
        public string BadgeCode { get; init; } = null!;
        public string BadgeType { get; init; } = null!;
        public string Status { get; init; } = null!;
        public DateTime IssuedAt { get; init; }
        public DateTime? ExpiresAt { get; init; }
        public DateTime? LastUsedAt { get; init; }

        public static BadgeResponse From(Badge badge) => new()
        {
            Id = badge.Id,
            UserId = badge.UserId,
            SiteId = badge.SiteId,
            BadgeCode = badge.BadgeCode.Value,
            BadgeType = badge.BadgeType.ToString(),
            Status = badge.Status.ToString(),
            IssuedAt = badge.IssuedAt,
            ExpiresAt = badge.ExpiresAt,
            LastUsedAt = badge.LastUsedAt
        };
    }
}
