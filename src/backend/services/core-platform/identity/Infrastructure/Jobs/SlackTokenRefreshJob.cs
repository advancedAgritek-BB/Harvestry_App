using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Harvestry.Identity.Infrastructure.External;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Harvestry.Identity.Infrastructure.Jobs;

/// <summary>
/// Periodically exchanges the Slack refresh token for a fresh bot token and persists it.
/// </summary>
public sealed class SlackTokenRefreshJob : BackgroundService
{
    private static readonly TimeSpan FailureBackoff = TimeSpan.FromMinutes(15);
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SlackTokenRefreshJob> _logger;
    private readonly IOptionsMonitor<SlackCredentialsOptions> _credentialsMonitor;
    private readonly IConfiguration _configuration;

    public SlackTokenRefreshJob(
        IHttpClientFactory httpClientFactory,
        ILogger<SlackTokenRefreshJob> logger,
        IOptionsMonitor<SlackCredentialsOptions> credentialsMonitor,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _credentialsMonitor = credentialsMonitor ?? throw new ArgumentNullException(nameof(credentialsMonitor));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Slack token refresh job started");

        while (!stoppingToken.IsCancellationRequested)
        {
            TimeSpan delay;

            try
            {
                delay = await RefreshTokenAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Slack token refresh cycle failed");
                delay = FailureBackoff;
            }

            if (delay < TimeSpan.FromMinutes(5))
            {
                delay = TimeSpan.FromMinutes(5);
            }

            try
            {
                await Task.Delay(delay, stoppingToken).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Slack token refresh job stopped");
    }

    private async Task<TimeSpan> RefreshTokenAsync(CancellationToken cancellationToken)
    {
        var credentials = _credentialsMonitor.CurrentValue;

        if (string.IsNullOrWhiteSpace(credentials.RefreshToken) ||
            string.IsNullOrWhiteSpace(credentials.ClientId) ||
            string.IsNullOrWhiteSpace(credentials.ClientSecret))
        {
            _logger.LogWarning("Slack token refresh skipped because mandatory credentials are missing");
            return FailureBackoff;
        }

        var httpClient = _httpClientFactory.CreateClient("SlackOAuth");

        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = credentials.RefreshToken!,
            ["client_id"] = credentials.ClientId!,
            ["client_secret"] = credentials.ClientSecret!
        });

        using var response = await httpClient
            .PostAsync("https://slack.com/api/oauth.v2.access", form, cancellationToken)
            .ConfigureAwait(false);

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Slack refresh request failed with status {StatusCode}: {Body}",
                (int)response.StatusCode,
                body);
            return FailureBackoff;
        }

        var slackResponse = JsonSerializer.Deserialize<SlackRefreshResponse>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (slackResponse is null)
        {
            _logger.LogError("Slack refresh response could not be parsed");
            return FailureBackoff;
        }

        if (!slackResponse.Ok)
        {
            _logger.LogError("Slack refresh request returned error: {Error}", slackResponse.Error ?? "unknown_error");
            return FailureBackoff;
        }

        if (string.IsNullOrWhiteSpace(slackResponse.AccessToken))
        {
            _logger.LogError("Slack refresh response missing access_token");
            return FailureBackoff;
        }

        var nextRefreshDelay = CalculateNextDelay(slackResponse.ExpiresIn);
        var updatedRefreshToken = string.IsNullOrWhiteSpace(slackResponse.RefreshToken)
            ? credentials.RefreshToken!
            : slackResponse.RefreshToken;

        await PersistTokensAsync(slackResponse.AccessToken, updatedRefreshToken, cancellationToken).ConfigureAwait(false);
        UpdateLocalConfiguration(slackResponse.AccessToken, updatedRefreshToken);

        _logger.LogInformation(
            "Slack bot token refreshed successfully. Scheduled next refresh in {Delay}",
            nextRefreshDelay);

        return nextRefreshDelay;
    }

    private static TimeSpan CalculateNextDelay(int? expiresInSeconds)
    {
        const int defaultLifetimeSeconds = 39600; // 11 hours
        var lifetime = expiresInSeconds is > 0 ? expiresInSeconds.Value : defaultLifetimeSeconds;
        var refreshLeadSeconds = Math.Clamp(lifetime / 6, 300, 3600); // refresh 10-15% before expiry
        var delaySeconds = Math.Max(300, lifetime - refreshLeadSeconds);
        return TimeSpan.FromSeconds(delaySeconds);
    }

    private async Task PersistTokensAsync(string accessToken, string refreshToken, CancellationToken cancellationToken)
    {
        var secretName = _configuration["Slack:SecretsManager:SecretName"];
        var regionName = _configuration["Slack:SecretsManager:Region"];

        if (string.IsNullOrWhiteSpace(secretName) || string.IsNullOrWhiteSpace(regionName))
        {
            _logger.LogWarning("Skipping Slack secret update because secret name or region is not configured");
            return;
        }

        using var client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(regionName));

        var currentSecret = await client.GetSecretValueAsync(new GetSecretValueRequest
        {
            SecretId = secretName
        }, cancellationToken).ConfigureAwait(false);

        var payload = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(currentSecret.SecretString))
        {
            try
            {
                var existing = JsonSerializer.Deserialize<Dictionary<string, string>>(currentSecret.SecretString);
                if (existing is not null)
                {
                    foreach (var kvp in existing)
                    {
                        payload[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Existing Slack secret was not valid JSON; recreating payload");
            }
        }

        payload["TASKS_SLACK_BOT_TOKEN"] = accessToken;
        payload["TASKS_SLACK_REFRESH_TOKEN"] = refreshToken;

        var credentials = _credentialsMonitor.CurrentValue;
        if (!string.IsNullOrWhiteSpace(credentials.WorkspaceId))
        {
            payload["TASKS_SLACK_WORKSPACE_ID"] = credentials.WorkspaceId!;
        }

        if (!string.IsNullOrWhiteSpace(credentials.ClientId))
        {
            payload["SLACK_CLIENT_ID"] = credentials.ClientId!;
        }

        if (!string.IsNullOrWhiteSpace(credentials.ClientSecret))
        {
            payload["SLACK_CLIENT_SECRET"] = credentials.ClientSecret!;
        }

        var updatedPayload = JsonSerializer.Serialize(payload);

        await client.PutSecretValueAsync(new PutSecretValueRequest
        {
            SecretId = secretName,
            SecretString = updatedPayload
        }, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Slack secrets manager entry '{SecretName}' updated", secretName);
    }

    private void UpdateLocalConfiguration(string accessToken, string refreshToken)
    {
        _configuration["Slack:Credentials:BotToken"] = accessToken;
        _configuration["Slack:Credentials:RefreshToken"] = refreshToken;
        _configuration["TASKS_SLACK_BOT_TOKEN"] = accessToken;
        _configuration["TASKS_SLACK_REFRESH_TOKEN"] = refreshToken;
    }

    private sealed class SlackRefreshResponse
    {
        public bool Ok { get; set; }
        public string? Error { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public int? ExpiresIn { get; set; }
    }
}
