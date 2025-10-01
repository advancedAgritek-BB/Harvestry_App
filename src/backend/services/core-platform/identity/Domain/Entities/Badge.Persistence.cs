using System;
using System.Collections.Generic;
using Harvestry.Identity.Domain.Enums;
using Harvestry.Identity.Domain.ValueObjects;

namespace Harvestry.Identity.Domain.Entities;

public sealed partial class Badge
{
public static Badge Restore(
        Guid id,
        Guid userId,
        Guid siteId,
        BadgeCode badgeCode,
        BadgeType badgeType,
        BadgeStatus status,
        DateTime issuedAt,
        DateTime? expiresAt,
        DateTime? lastUsedAt,
        DateTime? revokedAt,
        Guid? revokedBy,
        string? revokeReason,
        IDictionary<string, object>? metadata,
        DateTime createdAt,
        DateTime updatedAt)
    {
        var badge = new Badge(id)
        {
            UserId = userId,
            SiteId = siteId,
            BadgeCode = badgeCode,
            BadgeType = badgeType,
            Status = status,
            IssuedAt = issuedAt,
            ExpiresAt = expiresAt,
            LastUsedAt = lastUsedAt,
            RevokedAt = revokedAt,
            RevokedBy = revokedBy,
            RevokeReason = revokeReason,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        if (metadata != null)
        {
            foreach (var kvp in metadata)
            {
                badge._metadata[kvp.Key] = kvp.Value;
            }
        }

        return badge;
    }
}
