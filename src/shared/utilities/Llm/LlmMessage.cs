namespace Harvestry.Shared.Utilities.Llm;

/// <summary>
/// A single message in a chat exchange.
/// </summary>
public sealed record LlmMessage(string Role, string Content)
{
    public static LlmMessage System(string content) => new("system", content);
    public static LlmMessage User(string content) => new("user", content);
    public static LlmMessage Assistant(string content) => new("assistant", content);
}




