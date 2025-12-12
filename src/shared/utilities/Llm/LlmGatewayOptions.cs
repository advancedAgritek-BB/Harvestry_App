using System;

namespace Harvestry.Shared.Utilities.Llm;

/// <summary>
/// Configuration for the LLM gateway.
/// </summary>
public sealed class LlmGatewayOptions
{
    public const string SectionName = "LlmGateway";

    /// <summary>OpenAI-style API key.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Base URL for the provider.</summary>
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";

    /// <summary>Default model identifier (e.g., gpt-4o, o3-mini).</summary>
    public string Model { get; set; } = "gpt-4o";

    /// <summary>Request timeout.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Maximum completion tokens to request.</summary>
    public int MaxCompletionTokens { get; set; } = 512;

    /// <summary>Sampling temperature.</summary>
    public float Temperature { get; set; } = 0.2f;

    /// <summary>Maximum retry attempts for transient failures.</summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>Optional organization/tenant identifier for the provider.</summary>
    public string Organization { get; set; } = string.Empty;

    /// <summary>Enable basic PII redaction before sending to provider.</summary>
    public bool EnablePiiRedaction { get; set; } = true;

    /// <summary>Enable content safety scanning on responses.</summary>
    public bool EnableContentSafety { get; set; } = true;
}




