namespace Harvestry.Shared.Utilities.Llm;

/// <summary>
/// Represents a normalized chat completion response from the LLM gateway.
/// </summary>
public sealed class LlmChatResponse
{
    public string Content { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string? FinishReason { get; init; }
    public int? InputTokens { get; init; }
    public int? OutputTokens { get; init; }
    public bool RedactionApplied { get; init; }
    public bool ContentFlagged { get; init; }
}




