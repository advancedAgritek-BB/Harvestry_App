using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Application.Interfaces;
using Harvestry.Identity.Domain.Entities;
using Harvestry.Identity.Domain.ValueObjects;
using DomainUser = Harvestry.Identity.Domain.Entities.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Harvestry.Identity.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IPolicyEvaluationService _policyEvaluationService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserRepository userRepository,
        IPolicyEvaluationService policyEvaluationService,
        ILogger<UsersController> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _policyEvaluationService = policyEvaluationService ?? throw new ArgumentNullException(nameof(policyEvaluationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken)
    {
        // Check authorization before revealing user existence
        if (!IsCurrentUser(id))
        {
            var siteId = ResolveSiteId();
            if (!siteId.HasValue)
            {
                return BadRequest(new { error = "Site context required" });
            }

            var authorization = await AuthorizeAsync(siteId.Value, "users:read", "user", cancellationToken);
            if (authorization != null)
            {
                return authorization;
            }
        }

        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            // Return Forbid instead of NotFound to prevent information leakage
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        return Ok(UserResponse.From(user));
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        Email email;
        PhoneNumber? phoneNumber = null;

        try
        {
            email = Email.Create(request.Email);
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                phoneNumber = PhoneNumber.Create(request.PhoneNumber);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate user contact info for email {Email}", request.Email);
            return BadRequest(new { error = "Invalid email or phone number" });
        }

        var user = DomainUser.Create(email, request.FirstName, request.LastName, phoneNumber);

        var siteId = request.SiteAssignments?.FirstOrDefault()?.SiteId ?? ResolveSiteId();
        if (!siteId.HasValue)
        {
            return BadRequest(new { error = "Site context required" });
        }

        var authorization = await AuthorizeAsync(siteId.Value, "users:create", "user", cancellationToken);
        if (authorization != null)
        {
            return authorization;
        }

        if (request.SiteAssignments != null)
        {
            // Validate at most one primary site assignment
            var primaryCount = request.SiteAssignments.Count(a => a.IsPrimarySite);
            if (primaryCount > 1)
            {
                return BadRequest(new { error = "At most one primary site assignment is allowed" });
            }

            foreach (var assignment in request.SiteAssignments)
            {
                user.AssignToSite(assignment.SiteId, assignment.RoleId, assignment.IsPrimarySite, assignment.AssignedBy);
            }
        }

        await _userRepository.AddAsync(user, cancellationToken);

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, UserResponse.From(user));
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return NotFound();
        }

        if (!IsCurrentUser(id))
        {
            var siteId = ResolveSiteId() ?? user.UserSites.FirstOrDefault()?.SiteId;
            if (!siteId.HasValue)
            {
                return BadRequest(new { error = "Site context required" });
            }

            var authorization = await AuthorizeAsync(siteId.Value, "users:update", "user", cancellationToken);
            if (authorization != null)
            {
                return authorization;
            }
        }

        PhoneNumber? phoneNumber = null;
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            try
            {
                phoneNumber = PhoneNumber.Create(request.PhoneNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid phone number while updating user {UserId}", id);
                return BadRequest(new { error = "Invalid phone number" });
            }
        }

        // Note: UpdateProfile uses "no change" semantics for null parameters - existing values are preserved
        user.UpdateProfile(
            firstName: request.FirstName,
            lastName: request.LastName,
            phoneNumber: phoneNumber,
            profilePhotoUrl: request.ProfilePhotoUrl,
            languagePreference: request.LanguagePreference,
            timezone: request.Timezone,
            updatedBy: request.UpdatedBy);

        await _userRepository.UpdateAsync(user, cancellationToken);
        return Ok(UserResponse.From(user));
    }

    [HttpPut("{id:guid}/suspend")]
    [Authorize]
    public async Task<IActionResult> SuspendUser(Guid id, [FromBody] SuspendUserRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return NotFound();
        }

        var siteId = ResolveSiteId() ?? user.UserSites.FirstOrDefault()?.SiteId;
        if (!siteId.HasValue)
        {
            return BadRequest(new { error = "Site context required" });
        }

        var authorization = await AuthorizeAsync(siteId.Value, "users:suspend", "user", cancellationToken);
        if (authorization != null)
        {
            return authorization;
        }

        user.Suspend(request.SuspendedBy, request.Reason);
        await _userRepository.UpdateAsync(user, cancellationToken);
        return Ok(UserResponse.From(user));
    }

    [HttpPut("{id:guid}/unlock")]
    public async Task<IActionResult> UnlockUser(Guid id, [FromBody] UnlockUserRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return NotFound();
        }

        var siteId = ResolveSiteId() ?? user.UserSites.FirstOrDefault()?.SiteId;
        if (!siteId.HasValue)
        {
            return BadRequest(new { error = "Site context required" });
        }

        var authorization = await AuthorizeAsync(siteId.Value, "users:unlock", "user", cancellationToken);
        if (authorization != null)
        {
            return authorization;
        }

        user.Unlock(request.UnlockedBy);
        await _userRepository.UpdateAsync(user, cancellationToken);
        return Ok(UserResponse.From(user));
    }

    private bool IsCurrentUser(Guid userId) => TryGetUserId(out var currentUserId) && currentUserId == userId;

    private Guid? ResolveSiteId()
    {
        if (Request.Headers.TryGetValue("X-Site-Id", out var header) && Guid.TryParse(header.ToString(), out var siteId))
        {
            return siteId;
        }

        return null;
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

    public sealed class CreateUserRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; init; } = null!;

        [Required]
        public string FirstName { get; init; } = null!;

        [Required]
        public string LastName { get; init; } = null!;

        public string? PhoneNumber { get; init; }

        public SiteAssignmentRequest[]? SiteAssignments { get; init; }
    }

    public sealed class SiteAssignmentRequest
    {
        [Required]
        public Guid SiteId { get; init; }

        [Required]
        public Guid RoleId { get; init; }

        public bool IsPrimarySite { get; init; }

        [Required]
        public Guid AssignedBy { get; init; }
    }

    public sealed class UpdateUserRequest
    {
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string? PhoneNumber { get; init; }
        public string? ProfilePhotoUrl { get; init; }
        public string? LanguagePreference { get; init; }
        public string? Timezone { get; init; }
        public Guid? UpdatedBy { get; init; }
    }

    public sealed class SuspendUserRequest
    {
        [Required]
        public Guid SuspendedBy { get; init; }

        [Required]
        [StringLength(1024, MinimumLength = 3)]
        public string Reason { get; init; } = null!;
    }

    public sealed class UnlockUserRequest
    {
        [Required]
        public Guid UnlockedBy { get; init; }
    }

    public sealed class UserResponse
    {
        public Guid Id { get; init; }
        public string Email { get; init; } = null!;
        public string DisplayName { get; init; } = null!;
        public string FirstName { get; init; } = null!;
        public string LastName { get; init; } = null!;
        public string Status { get; init; } = null!;
        public string Timezone { get; init; } = null!;
        public string LanguagePreference { get; init; } = null!;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }

        public static UserResponse From(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email.ToString(),
                DisplayName = user.DisplayName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Status = user.Status.ToString(),
                Timezone = user.Timezone,
                LanguagePreference = user.LanguagePreference,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}
