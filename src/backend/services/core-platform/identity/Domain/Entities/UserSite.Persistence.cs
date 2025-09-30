using System;

namespace Harvestry.Identity.Domain.Entities;

internal sealed partial class UserSite
{
    internal static UserSite Restore(
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
