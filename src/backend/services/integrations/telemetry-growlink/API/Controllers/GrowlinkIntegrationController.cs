using Harvestry.Integration.Growlink.Application.DTOs;
using Harvestry.Integration.Growlink.Application.Interfaces;
using Harvestry.Integration.Growlink.Domain.Entities;
using Harvestry.Integration.Growlink.Domain.Enums;
using Harvestry.Integration.Growlink.Infrastructure.External;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Harvestry.Integration.Growlink.API.Controllers;

/// <summary>
/// API endpoints for Growlink integration management.
/// </summary>
[ApiController]
[Route("api/integrations/growlink")]
[Authorize]
public sealed class GrowlinkIntegrationController : ControllerBase
{
    private readonly IGrowlinkApiClient _apiClient;
    private readonly IGrowlinkCredentialRepository _credentialRepository;
    private readonly IGrowlinkStreamMappingRepository _mappingRepository;
    private readonly IGrowlinkStreamMapper _streamMapper;
    private readonly IGrowlinkSyncService _syncService;
    private readonly GrowlinkApiConfiguration _config;
    private readonly ILogger<GrowlinkIntegrationController> _logger;

    public GrowlinkIntegrationController(
        IGrowlinkApiClient apiClient,
        IGrowlinkCredentialRepository credentialRepository,
        IGrowlinkStreamMappingRepository mappingRepository,
        IGrowlinkStreamMapper streamMapper,
        IGrowlinkSyncService syncService,
        IOptions<GrowlinkApiConfiguration> config,
        ILogger<GrowlinkIntegrationController> logger)
    {
        _apiClient = apiClient;
        _credentialRepository = credentialRepository;
        _mappingRepository = mappingRepository;
        _streamMapper = streamMapper;
        _syncService = syncService;
        _config = config.Value;
        _logger = logger;
    }

    /// <summary>
    /// Initiates OAuth flow by returning the authorization URL.
    /// </summary>
    [HttpPost("connect")]
    [ProducesResponseType(typeof(ConnectResponse), StatusCodes.Status200OK)]
    public IActionResult Connect([FromQuery] Guid siteId)
    {
        if (siteId == Guid.Empty)
            return BadRequest("Site ID is required");

        var state = Convert.ToBase64String(siteId.ToByteArray());
        var authUrl = BuildAuthorizationUrl(state);

        _logger.LogInformation("Initiated Growlink OAuth for site {SiteId}", siteId);

        return Ok(new ConnectResponse(authUrl, state));
    }

    /// <summary>
    /// OAuth callback endpoint - exchanges authorization code for tokens.
    /// </summary>
    [HttpGet("callback")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Callback(
        [FromQuery] string code,
        [FromQuery] string state,
        [FromQuery] string? error,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogWarning("Growlink OAuth error: {Error}", error);
            return RedirectToError("OAuth authorization was denied");
        }

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
            return BadRequest("Missing code or state parameter");

        // Decode site ID from state
        Guid siteId;
        try
        {
            siteId = new Guid(Convert.FromBase64String(state));
        }
        catch
        {
            return BadRequest("Invalid state parameter");
        }

        // Exchange code for tokens
        var tokenResponse = await _apiClient.ExchangeCodeForTokensAsync(code, cancellationToken);

        if (!tokenResponse.IsSuccess || tokenResponse.Data == null)
        {
            _logger.LogError("Failed to exchange Growlink code: {Error}", tokenResponse.ErrorMessage);
            return RedirectToError("Failed to complete authorization");
        }

        // Get account info
        var accountResponse = await _apiClient.GetAccountAsync(
            tokenResponse.Data.AccessToken, cancellationToken);

        var accountId = accountResponse.Data?.AccountId ?? "unknown";

        // Store credentials
        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.Data.ExpiresIn);
        var credential = GrowlinkCredential.Create(
            siteId,
            accountId,
            tokenResponse.Data.AccessToken,
            tokenResponse.Data.RefreshToken,
            expiresAt);

        // Check for existing credential
        var existing = await _credentialRepository.GetBySiteIdAsync(siteId, cancellationToken);
        if (existing != null)
        {
            existing.UpdateTokens(
                tokenResponse.Data.AccessToken,
                tokenResponse.Data.RefreshToken,
                expiresAt);
            await _credentialRepository.UpdateAsync(existing, cancellationToken);
        }
        else
        {
            await _credentialRepository.CreateAsync(credential, cancellationToken);
        }

