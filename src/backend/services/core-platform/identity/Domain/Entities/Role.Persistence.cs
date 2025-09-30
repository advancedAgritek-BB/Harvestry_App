using System;
using System.Collections.Generic;
using System.Linq;

namespace Harvestry.Identity.Domain.Entities;

internal sealed partial class Role
{
    internal static Role Restore(
        Guid id,
        string roleName,
        string displayName,
        string? description,
        IEnumerable<string> permissions,
        bool isSystemRole,
        DateTime createdAt,
        DateTime updatedAt)
    {
        var role = new Role(id)
        {
            RoleName = roleName,
            DisplayName = displayName,
            Description = description,
            IsSystemRole = isSystemRole,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        foreach (var permission in permissions ?? Enumerable.Empty<string>())
        {
            role._permissions.Add(permission);
        }

        return role;
    }
}
