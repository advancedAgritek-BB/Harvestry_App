using System.Text.Json;
using System.Text.Json.Serialization;

namespace Harvestry.Tasks.Application.DTOs;

public sealed class SlackWebhookRequest
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("token")]
    public string? Token { get; init; }

    [JsonPropertyName("challenge")]
    public string? Challenge { get; init; }

    [JsonPropertyName("event")]
    public JsonElement Event { get; init; }
}

public abstract record SlackWebhookResult;
public sealed record SlackWebhookChallenge(string Challenge) : SlackWebhookResult;
public sealed record SlackWebhookAck : SlackWebhookResult;
public sealed record SlackWebhookError(string Message) : SlackWebhookResult;
