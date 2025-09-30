using System;
using System.Collections.Generic;

namespace Harvestry.Identity.Domain.Entities;

public sealed record AuthorizationAuditEntry(
    Guid UserId,
    Guid SiteId,
    string Action,
    string ResourceType,
    Guid? ResourceId,
    bool Granted,
    string? DenyReason = null,
    IReadOnlyDictionary<string, object?>? Context = null,
    string? IpAddress = null,
    string? UserAgent = null,
    DateTime? OccurredAt = null)
{
    public static AuthorizationAuditEntry Denied(
        Guid userId,
        Guid siteId,
        string action,
        string resourceType,
        Guid? resourceId,
        string denyReason,
        IReadOnlyDictionary<string, object?>? context = null,
        string? ipAddress = null,
        string? userAgent = null) => new(
            userId,
            siteId,
            action,
            resourceType,
            resourceId,
            Granted: false,
            DenyReason: denyReason,
            Context: context,
            IpAddress: ipAddress,
            UserAgent: userAgent);
}
