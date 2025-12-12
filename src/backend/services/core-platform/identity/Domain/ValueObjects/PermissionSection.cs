using System;
using System.Collections.Generic;
using System.Linq;

namespace Harvestry.Identity.Domain.ValueObjects;

/// <summary>
/// Represents a logical grouping of permissions within a specific functional area.
/// Sections enable bundled permission management in the UI.
/// </summary>
public sealed class PermissionSection
{
    private readonly List<PermissionDefinition> _permissions;

    public PermissionSection(
        string id,
        string label,
        string? description,
        int displayOrder,
        IEnumerable<PermissionDefinition> permissions)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Section ID is required", nameof(id));

        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Section label is required", nameof(label));

        Id = id;
        Label = label;
        Description = description;
        DisplayOrder = displayOrder;
        _permissions = permissions?.ToList() ?? new List<PermissionDefinition>();
    }

    /// <summary>
    /// Unique identifier for this section (e.g., "cultivation", "inventory")
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Human-readable label for display (e.g., "Cultivation & Environment")
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Optional description of what this section covers
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Order in which to display this section in the UI
    /// </summary>
    public int DisplayOrder { get; }

    /// <summary>
    /// All permissions within this section
    /// </summary>
    public IReadOnlyList<PermissionDefinition> Permissions => _permissions.AsReadOnly();

    /// <summary>
    /// Get all permission keys in this section
    /// </summary>
    public IEnumerable<string> GetPermissionKeys() => _permissions.Select(p => p.Key);

    /// <summary>
    /// Check if this section contains a specific permission key
    /// </summary>
    public bool ContainsPermission(string permissionKey) =>
        _permissions.Any(p => p.Key.Equals(permissionKey, StringComparison.OrdinalIgnoreCase));
}

/// <summary>
/// Represents a single permission definition within a section
/// </summary>
public sealed class PermissionDefinition
{
    public PermissionDefinition(
        string key,
        string label,
        string? description = null,
        bool requiresTwoPersonApproval = false,
        bool requiresReason = false)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Permission key is required", nameof(key));

        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Permission label is required", nameof(label));

        Key = key;
        Label = label;
        Description = description;
        RequiresTwoPersonApproval = requiresTwoPersonApproval;
        RequiresReason = requiresReason;
    }

    /// <summary>
    /// Unique permission key in resource:action format (e.g., "cultivation:view")
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Human-readable label for display
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Optional description of what this permission grants
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Whether actions using this permission require two-person approval
    /// </summary>
    public bool RequiresTwoPersonApproval { get; }

    /// <summary>
    /// Whether actions using this permission require a reason to be provided
    /// </summary>
    public bool RequiresReason { get; }
}
