using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harvestry.Identity.API.Controllers;

[ApiController]
[Route("api/v1/slack")]
[AllowAnonymous]
public sealed class SlackOAuthController : ControllerBase
{
    private readonly ILogger<SlackOAuthController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public SlackOAuthController(
        ILogger<SlackOAuthController> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpGet("oauth/callback")]
    public async Task<IActionResult> OAuthCallback(
        [FromQuery] string code,
        [FromQuery] string? state,
        [FromQuery(Name = "error")] string? error)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            _logger.LogWarning("Slack OAuth returned error: {Error}", error);
            return Content("Slack authorization was cancelled or failed. You can close this window.");
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            _logger.LogWarning("Slack OAuth callback invoked without a code parameter.");
            return Content("Missing authorization code. Please retry installing the Slack app.");
        }

        var slackClientId = ResolveSetting("Slack:OAuth:ClientId", "SLACK_CLIENT_ID");
        var slackClientSecret = ResolveSetting("Slack:OAuth:ClientSecret", "SLACK_CLIENT_SECRET");

        if (string.IsNullOrWhiteSpace(slackClientId) || string.IsNullOrWhiteSpace(slackClientSecret))
        {
            _logger.LogError("Slack OAuth configuration missing. Ensure client ID/secret environment variables are set.");
            return Content("Slack authorization received, but server configuration is missing. Please set SLACK_CLIENT_ID and SLACK_CLIENT_SECRET, then retry.");
        }

        var httpClient = _httpClientFactory.CreateClient("SlackOAuth");
        var formValues = new Dictionary<string, string>
        {
            ["client_id"] = slackClientId,
            ["client_secret"] = slackClientSecret,
            ["code"] = code
        };


        using var requestContent = new FormUrlEncodedContent(formValues);

        // Use relative path to honor the configured BaseAddress on the named HttpClient
        using var response = await httpClient.PostAsync("api/oauth.v2.access", requestContent, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            _logger.LogError("Slack OAuth token exchange failed with status {StatusCode}: {Body}", response.StatusCode, errorBody);
            return Content($"Slack authorization failed at token exchange. HTTP {(int)response.StatusCode}. See server logs for details.");
        }

        var payloadStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        var slackResponse = await JsonSerializer.DeserializeAsync<SlackOAuthAccessResponse>(payloadStream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }).ConfigureAwait(false);

        if (slackResponse is null)
        {
            _logger.LogError("Slack OAuth token exchange returned an empty payload.");
            return Content("Slack authorization failed: empty response payload.");
        }

        if (!slackResponse.Ok)
        {
            _logger.LogError("Slack OAuth token exchange returned error: {Error}", slackResponse.Error);
            return Content($"Slack authorization failed: {WebUtility.HtmlEncode(slackResponse.Error ?? "unknown_error")}");
        }

        _logger.LogInformation("Slack OAuth succeeded for team {TeamId} / {TeamName}", slackResponse.Team?.Id, slackResponse.Team?.Name);

        // Set security headers to prevent caching of sensitive token data
        Response.Headers.Append("Cache-Control", "no-store, no-cache, must-revalidate, max-age=0");
        Response.Headers.Append("Pragma", "no-cache");
        Response.Headers.Append("Expires", "0");

        var builder = new StringBuilder();
        builder.Append("<html><head><title>Slack Install Complete</title>");
        builder.Append("<script>");
        builder.Append("function showToken(id) { document.getElementById(id).style.display='inline'; document.getElementById(id+'-btn').style.display='none'; }");
        builder.Append("function copyToken(id) { ");
        builder.Append("  var token = document.getElementById(id).innerText; ");
        builder.Append("  navigator.clipboard.writeText(token).then(() => { alert('Copied to clipboard'); document.getElementById(id).innerText='[REDACTED]'; });");
        builder.Append("}");
        builder.Append("setTimeout(() => { document.querySelectorAll('.token').forEach(el => el.innerText='[EXPIRED]'); }, 300000);"); // 5 min auto-clear
        builder.Append("</script></head><body>");
        builder.Append("<h1>⚠️ Slack Install Complete - SECURITY WARNING</h1>");
        builder.Append("<p style='color:red;font-weight:bold;'>CRITICAL: This page contains sensitive credentials. Copy them immediately to your secrets manager and close this window. DO NOT share this URL or screenshot this page.</p>");

        if (slackResponse.Team is not null)
        {
            builder.AppendFormat("<p><strong>Team:</strong> {0} (ID: {1})</p>",
                WebUtility.HtmlEncode(slackResponse.Team.Name),
                WebUtility.HtmlEncode(slackResponse.Team.Id));
        }

        builder.Append("<p><strong>Bot Token:</strong> ");
        builder.AppendFormat("<button id='bot-token-btn' onclick='showToken(\"bot-token\")'>Show Token</button> ");
        builder.AppendFormat("<code id='bot-token' class='token' style='display:none'>{0}</code> ", WebUtility.HtmlEncode(slackResponse.AccessToken));
        builder.Append("<button onclick='copyToken(\"bot-token\")'>Copy & Hide</button></p>");

        if (slackResponse.Scope is not null)
        {
            builder.AppendFormat("<p><strong>Scopes:</strong> {0}</p>", WebUtility.HtmlEncode(slackResponse.Scope));
        }

        if (slackResponse.BotUserId is not null)
        {
            builder.AppendFormat("<p><strong>Bot User ID:</strong> {0}</p>", WebUtility.HtmlEncode(slackResponse.BotUserId));
        }

        if (slackResponse.ExpiresIn is not null)
        {
            builder.AppendFormat("<p><strong>Expires In:</strong> {0} seconds</p>", slackResponse.ExpiresIn);
        }

        if (slackResponse.RefreshToken is not null)
        {
            builder.Append("<p><strong>Refresh Token:</strong> ");
            builder.AppendFormat("<button id='refresh-token-btn' onclick='showToken(\"refresh-token\")'>Show Token</button> ");
            builder.AppendFormat("<code id='refresh-token' class='token' style='display:none'>{0}</code> ", WebUtility.HtmlEncode(slackResponse.RefreshToken));
            builder.Append("<button onclick='copyToken(\"refresh-token\")'>Copy & Hide</button></p>");
        }

        if (slackResponse.AuthedUser?.AccessToken is not null)
        {
            builder.Append("<p><strong>User Token:</strong> ");
            builder.AppendFormat("<button id='user-token-btn' onclick='showToken(\"user-token\")'>Show Token</button> ");
            builder.AppendFormat("<code id='user-token' class='token' style='display:none'>{0}</code> ", WebUtility.HtmlEncode(slackResponse.AuthedUser.AccessToken));
            builder.Append("<button onclick='copyToken(\"user-token\")'>Copy & Hide</button></p>");
        }

        builder.Append("<p>After copying, set the following environment variables (or configuration entries):</p>");
        builder.Append("<ul>");
        builder.Append("<li><code>TASKS_SLACK_BOT_TOKEN</code></li>");
        builder.Append("<li><code>TASKS_SLACK_WORKSPACE_ID</code> (from Slack workspace settings)</li>");
        builder.Append("<li><code>TASKS_SLACK_REFRESH_TOKEN</code></li>");
        builder.Append("<li><code>SLACK_CLIENT_ID</code>, <code>SLACK_CLIENT_SECRET</code> (for future reinstalls)</li>");
        builder.Append("</ul>");
        builder.Append("<p style='color:red;'>Tokens on this page will auto-expire in 5 minutes. Close this window after saving.</p>");
        builder.Append("</body></html>");

        return Content(builder.ToString(), "text/html");
    }

    private string? ResolveSetting(string configurationKey, string environmentVariable)
    {
        var value = _configuration[configurationKey];
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        value = Environment.GetEnvironmentVariable(environmentVariable);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private sealed class SlackOAuthAccessResponse
    {
        public bool Ok { get; set; }
        public string? Error { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public int? ExpiresIn { get; set; }
        public string? Scope { get; set; }
        public string? BotUserId { get; set; }
        public SlackTeam? Team { get; set; }
        public SlackAuthedUser? AuthedUser { get; set; }
    }

    private sealed class SlackTeam
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
    }

    private sealed class SlackAuthedUser
    {
        public string? Id { get; set; }
        public string? Scope { get; set; }
        public string? AccessToken { get; set; }
        public string? TokenType { get; set; }
    }
}
