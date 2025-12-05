using System;
using System.Collections.Generic;
using Harvestry.Identity.Domain.Enums;

namespace Harvestry.Identity.Domain.Entities;

public sealed partial class Session
{
    public static Session Restore(
        Guid id,
        Guid userId,
        Guid? siteId,
        string sessionToken,
        string? refreshToken,
        string? deviceFingerprint,
        string? ipAddress,
        string? userAgent,
        LoginMethod loginMethod,
        DateTime sessionStart,
        DateTime? sessionEnd,
        DateTime lastActivity,
        DateTime expiresAt,
        bool isRevoked,
        string? revokeReason,
        IDictionary<string, object>? metadata)
    {
        var session = new Session(id)
        {
            UserId = userId,
            SiteId = siteId,
            SessionToken = sessionToken,
            RefreshToken = refreshToken,
            DeviceFingerprint = deviceFingerprint,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            LoginMethod = loginMethod,
            SessionStart = sessionStart,
            SessionEnd = sessionEnd,
            LastActivity = lastActivity,
            ExpiresAt = expiresAt,
            IsRevoked = isRevoked,
            RevokeReason = revokeReason,
            Metadata = metadata != null
                ? new Dictionary<string, object>(metadata)
                : new Dictionary<string, object>()
        };

        return session;
    }
}
