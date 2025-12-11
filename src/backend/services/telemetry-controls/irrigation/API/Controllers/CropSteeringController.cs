using Harvestry.Genetics.Domain.ValueObjects;
using Harvestry.Irrigation.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Harvestry.Irrigation.API.Controllers;

/// <summary>
/// API controller for crop steering profiles and recommendations.
/// Manages vegetative/generative steering configurations and provides
/// phase-aware suggestions based on current telemetry.
/// </summary>
[ApiController]
[Route("api/v1/steering")]
[Authorize]
public sealed class CropSteeringController : ControllerBase
{
    private readonly ICropSteeringSuggestionService _suggestionService;
    private readonly ICropSteeringProfileRepository _profileRepository;
    private readonly ILogger<CropSteeringController> _logger;

    public CropSteeringController(
        ICropSteeringSuggestionService suggestionService,
        ICropSteeringProfileRepository profileRepository,
        ILogger<CropSteeringController> logger)
    {
        _suggestionService = suggestionService ?? throw new ArgumentNullException(nameof(suggestionService));
        _profileRepository = profileRepository ?? throw new ArgumentNullException(nameof(profileRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Profile Endpoints

    /// <summary>
    /// Get all crop steering profiles for a site.
    /// </summary>
    [HttpGet("profiles")]
    [ProducesResponseType(typeof(IReadOnlyList<CropSteeringProfileResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CropSteeringProfileResponse>>> GetProfiles(
        [FromQuery] Guid siteId,
        CancellationToken cancellationToken)
    {
        if (siteId == Guid.Empty)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid site ID",
                Detail = "A valid site ID is required."
            });
        }

        var profiles = await _profileRepository.GetBySiteIdAsync(siteId, cancellationToken);
        
        var response = profiles.Select(p => new CropSteeringProfileResponse
        {
            Id = p.Id,
            SiteId = p.SiteId,
            StrainId = p.StrainId,
            Name = p.Name,
            TargetMode = p.TargetMode.ToString().ToLowerInvariant(),
            IsSiteDefault = p.IsSiteDefault,
            IsActive = p.IsActive,
            Configuration = p.Configuration
        }).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Get a specific crop steering profile by ID.
    /// </summary>
    [HttpGet("profiles/{profileId:guid}")]
    [ProducesResponseType(typeof(CropSteeringProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CropSteeringProfileResponse>> GetProfileById(
        Guid profileId,
        CancellationToken cancellationToken)
    {
        var profile = await _profileRepository.GetByIdAsync(profileId, cancellationToken);

        if (profile == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Profile not found",
                Detail = $"Crop steering profile with ID {profileId} was not found."
            });
        }

        return Ok(new CropSteeringProfileResponse
        {
            Id = profile.Id,
            SiteId = profile.SiteId,
            StrainId = profile.StrainId,
            Name = profile.Name,
            TargetMode = profile.TargetMode.ToString().ToLowerInvariant(),
            IsSiteDefault = profile.IsSiteDefault,
            IsActive = profile.IsActive,
            Configuration = profile.Configuration
        });
    }

    /// <summary>
    /// Create a new crop steering profile.
    /// </summary>
    [HttpPost("profiles")]
    [ProducesResponseType(typeof(CropSteeringProfileResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CropSteeringProfileResponse>> CreateProfile(
        [FromBody] CreateCropSteeringProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (request.SiteId == Guid.Empty)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid site ID",
                Detail = "A valid site ID is required."
            });
        }

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

        // TODO: Implement profile creation via repository/service
        // For now, return a placeholder response
        _logger.LogInformation(
            "Creating crop steering profile for site {SiteId}, strain {StrainId}, mode {Mode}",
            request.SiteId, request.StrainId, request.TargetMode);

        return StatusCode(StatusCodes.Status501NotImplemented, new ProblemDetails
        {
            Status = StatusCodes.Status501NotImplemented,
            Title = "Not yet implemented",
            Detail = "Profile creation endpoint is pending repository implementation."
        });
    }

    /// <summary>
    /// Update an existing crop steering profile.
    /// </summary>
    [HttpPut("profiles/{profileId:guid}")]
    [ProducesResponseType(typeof(CropSteeringProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CropSteeringProfileResponse>> UpdateProfile(
        Guid profileId,
        [FromBody] UpdateCropSteeringProfileRequest request,
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

        // TODO: Implement profile update via repository/service
        _logger.LogInformation("Updating crop steering profile {ProfileId}", profileId);

        return StatusCode(StatusCodes.Status501NotImplemented, new ProblemDetails
        {
            Status = StatusCodes.Status501NotImplemented,
            Title = "Not yet implemented",
            Detail = "Profile update endpoint is pending repository implementation."
        });
    }

    #endregion

    #region Zone Suggestion Endpoints

    /// <summary>
    /// Get crop steering suggestions for a specific zone.
    /// Evaluates current telemetry against the zone's steering profile.
    /// </summary>
    [HttpGet("zones/{zoneId:guid}/suggestions")]
    [ProducesResponseType(typeof(ZoneSteeringSuggestionsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ZoneSteeringSuggestionsResponse>> GetZoneSuggestions(
        Guid zoneId,
        [FromQuery] string? targetMode,
        CancellationToken cancellationToken)
    {
        // Determine target mode
        var mode = SteeringMode.Balanced;
        if (!string.IsNullOrEmpty(targetMode))
        {
            if (Enum.TryParse<SteeringMode>(targetMode, ignoreCase: true, out var parsedMode))
            {
                mode = parsedMode;
            }
        }

        // Get current phase
        var currentPhase = await _suggestionService.GetCurrentPhaseAsync(zoneId, cancellationToken);

        // Get effective profile
        var profile = await _suggestionService.GetEffectiveProfileAsync(zoneId, cancellationToken);

        // If profile exists, use its mode
        if (profile != null)
        {
            mode = profile.TargetMode;
        }

        // Generate suggestions
        var suggestions = await _suggestionService.EvaluateAsync(
            zoneId, 
            mode, 
            currentPhase, 
            cancellationToken);

        return Ok(new ZoneSteeringSuggestionsResponse
        {
            ZoneId = zoneId,
            CurrentPhase = FormatPhase(currentPhase),
            TargetMode = mode.ToString().ToLowerInvariant(),
            Profile = profile,
            Suggestions = suggestions.Select(s => new SteeringSuggestionResponse
            {
                Type = s.Type.ToString(),
                MetricName = s.MetricName,
                Title = s.Title,
                Description = s.Description,
                CurrentValue = s.CurrentValue,
                TargetRange = s.TargetRange,
                SuggestedAction = s.SuggestedAction,
                Priority = s.Priority.ToString().ToLowerInvariant(),
                ImpactScore = s.ImpactScore,
                Phase = FormatPhase(s.Phase),
                RelatedStreamType = s.RelatedStreamType
            }).ToList(),
            EvaluatedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get the current daily irrigation phase for a zone.
    /// </summary>
    [HttpGet("zones/{zoneId:guid}/phase")]
    [ProducesResponseType(typeof(ZonePhaseResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ZonePhaseResponse>> GetZonePhase(
        Guid zoneId,
        CancellationToken cancellationToken)
    {
        var phase = await _suggestionService.GetCurrentPhaseAsync(zoneId, cancellationToken);
        var profile = await _suggestionService.GetEffectiveProfileAsync(zoneId, cancellationToken);

        return Ok(new ZonePhaseResponse
        {
            ZoneId = zoneId,
            CurrentPhase = FormatPhase(phase),
            PhaseDescription = GetPhaseDescription(phase),
            TargetMode = profile?.TargetMode.ToString().ToLowerInvariant(),
            ProfileName = profile?.ProfileName,
            EvaluatedAt = DateTime.UtcNow
        });
    }

    #endregion

    #region Reference Data Endpoints

    /// <summary>
    /// Get default steering levers reference data.
    /// </summary>
    [HttpGet("reference/levers")]
    [ProducesResponseType(typeof(IReadOnlyList<SteeringLever>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<SteeringLever>> GetSteeringLevers()
    {
        return Ok(SteeringLever.DefaultLevers);
    }

    /// <summary>
    /// Get default irrigation signals reference data.
    /// </summary>
    [HttpGet("reference/signals")]
    [ProducesResponseType(typeof(IReadOnlyList<IrrigationSignal>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<IrrigationSignal>> GetIrrigationSignals()
    {
        return Ok(IrrigationSignal.DefaultSignals);
    }

    /// <summary>
    /// Get default dryback targets.
    /// </summary>
    [HttpGet("reference/dryback-targets")]
    [ProducesResponseType(typeof(DrybackTargets), StatusCodes.Status200OK)]
    public ActionResult<DrybackTargets> GetDrybackTargets()
    {
        return Ok(DrybackTargets.Default);
    }

    #endregion

    #region Helpers

    private Guid ResolveUserId()
    {
        // Try to get from claims first
        var userIdClaim = User.FindFirst("sub")?.Value 
            ?? User.FindFirst("user_id")?.Value;
        
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;

        // Fall back to header
        if (Request.Headers.TryGetValue("X-User-Id", out var headerValue) 
            && Guid.TryParse(headerValue.FirstOrDefault(), out var headerUserId))
        {
            return headerUserId;
        }

        return Guid.Empty;
    }

    private static string FormatPhase(DailyPhase phase) => phase switch
    {
        DailyPhase.Night => "night",
        DailyPhase.P1Ramp => "p1-ramp",
        DailyPhase.P2Maintenance => "p2-maintenance",
        DailyPhase.P3Dryback => "p3-dryback",
        _ => "unknown"
    };

    private static string GetPhaseDescription(DailyPhase phase) => phase switch
    {
        DailyPhase.Night => "Lights off period - minimal irrigation activity",
        DailyPhase.P1Ramp => "Ramp phase - saturating substrate after lights-on",
        DailyPhase.P2Maintenance => "Maintenance phase - sustaining target VWC",
        DailyPhase.P3Dryback => "Dryback phase - controlled drying before lights-off",
        _ => "Unknown phase"
    };

    #endregion
}

#region DTOs

/// <summary>
/// Response containing a crop steering profile.
/// </summary>
public sealed class CropSteeringProfileResponse
{
    public Guid Id { get; init; }
    public Guid SiteId { get; init; }
    public Guid? StrainId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string TargetMode { get; init; } = string.Empty;
    public bool IsSiteDefault { get; init; }
    public bool IsActive { get; init; }
    public SteeringConfiguration Configuration { get; init; }
}

/// <summary>
/// Request to create a new crop steering profile.
/// </summary>
public sealed class CreateCropSteeringProfileRequest
{
    public Guid SiteId { get; init; }
    public Guid? StrainId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string TargetMode { get; init; } = "balanced";
    public SteeringConfiguration? CustomConfiguration { get; init; }
}

/// <summary>
/// Request to update an existing crop steering profile.
/// </summary>
public sealed class UpdateCropSteeringProfileRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? TargetMode { get; init; }
    public SteeringConfiguration? Configuration { get; init; }
    public bool? IsActive { get; init; }
}

/// <summary>
/// Response containing zone steering suggestions.
/// </summary>
public sealed class ZoneSteeringSuggestionsResponse
{
    public Guid ZoneId { get; init; }
    public string CurrentPhase { get; init; } = string.Empty;
    public string TargetMode { get; init; } = string.Empty;
    public SteeringProfileSummary? Profile { get; init; }
    public IReadOnlyList<SteeringSuggestionResponse> Suggestions { get; init; } = Array.Empty<SteeringSuggestionResponse>();
    public DateTime EvaluatedAt { get; init; }
}

/// <summary>
/// A single steering suggestion response.
/// </summary>
public sealed class SteeringSuggestionResponse
{
    public string Type { get; init; } = string.Empty;
    public string MetricName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string CurrentValue { get; init; } = string.Empty;
    public string TargetRange { get; init; } = string.Empty;
    public string SuggestedAction { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public int ImpactScore { get; init; }
    public string Phase { get; init; } = string.Empty;
    public int? RelatedStreamType { get; init; }
}

/// <summary>
/// Response containing zone phase information.
/// </summary>
public sealed class ZonePhaseResponse
{
    public Guid ZoneId { get; init; }
    public string CurrentPhase { get; init; } = string.Empty;
    public string PhaseDescription { get; init; } = string.Empty;
    public string? TargetMode { get; init; }
    public string? ProfileName { get; init; }
    public DateTime EvaluatedAt { get; init; }
}

#endregion