        _logger.LogInformation(
            "Growlink OAuth completed for site {SiteId}, account {AccountId}",
            siteId, accountId);

        // Redirect to success page
        return Redirect($"/admin/integrations?growlink=connected&site={siteId}");
    }

    /// <summary>
    /// Gets the connection status for a site.
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(GrowlinkConnectionStatusDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<GrowlinkConnectionStatusDto>> GetStatus(
        [FromQuery] Guid siteId,
        CancellationToken cancellationToken)
    {
        if (siteId == Guid.Empty)
            return BadRequest("Site ID is required");

        var credential = await _credentialRepository.GetBySiteIdAsync(siteId, cancellationToken);

        if (credential == null)
        {
            return Ok(new GrowlinkConnectionStatusDto(
                siteId,
                IsConnected: false,
                Status: "Not Connected",
                GrowlinkAccountId: null,
                LastSyncAt: null,
                LastSyncError: null,
                DeviceCount: 0,
                MappedSensorCount: 0));
        }

        var mappings = await _mappingRepository.GetBySiteIdAsync(siteId, cancellationToken);

        return Ok(new GrowlinkConnectionStatusDto(
            siteId,
            IsConnected: credential.Status == GrowlinkConnectionStatus.Connected,
            Status: credential.Status.ToString(),
            GrowlinkAccountId: credential.GrowlinkAccountId,
            LastSyncAt: credential.LastSyncAt,
            LastSyncError: credential.LastSyncError,
            DeviceCount: mappings.Select(m => m.GrowlinkDeviceId).Distinct().Count(),
            MappedSensorCount: mappings.Count(m => m.IsActive)));
    }

    /// <summary>
    /// Gets available devices from Growlink.
    /// </summary>
    [HttpGet("devices")]
    [ProducesResponseType(typeof(List<GrowlinkDeviceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<GrowlinkDeviceDto>>> GetDevices(
        [FromQuery] Guid siteId,
        CancellationToken cancellationToken)
    {
        if (siteId == Guid.Empty)
            return BadRequest("Site ID is required");

        var credential = await _credentialRepository.GetBySiteIdAsync(siteId, cancellationToken);

        if (credential == null || !credential.IsUsable())
            return NotFound("No active Growlink connection for this site");

        var response = await _apiClient.GetDevicesAsync(credential.AccessToken, cancellationToken);

        if (!response.IsSuccess)
            return StatusCode(502, response.ErrorMessage);

        return Ok(response.Data ?? new List<GrowlinkDeviceDto>());
    }

    /// <summary>
    /// Gets stream mappings for a site.
    /// </summary>
    [HttpGet("mappings")]
    [ProducesResponseType(typeof(List<StreamMappingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<StreamMappingDto>>> GetMappings(
        [FromQuery] Guid siteId,
        CancellationToken cancellationToken)
    {
        if (siteId == Guid.Empty)
            return BadRequest("Site ID is required");

        var mappings = await _mappingRepository.GetBySiteIdAsync(siteId, cancellationToken);

        var dtos = mappings.Select(m => new StreamMappingDto(
            m.Id,
            m.GrowlinkDeviceId,
            m.GrowlinkSensorId,
            m.GrowlinkSensorName,
            m.GrowlinkSensorType,
            m.HarvestryStreamId,
            m.IsActive,
            m.AutoCreated)).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Creates a stream mapping.
    /// </summary>
    [HttpPost("mappings")]
    [ProducesResponseType(typeof(StreamMappingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StreamMappingDto>> CreateMapping(
        [FromQuery] Guid siteId,
        [FromBody] CreateStreamMappingRequest request,
        CancellationToken cancellationToken)
    {
        if (siteId == Guid.Empty)
            return BadRequest("Site ID is required");

        if (string.IsNullOrWhiteSpace(request.GrowlinkDeviceId) ||
            string.IsNullOrWhiteSpace(request.GrowlinkSensorId))
        {
            return BadRequest("Device ID and Sensor ID are required");
        }

        if (!request.HarvestryStreamId.HasValue && !request.AutoCreateStream)
        {
            return BadRequest("Either HarvestryStreamId or AutoCreateStream must be specified");
        }

        // Check for existing mapping
        var existing = await _mappingRepository.GetByGrowlinkSensorAsync(
            siteId,
            request.GrowlinkDeviceId,
            request.GrowlinkSensorId,
            cancellationToken);

        if (existing != null)
        {
            return Conflict("Mapping already exists for this sensor");
        }

        Guid streamId;
        if (request.AutoCreateStream)
        {
            var created = await _streamMapper.GetHarvestryStreamIdAsync(
                siteId,
                request.GrowlinkDeviceId,
                request.GrowlinkSensorId,
                request.GrowlinkSensorId,
                "custom",
                autoCreate: true,
                cancellationToken);

            streamId = created ?? throw new InvalidOperationException("Failed to create stream");
        }
        else
        {
            streamId = request.HarvestryStreamId!.Value;
        }

        var mapping = await _streamMapper.CreateMappingAsync(
            siteId,
            request.GrowlinkDeviceId,
            request.GrowlinkSensorId,
            request.GrowlinkSensorId,
            "custom",
            streamId,
            cancellationToken);

        var dto = new StreamMappingDto(
            mapping.Id,
            mapping.GrowlinkDeviceId,
            mapping.GrowlinkSensorId,
            mapping.GrowlinkSensorName,
            mapping.GrowlinkSensorType,
            mapping.HarvestryStreamId,
            mapping.IsActive,
            mapping.AutoCreated);

        return CreatedAtAction(nameof(GetMappings), new { siteId }, dto);
    }

    /// <summary>
    /// Deletes a stream mapping.
    /// </summary>
    [HttpDelete("mappings/{mappingId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMapping(
        Guid mappingId,
        CancellationToken cancellationToken)
    {
        var mapping = await _mappingRepository.GetByIdAsync(mappingId, cancellationToken);
        if (mapping == null)
            return NotFound();

        await _mappingRepository.DeleteAsync(mappingId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Triggers a manual sync for a site.
    /// </summary>
    [HttpPost("sync")]
    [ProducesResponseType(typeof(GrowlinkSyncResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GrowlinkSyncResultDto>> TriggerSync(
        [FromQuery] Guid siteId,
        CancellationToken cancellationToken)
    {
        if (siteId == Guid.Empty)
            return BadRequest("Site ID is required");

        var credential = await _credentialRepository.GetBySiteIdAsync(siteId, cancellationToken);
        if (credential == null)
            return NotFound("No Growlink connection for this site");

        var result = await _syncService.SyncLatestReadingsAsync(siteId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Disconnects Growlink integration for a site.
    /// </summary>
    [HttpDelete("disconnect")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Disconnect(
        [FromQuery] Guid siteId,
        CancellationToken cancellationToken)
    {
        if (siteId == Guid.Empty)
            return BadRequest("Site ID is required");

        var credential = await _credentialRepository.GetBySiteIdAsync(siteId, cancellationToken);
        if (credential == null)
            return NotFound("No Growlink connection for this site");

        // Revoke token
        if (!string.IsNullOrWhiteSpace(credential.AccessToken))
        {
            await _apiClient.RevokeTokenAsync(credential.AccessToken, cancellationToken);
        }

        // Delete credential
        await _credentialRepository.DeleteAsync(credential.Id, cancellationToken);

        // Optionally delete mappings
        await _mappingRepository.DeleteBySiteIdAsync(siteId, cancellationToken);

        _logger.LogInformation("Disconnected Growlink integration for site {SiteId}", siteId);

        return NoContent();
    }

    private string BuildAuthorizationUrl(string state)
    {
        var scope = "read:devices read:sensors";
        return $"{_config.AuthorizationEndpoint}?" +
               $"client_id={Uri.EscapeDataString(_config.ClientId)}&" +
               $"redirect_uri={Uri.EscapeDataString(_config.RedirectUri)}&" +
               $"response_type=code&" +
               $"scope={Uri.EscapeDataString(scope)}&" +
               $"state={Uri.EscapeDataString(state)}";
    }

    private IActionResult RedirectToError(string message)
    {
        return Redirect($"/admin/integrations?growlink=error&message={Uri.EscapeDataString(message)}");
    }
}

/// <summary>
/// Response for connect endpoint.
/// </summary>
public sealed record ConnectResponse(string AuthorizationUrl, string State);




