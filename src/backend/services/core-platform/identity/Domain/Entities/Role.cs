using System;
using System.Collections.Generic;
using System.Linq;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Identity.Domain.Entities;

/// <summary>
/// Role entity - defines a set of permissions
/// </summary>
public sealed partial class Role : Entity<Guid>
{
    private readonly HashSet<string> _permissions = new();

    // Private constructor for EF Core
    private Role(Guid id) : base(id) { }

    private Role(
        Guid id,
        string roleName,
        string displayName,
        string? description,
        IEnumerable<string> permissions,
        bool isSystemRole = false) : base(id)
    {
        RoleName = roleName;
        DisplayName = displayName;
        Description = description;
        IsSystemRole = isSystemRole;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        foreach (var permission in permissions)
        {
            _permissions.Add(permission);
        }
    }

    public string RoleName { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsSystemRole { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlySet<string> Permissions => _permissions;

    /// <summary>
    /// Factory method to create a role
    /// </summary>
    public static Role Create(
        string roleName,
        string displayName,
        string? description,
        IEnumerable<string> permissions,
        bool isSystemRole = false)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new ArgumentException("Role name is required", nameof(roleName));

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required", nameof(displayName));

        if (permissions == null || !permissions.Any())
            throw new ArgumentException("At least one permission is required", nameof(permissions));

        return new Role(
            Guid.NewGuid(),
            roleName,
            displayName,
            description,
            permissions,
            isSystemRole);
    }

    /// <summary>
    /// Check if this role has a specific permission
    /// </summary>
    public bool HasPermission(string permission)
    {
        // Wildcard permission grants all
        if (_permissions.Contains("*:*"))
            return true;

        // Check exact match
        if (_permissions.Contains(permission))
            return true;

        // Check resource-level wildcard (e.g., "tasks:*" grants "tasks:read", "tasks:write")
        var parts = permission.Split(':');
        if (parts.Length == 2)
        {
            var resourceWildcard = $"{parts[0]}:*";
            if (_permissions.Contains(resourceWildcard))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Add a permission to this role
    /// </summary>
    public void AddPermission(string permission)
    {
        if (IsSystemRole)
            throw new InvalidOperationException("Cannot modify system roles");

        if (string.IsNullOrWhiteSpace(permission))
            throw new ArgumentException("Permission cannot be empty", nameof(permission));

        _permissions.Add(permission);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Remove a permission from this role
    /// </summary>
    public void RemovePermission(string permission)
    {
        if (IsSystemRole)
            throw new InvalidOperationException("Cannot modify system roles");

        _permissions.Remove(permission);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update role details
    /// </summary>
    public void Update(string? displayName = null, string? description = null)
    {
        if (IsSystemRole)
            throw new InvalidOperationException("Cannot modify system roles");

        if (!string.IsNullOrWhiteSpace(displayName))
            DisplayName = displayName;

        // Only update description if a value is provided
        if (description != null)
            Description = description;
            
        UpdatedAt = DateTime.UtcNow;
    }
}
