using System;

namespace Harvestry.Identity.Domain.Entities;

public sealed partial class UserSite
{
public static UserSite Restore(
        Guid id,
        Guid userId,
        Guid siteId,
        Guid roleId,
        bool isPrimarySite,
        DateTime assignedAt,
        Guid assignedBy,
        DateTime? revokedAt,
        Guid? revokedBy,
        string? revokeReason)
    {
        return new UserSite(id)
        {
            UserId = userId,
            SiteId = siteId,
            RoleId = roleId,
            IsPrimarySite = isPrimarySite,
            AssignedAt = assignedAt,
            AssignedBy = assignedBy,
            RevokedAt = revokedAt,
            RevokedBy = revokedBy,
            RevokeReason = revokeReason
        };
    }
}
