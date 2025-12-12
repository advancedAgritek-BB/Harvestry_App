using System;
using System.Collections.Generic;
using System.Linq;

namespace Harvestry.Shared.Utilities.Llm;

/// <summary>
/// Represents a versioned prompt template with required variables.
/// </summary>
public sealed class PromptTemplate
{
    public PromptTemplate(string id, string version, string content, IReadOnlyCollection<string> requiredVariables)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Template id is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(version))
        {
            throw new ArgumentException("Template version is required.", nameof(version));
        }

        Id = id;
        Version = version;
        Content = content ?? throw new ArgumentNullException(nameof(content));
        RequiredVariables = requiredVariables ?? Array.Empty<string>();
    }

    public string Id { get; }

    public string Version { get; }

    public string Content { get; }

    public IReadOnlyCollection<string> RequiredVariables { get; }

    public string Render(IDictionary<string, string> variables)
    {
        var missing = RequiredVariables.Where(k => !variables.ContainsKey(k) || string.IsNullOrWhiteSpace(variables[k])).ToList();
        if (missing.Count > 0)
        {
            throw new ArgumentException($"Missing required variables: {string.Join(", ", missing)}", nameof(variables));
        }

        var output = Content;
        foreach (var kvp in variables)
        {
            output = output.Replace($"{{{{{kvp.Key}}}}}", kvp.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        return output;
    }
}




