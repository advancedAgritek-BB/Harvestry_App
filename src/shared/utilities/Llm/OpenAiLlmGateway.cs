using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Harvestry.Shared.Utilities.Llm;

/// <summary>
/// OpenAI-compatible implementation of the LLM gateway with basic safety guardrails.
/// </summary>
public sealed class OpenAiLlmGateway : ILlmGateway
{
    private const string ClientName = "openai-llm";
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IHttpClientFactory _clientFactory;
    private readonly LlmGatewayOptions _options;
    private readonly SensitiveDataRedactor _redactor;
    private readonly ContentSafetyEvaluator _safetyEvaluator;
    private readonly ILogger<OpenAiLlmGateway> _logger;

    public OpenAiLlmGateway(
        IHttpClientFactory clientFactory,
        IOptions<LlmGatewayOptions> options,
        SensitiveDataRedactor redactor,
        ContentSafetyEvaluator safetyEvaluator,
        ILogger<OpenAiLlmGateway> logger)
    {
        _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _redactor = redactor ?? throw new ArgumentNullException(nameof(redactor));
        _safetyEvaluator = safetyEvaluator ?? throw new ArgumentNullException(nameof(safetyEvaluator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LlmChatResponse> CompleteChatAsync(LlmChatRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.Messages.Count == 0 && string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            throw new ArgumentException("At least one message or a system prompt is required.", nameof(request));
        }

        var model = string.IsNullOrWhiteSpace(request.Model) ? _options.Model : request.Model;
        var temperature = request.Temperature ?? _options.Temperature;
        var maxTokens = request.MaxTokens ?? _options.MaxCompletionTokens;

        var assembledMessages = BuildMessages(request);
        var (payload, redactionApplied) = BuildPayload(model, temperature, maxTokens, request.UserContext, assembledMessages);

        using var httpClient = _clientFactory.CreateClient(ClientName);

        // Ensure auth header for each request in case handler has been reused.
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        if (!string.IsNullOrWhiteSpace(_options.Organization))
        {
            httpClient.DefaultRequestHeaders.Remove("OpenAI-Organization");
            httpClient.DefaultRequestHeaders.Add("OpenAI-Organization", _options.Organization);
        }

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, SerializerOptions), Encoding.UTF8, "application/json")
        };

        var response = await httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("OpenAI call failed with status {Status}: {Body}", response.StatusCode, body);
            throw new InvalidOperationException($"LLM gateway call failed with status {response.StatusCode}");
        }

        var parsed = JsonSerializer.Deserialize<OpenAiChatCompletionResponse>(body, SerializerOptions)
                     ?? throw new InvalidOperationException("LLM gateway returned an empty response.");

        var choice = parsed.Choices?.FirstOrDefault();
        if (choice?.Message?.Content is null)
        {
            throw new InvalidOperationException("LLM gateway returned no content.");
        }

        var safetyResult = _options.EnableContentSafety
            ? _safetyEvaluator.Evaluate(choice.Message.Content)
            : ContentSafetyResult.Safe();

        if (safetyResult.IsFlagged)
        {
            _logger.LogWarning("LLM content flagged: {Reasons}", string.Join("; ", safetyResult.Reasons));
        }

        return new LlmChatResponse
        {
            Content = choice.Message.Content,
            FinishReason = choice.FinishReason,
            Model = parsed.Model ?? model,
            InputTokens = parsed.Usage?.PromptTokens,
            OutputTokens = parsed.Usage?.CompletionTokens,
            RedactionApplied = redactionApplied,
            ContentFlagged = safetyResult.IsFlagged
        };
    }

    private (OpenAiChatCompletionRequest Payload, bool RedactionApplied) BuildPayload(
        string model,
        float temperature,
        int maxTokens,
        string? userContext,
        IReadOnlyCollection<LlmMessage> messages)
    {
        var redactionApplied = false;
        var normalizedMessages = new List<OpenAiMessage>();

        foreach (var message in messages)
        {
            var content = message.Content;
            if (_options.EnablePiiRedaction)
            {
                var redacted = _redactor.Redact(message.Content);
                content = redacted.Value;
                redactionApplied = redactionApplied || redacted.WasRedacted;
            }

            normalizedMessages.Add(new OpenAiMessage(message.Role, content));
        }

        return (new OpenAiChatCompletionRequest
        {
            Model = model,
            Messages = normalizedMessages,
            Temperature = temperature,
            MaxTokens = maxTokens,
            User = userContext
        }, redactionApplied);
    }

    private IReadOnlyCollection<LlmMessage> BuildMessages(LlmChatRequest request)
    {
        var messages = new List<LlmMessage>();

        if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            messages.Add(LlmMessage.System(request.SystemPrompt));
        }

        messages.AddRange(request.Messages);

        return messages;
    }

    private sealed class OpenAiChatCompletionRequest
    {
        public string Model { get; init; } = string.Empty;
        public IReadOnlyCollection<OpenAiMessage> Messages { get; init; } = Array.Empty<OpenAiMessage>();
        public float Temperature { get; init; }
        public int MaxTokens { get; init; }
        public string? User { get; init; }
    }

    private sealed class OpenAiMessage
    {
        public OpenAiMessage(string role, string content)
        {
            Role = role;
            Content = content;
        }

        public string Role { get; }
        public string Content { get; }
    }

    private sealed class OpenAiChatCompletionResponse
    {
        public string? Model { get; init; }
        public IReadOnlyCollection<OpenAiChoice>? Choices { get; init; }
        public OpenAiUsage? Usage { get; init; }
    }

    private sealed class OpenAiChoice
    {
        public OpenAiMessage? Message { get; init; }
        public string? FinishReason { get; init; }
    }

    private sealed class OpenAiUsage
    {
        public int? PromptTokens { get; init; }
        public int? CompletionTokens { get; init; }
    }
}




