using System.Collections.Generic;

namespace Harvestry.Shared.Utilities.Llm;

/// <summary>
/// Represents an outbound chat completion request to the LLM gateway.
/// </summary>
public sealed class LlmChatRequest
{
    public string Model { get; init; } = string.Empty;
    public string? SystemPrompt { get; init; }
    public IReadOnlyCollection<LlmMessage> Messages { get; init; } = new List<LlmMessage>();
    public float? Temperature { get; init; }
    public int? MaxTokens { get; init; }
    public string? UserContext { get; init; }
    public IDictionary<string, string>? Metadata { get; init; }
}




