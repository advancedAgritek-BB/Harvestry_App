using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Tasks.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/slack/events")]
public sealed class SlackWebhookController : ControllerBase
{
    private readonly ISlackWebhookHandler _webhookHandler;

    public SlackWebhookController(ISlackWebhookHandler webhookHandler)
    {
        _webhookHandler = webhookHandler ?? throw new ArgumentNullException(nameof(webhookHandler));
    }

    [HttpPost]
    public async Task<IActionResult> Receive([FromBody] SlackWebhookRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var response = await _webhookHandler.HandleAsync(request, cancellationToken).ConfigureAwait(false);

        if (response is SlackWebhookChallenge challenge)
        {
            return Ok(new { challenge = challenge.Challenge });
        }

        if (response is SlackWebhookError error)
        {
            return BadRequest(new { error = error.Message });
        }

        return Ok(new { ok = true });
    }
}
