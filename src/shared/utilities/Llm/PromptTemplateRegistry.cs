using System;
using System.Collections.Generic;
using System.Linq;

namespace Harvestry.Shared.Utilities.Llm;

/// <summary>
/// Simple in-memory registry for prompt templates.
/// </summary>
public sealed class PromptTemplateRegistry
{
    private readonly Dictionary<string, PromptTemplate> _templates = new();

    public void Register(PromptTemplate template)
    {
        if (template is null)
        {
            throw new ArgumentNullException(nameof(template));
        }

        var key = GetKey(template.Id, template.Version);
        _templates[key] = template;
    }

    public PromptTemplate Get(string id, string version)
    {
        var key = GetKey(id, version);
        if (_templates.TryGetValue(key, out var template))
        {
            return template;
        }

        throw new KeyNotFoundException($"Prompt template {id} v{version} not found.");
    }

    public IReadOnlyCollection<PromptTemplate> List(string id)
    {
        return _templates.Values.Where(t => t.Id.Equals(id, StringComparison.OrdinalIgnoreCase)).ToArray();
    }

    private static string GetKey(string id, string version) => $"{id}:{version}".ToLowerInvariant();
}




