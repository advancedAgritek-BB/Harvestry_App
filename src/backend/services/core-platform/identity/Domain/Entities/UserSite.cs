using System;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Identity.Domain.Entities;

/// <summary>
/// Represents a user's assignment to a site with a specific role
/// </summary>
public sealed partial class UserSite : Entity<Guid>
{
    // Private constructor for EF Core
    private UserSite(Guid id) : base(id) { }

    private UserSite(
        Guid id,
        Guid userId,
        Guid siteId,
        Guid roleId,
        bool isPrimarySite,
        Guid assignedBy) : base(id)
    {
        UserId = userId;
        SiteId = siteId;
        RoleId = roleId;
        IsPrimarySite = isPrimarySite;
        AssignedAt = DateTime.UtcNow;
        AssignedBy = assignedBy;
    }

    public Guid UserId { get; private set; }
    public Guid SiteId { get; private set; }
    public Guid RoleId { get; private set; }
    public bool IsPrimarySite { get; private set; }
    public DateTime AssignedAt { get; private set; }
    public Guid AssignedBy { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public Guid? RevokedBy { get; private set; }
    public string? RevokeReason { get; private set; }

    /// <summary>
    /// Is this assignment currently active?
    /// </summary>
    public bool IsActive => RevokedAt == null;

    /// <summary>
    /// Factory method to create a user-site assignment
    /// </summary>
    public static UserSite Create(
        Guid userId,
        Guid siteId,
        Guid roleId,
        bool isPrimarySite,
        Guid assignedBy)
    {
        return new UserSite(
            Guid.NewGuid(),
            userId,
            siteId,
            roleId,
            isPrimarySite,
            assignedBy);
    }

    /// <summary>
    /// Revoke this site assignment
    /// </summary>
    public void Revoke(Guid revokedBy, string reason)
    {
        if (RevokedAt.HasValue)
            throw new InvalidOperationException("Assignment is already revoked");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Revoke reason is required", nameof(reason));

        RevokedAt = DateTime.UtcNow;
        RevokedBy = revokedBy;
        RevokeReason = reason;
    }

    /// <summary>
    /// Set as primary site
    /// </summary>
    public void SetAsPrimary()
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot set revoked assignment as primary");

        IsPrimarySite = true;
    }

    /// <summary>
    /// Unset as primary site
    /// </summary>
    public void UnsetAsPrimary()
    {
        IsPrimarySite = false;
    }

    /// <summary>
    /// Change the role for this site assignment
    /// </summary>
    public void ChangeRole(Guid newRoleId)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot change role on revoked assignment");

        RoleId = newRoleId;
    }
}
