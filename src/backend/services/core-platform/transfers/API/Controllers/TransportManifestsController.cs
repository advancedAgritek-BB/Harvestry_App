using Harvestry.Transfers.Application.DTOs;
using Harvestry.Transfers.Application.Interfaces;
using Harvestry.Shared.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Transfers.API.Controllers;

[ApiController]
[Route("api/v1/sites/{siteId:guid}/transfers/outbound/{outboundTransferId:guid}/manifest")]
[Authorize]
public sealed class TransportManifestsController : ControllerBase
{
    private readonly ITransportManifestService _manifestService;

    public TransportManifestsController(ITransportManifestService manifestService)
    {
        _manifestService = manifestService;
    }

    [HttpGet]
    public async Task<ActionResult<TransportManifestDto>> Get(
        [FromRoute] Guid siteId,
        [FromRoute] Guid outboundTransferId,
        CancellationToken cancellationToken = default)
    {
        var result = await _manifestService.GetByTransferIdAsync(siteId, outboundTransferId, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult<TransportManifestDto>> Upsert(
        [FromRoute] Guid siteId,
        [FromRoute] Guid outboundTransferId,
        [FromBody] UpsertTransportManifestRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _manifestService.CreateOrUpdateAsync(siteId, outboundTransferId, request, userId.Value, cancellationToken);
        return Ok(result);
    }
}

