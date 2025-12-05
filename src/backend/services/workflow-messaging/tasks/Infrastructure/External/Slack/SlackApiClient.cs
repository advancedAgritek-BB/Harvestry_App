using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace Harvestry.Tasks.Infrastructure.External.Slack;

public sealed class SlackApiClient : ISlackApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SlackApiClient> _logger;

    public SlackApiClient(HttpClient httpClient, ILogger<SlackApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (_httpClient.BaseAddress is null)
        {
            _httpClient.BaseAddress = new Uri("https://slack.com/api/");
        }
    }

    public async Task<SlackMessageResponse> SendMessageAsync(string botToken, string channelId, string payloadJson, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(botToken))
        {
            throw new ArgumentException("Slack bot token is required.", nameof(botToken));
        }

        if (string.IsNullOrWhiteSpace(channelId))
        {
            throw new ArgumentException("Slack channel identifier is required.", nameof(channelId));
        }

        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            throw new ArgumentException("Slack payload JSON is required.", nameof(payloadJson));
        }

        using var payloadDocument = JsonDocument.Parse(payloadJson);
        var body = new Dictionary<string, object?>
        {
            ["channel"] = channelId
        };

        foreach (var property in payloadDocument.RootElement.EnumerateObject())
        {
            body[property.Name] = property.Value.ValueKind switch
            {
                JsonValueKind.Object => JsonSerializer.Deserialize<Dictionary<string, object?>>(property.Value.GetRawText()),
                JsonValueKind.Array => JsonSerializer.Deserialize<List<object?>>(property.Value.GetRawText()),
                JsonValueKind.String => property.Value.GetString(),
                JsonValueKind.Number => property.Value.TryGetInt64(out var longValue) ? longValue : property.Value.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => property.Value.GetRawText()
            };
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "chat.postMessage")
        {
            Content = JsonContent.Create(body)
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", botToken);

        var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Slack API request failed with status {Status}: {Body}", response.StatusCode, responseText);
            response.EnsureSuccessStatusCode();
        }

        string? responseChannel = null;
        string? responseTs = null;
        string? responseThreadTs = null;

        try
        {
            using var responseDocument = JsonDocument.Parse(responseText);
            if (responseDocument.RootElement.TryGetProperty("ok", out var okProperty) && okProperty.ValueKind == JsonValueKind.False)
            {
                var error = responseDocument.RootElement.TryGetProperty("error", out var errorProperty)
                    ? errorProperty.GetString() ?? "Unknown Slack error"
                    : "Unknown Slack error";
                throw new InvalidOperationException($"Slack API returned error: {error}");
            }

            responseChannel = responseDocument.RootElement.TryGetProperty("channel", out var channelProperty)
                ? channelProperty.GetString()
                : channelId;

            responseTs = responseDocument.RootElement.TryGetProperty("ts", out var tsProperty)
                ? tsProperty.GetString()
                : null;

            if (responseDocument.RootElement.TryGetProperty("message", out var messageProperty)
                && messageProperty.ValueKind == JsonValueKind.Object
                && messageProperty.TryGetProperty("thread_ts", out var threadProperty))
            {
                responseThreadTs = threadProperty.GetString();
            }

            if (string.IsNullOrWhiteSpace(responseThreadTs)
                && responseDocument.RootElement.TryGetProperty("thread_ts", out var threadTsProperty))
            {
                responseThreadTs = threadTsProperty.GetString();
            }
        }
        catch (JsonException jsonEx)
        {
            _logger.LogWarning(jsonEx, "Unable to parse Slack API response. Raw body: {Body}", responseText);
        }

        responseTs ??= string.Empty;
        responseChannel ??= channelId;

        return new SlackMessageResponse(
            responseChannel,
            responseTs,
            responseThreadTs,
            DateTimeOffset.UtcNow);
    }
}
