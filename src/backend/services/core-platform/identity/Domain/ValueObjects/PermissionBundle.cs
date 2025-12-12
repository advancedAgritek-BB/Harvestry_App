using System;
using System.Collections.Generic;
using System.Linq;

namespace Harvestry.Identity.Domain.ValueObjects;

/// <summary>
/// Represents a preset bundle of permissions that can be applied to roles.
/// Bundles provide quick assignment of common permission sets.
/// </summary>
public sealed class PermissionBundle
{
    private readonly HashSet<string> _permissions;

    public PermissionBundle(
        string id,
        string name,
        string description,
        string? category,
        int displayOrder,
        IEnumerable<string> permissions)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Bundle ID is required", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Bundle name is required", nameof(name));

        Id = id;
        Name = name;
        Description = description ?? string.Empty;
        Category = category;
        DisplayOrder = displayOrder;
        _permissions = new HashSet<string>(permissions ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Unique identifier for this bundle (e.g., "cultivation_operator")
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Human-readable name for display (e.g., "Cultivation Operator")
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Description of what this bundle provides
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Optional category for grouping bundles (e.g., "Cultivation", "Admin")
    /// </summary>
    public string? Category { get; }

    /// <summary>
    /// Order in which to display this bundle in the UI
    /// </summary>
    public int DisplayOrder { get; }

    /// <summary>
    /// All permission keys included in this bundle
    /// </summary>
    public IReadOnlySet<string> Permissions => _permissions;

    /// <summary>
    /// Check if this bundle includes a specific permission
    /// </summary>
    public bool HasPermission(string permissionKey) =>
        _permissions.Contains(permissionKey);

    /// <summary>
    /// Get the count of permissions in this bundle
    /// </summary>
    public int PermissionCount => _permissions.Count;

    /// <summary>
    /// Check if a set of permissions fully contains this bundle
    /// </summary>
    public bool IsFullyGrantedBy(IEnumerable<string> grantedPermissions)
    {
        var granted = new HashSet<string>(grantedPermissions, StringComparer.OrdinalIgnoreCase);
        return _permissions.All(p => granted.Contains(p));
    }

    /// <summary>
    /// Check if a set of permissions partially matches this bundle
    /// </summary>
    public bool IsPartiallyGrantedBy(IEnumerable<string> grantedPermissions)
    {
        var granted = new HashSet<string>(grantedPermissions, StringComparer.OrdinalIgnoreCase);
        return _permissions.Any(p => granted.Contains(p)) && !IsFullyGrantedBy(grantedPermissions);
    }
}
