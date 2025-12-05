using System;
using System.Collections.Generic;

namespace Harvestry.Identity.Domain.Entities;

public sealed record AuthorizationAuditEntry
{
    public AuthorizationAuditEntry(
        Guid userId,
        Guid siteId,
        string action,
        string resourceType,
        Guid? resourceId,
        bool granted,
        string? denyReason = null,
        IReadOnlyDictionary<string, object?>? context = null,
        string? ipAddress = null,
        string? userAgent = null,
        DateTime? occurredAt = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));
        if (siteId == Guid.Empty)
            throw new ArgumentException("SiteId cannot be empty.", nameof(siteId));
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be null or whitespace.", nameof(action));
        if (string.IsNullOrWhiteSpace(resourceType))
            throw new ArgumentException("ResourceType cannot be null or whitespace.", nameof(resourceType));

        this.UserId = userId;
        this.SiteId = siteId;
        this.Action = action;
        this.ResourceType = resourceType;
        this.ResourceId = resourceId;
        this.Granted = granted;
        this.DenyReason = denyReason;
        this.Context = context;
        this.IpAddress = ipAddress;
        this.UserAgent = userAgent;
        OccurredAt = occurredAt ?? DateTime.UtcNow;
    }
    
    public Guid UserId { get; }
    public Guid SiteId { get; }
    public string Action { get; }
    public string ResourceType { get; }
    public Guid? ResourceId { get; }
    public bool Granted { get; }
    public string? DenyReason { get; }
    public IReadOnlyDictionary<string, object?>? Context { get; }
    public string? IpAddress { get; }
    public string? UserAgent { get; }
    public DateTime OccurredAt { get; }

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
            userId: userId,
            siteId: siteId,
            action: action,
            resourceType: resourceType,
            resourceId: resourceId,
            granted: false,
            denyReason: denyReason,
            context: context,
            ipAddress: ipAddress,
            userAgent: userAgent);
}
