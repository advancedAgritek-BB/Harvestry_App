using System;
using System.Threading;
using System.Threading.Tasks;

namespace Harvestry.Tasks.Application.Interfaces;

/// <summary>
/// Provides authorization checks for site access and permissions.
/// </summary>
public interface ISiteAuthorizationService
{
    /// <summary>
    /// Checks if the authenticated user has access to the specified site.
    /// </summary>
    /// <param name="userId">The user ID to check.</param>
    /// <param name="siteId">The site ID to check access for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the user has access to the site, false otherwise.</returns>
    Task<bool> HasSiteAccessAsync(Guid userId, Guid siteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the authenticated user is an admin in the specified site.
    /// </summary>
    /// <param name="userId">The user ID to check.</param>
    /// <param name="siteId">The site ID to check admin status for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the user is an admin in the site, false otherwise.</returns>
    Task<bool> IsSiteAdminAsync(Guid userId, Guid siteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user can assign tasks to another user at a specific site.
    /// Validates: assignee has access to target site, and assigner has permission.
    /// </summary>
    /// <param name="assignerUserId">The user performing the assignment.</param>
    /// <param name="assigneeUserId">The user being assigned the task.</param>
    /// <param name="targetSiteId">The site where the task belongs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result indicating if assignment is allowed.</returns>
    Task<CrossSiteAssignmentResult> ValidateCrossSiteAssignmentAsync(
        Guid assignerUserId,
        Guid assigneeUserId,
        Guid targetSiteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has cross-site task management permission within an organization.
    /// </summary>
    /// <param name="userId">The user ID to check.</param>
    /// <param name="orgId">The organization ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the user has cross-site management permission.</returns>
    Task<bool> HasCrossSiteManagementPermissionAsync(Guid userId, Guid orgId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the organization ID for a site.
    /// </summary>
    Task<Guid?> GetOrgIdForSiteAsync(Guid siteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if two sites belong to the same organization.
    /// </summary>
    Task<bool> AreSitesInSameOrgAsync(Guid siteId1, Guid siteId2, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of cross-site assignment validation.
/// </summary>
public sealed class CrossSiteAssignmentResult
{
    public bool IsAllowed { get; init; }
    public string? FailureReason { get; init; }
    public bool IsCrossSite { get; init; }

    public static CrossSiteAssignmentResult Allowed(bool isCrossSite = false) =>
        new() { IsAllowed = true, IsCrossSite = isCrossSite };

    public static CrossSiteAssignmentResult Denied(string reason) =>
        new() { IsAllowed = false, FailureReason = reason };
}
