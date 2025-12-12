using Harvestry.Compliance.Metrc.Application.DTOs;
using Harvestry.Compliance.Metrc.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Harvestry.Compliance.Metrc.API.Controllers;

/// <summary>
/// API controller for managing METRC license configurations
/// </summary>
[ApiController]
[Route("api/v1/metrc/licenses")]
[Authorize]
public sealed class MetrcLicensesController : ControllerBase
{
    private readonly IMetrcLicenseService _licenseService;
    private readonly ILogger<MetrcLicensesController> _logger;

    public MetrcLicensesController(
        IMetrcLicenseService licenseService,
        ILogger<MetrcLicensesController> logger)
    {
        _licenseService = licenseService ?? throw new ArgumentNullException(nameof(licenseService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all licenses for a site
    /// </summary>
    [HttpGet("site/{siteId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<LicenseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LicenseDto>>> GetLicensesForSite(
        Guid siteId,
        CancellationToken cancellationToken)
    {
        var licenses = await _licenseService.GetLicensesForSiteAsync(siteId, cancellationToken);
        return Ok(licenses);
    }

    /// <summary>
    /// Gets a license by ID
    /// </summary>
    [HttpGet("{licenseId:guid}")]
    [ProducesResponseType(typeof(LicenseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LicenseDto>> GetLicense(
        Guid licenseId,
        CancellationToken cancellationToken)
    {
        var license = await _licenseService.GetLicenseAsync(licenseId, cancellationToken);
        if (license == null)
        {
            return NotFound();
        }
        return Ok(license);
    }

    /// <summary>
    /// Gets a license by license number
    /// </summary>
    [HttpGet("by-number/{licenseNumber}")]
    [ProducesResponseType(typeof(LicenseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LicenseDto>> GetLicenseByNumber(
        string licenseNumber,
        CancellationToken cancellationToken)
    {
        var license = await _licenseService.GetLicenseByNumberAsync(licenseNumber, cancellationToken);
        if (license == null)
        {
            return NotFound();
        }
        return Ok(license);
    }

    /// <summary>
    /// Creates or updates a license configuration
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(LicenseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LicenseDto>> UpsertLicense(
        [FromBody] UpsertLicenseRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var license = await _licenseService.UpsertLicenseAsync(request, userId, cancellationToken);
        return Ok(license);
    }

    /// <summary>
    /// Sets credentials for a license
    /// </summary>
    [HttpPost("{licenseId:guid}/credentials")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetCredentials(
        Guid licenseId,
        [FromBody] SetCredentialsRequest request,
        CancellationToken cancellationToken)
    {
        if (licenseId != request.LicenseId)
        {
            return BadRequest("License ID mismatch");
        }

        var userId = GetCurrentUserId();
        var success = await _licenseService.SetCredentialsAsync(request, userId, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        _logger.LogInformation("Credentials updated for license {LicenseId} by user {UserId}",
            licenseId, userId);

        return Ok(new { message = "Credentials updated successfully" });
    }

    /// <summary>
    /// Tests connection to METRC with the configured credentials
    /// </summary>
    [HttpPost("{licenseId:guid}/test-connection")]
    [ProducesResponseType(typeof(ConnectionTestResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConnectionTestResult>> TestConnection(
        Guid licenseId,
        CancellationToken cancellationToken)
    {
        var (success, message) = await _licenseService.TestConnectionAsync(licenseId, cancellationToken);

        return Ok(new ConnectionTestResult
        {
            Success = success,
            Message = message,
            TestedAt = DateTimeOffset.UtcNow
        });
    }

    /// <summary>
    /// Activates a license
    /// </summary>
    [HttpPost("{licenseId:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ActivateLicense(
        Guid licenseId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var success = await _licenseService.ActivateLicenseAsync(licenseId, userId, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return Ok(new { message = "License activated" });
    }

    /// <summary>
    /// Deactivates a license
    /// </summary>
    [HttpPost("{licenseId:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeactivateLicense(
        Guid licenseId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var success = await _licenseService.DeactivateLicenseAsync(licenseId, userId, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return Ok(new { message = "License deactivated" });
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst("sub") ?? User.FindFirst("user_id");
        return claim != null && Guid.TryParse(claim.Value, out var userId) ? userId : null;
    }
}

/// <summary>
/// Result of a METRC connection test
/// </summary>
public sealed record ConnectionTestResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTimeOffset TestedAt { get; init; }
}
