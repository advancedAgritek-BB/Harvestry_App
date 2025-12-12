using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Harvestry.Shared.Utilities.Llm;

/// <summary>
/// Lightweight harness to verify prompt templates render correctly and optionally sanity-check output using the LLM gateway.
/// </summary>
public sealed class PromptEvaluationHarness
{
    private readonly ILlmGateway _gateway;

    public PromptEvaluationHarness(ILlmGateway gateway)
    {
        _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
    }

    public PromptEvaluationResult ValidateTemplate(PromptTemplate template, IDictionary<string, string> variables)
    {
        try
        {
            template.Render(variables);
            return PromptEvaluationResult.Success();
        }
        catch (Exception ex)
        {
            return PromptEvaluationResult.Failure(new[] { ex.Message });
        }
    }

    /// <summary>
    /// Executes a prompt against the gateway for smoke-testing with expected phrases.
    /// </summary>
    public async Task<PromptEvaluationResult> ExecuteAsync(
        PromptTemplate template,
        IDictionary<string, string> variables,
        IEnumerable<string> expectedPhrases,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateTemplate(template, variables);
        if (!validation.IsSuccessful)
        {
            return validation;
        }

        var prompt = template.Render(variables);
        var response = await _gateway.CompleteChatAsync(new LlmChatRequest
        {
            Model = string.Empty,
            SystemPrompt = prompt,
            Messages = Array.Empty<LlmMessage>()
        }, cancellationToken).ConfigureAwait(false);

        var missing = expectedPhrases.Where(p => !response.Content.Contains(p, StringComparison.OrdinalIgnoreCase)).ToArray();
        return missing.Length == 0
            ? PromptEvaluationResult.Success()
            : PromptEvaluationResult.Failure(missing.Select(m => $"Missing expected phrase: {m}"));
    }
}

public sealed class PromptEvaluationResult
{
    private PromptEvaluationResult(bool isSuccessful, IReadOnlyCollection<string> errors)
    {
        IsSuccessful = isSuccessful;
        Errors = errors;
    }

    public bool IsSuccessful { get; }

    public IReadOnlyCollection<string> Errors { get; }

    public static PromptEvaluationResult Success() => new(true, Array.Empty<string>());

    public static PromptEvaluationResult Failure(IEnumerable<string> errors) =>
        new(false, errors.ToArray());
}




