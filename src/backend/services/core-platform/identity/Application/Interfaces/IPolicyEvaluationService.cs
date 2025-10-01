using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Application.DTOs;

namespace Harvestry.Identity.Application.Interfaces;

/// <summary>
/// Service for evaluating ABAC permissions and managing two-person approvals
/// </summary>
public interface IPolicyEvaluationService
{
    /// <summary>
    /// Check if a user has permission to perform an action
    /// </summary>
    Task<PolicyEvaluationResult> EvaluatePermissionAsync(
        Guid userId,
        string action,
        string resourceType,
        Guid siteId,
        IReadOnlyDictionary<string, object>? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiate a two-person approval workflow
    /// </summary>
    Task<TwoPersonApprovalResponse> InitiateTwoPersonApprovalAsync(
        TwoPersonApprovalRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Approve a two-person approval request
    /// </summary>
    Task<bool> ApproveTwoPersonRequestAsync(
        Guid approvalId,
        Guid approverUserId,
        string approverReason,
        string? attestation = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reject a two-person approval request
    /// </summary>
    Task<bool> RejectTwoPersonRequestAsync(
        Guid approvalId,
        Guid approverUserId,
        string rejectReason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pending two-person approvals for a site
    /// </summary>
    Task<IEnumerable<TwoPersonApprovalResponse>> GetPendingApprovalsAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for task gating logic (SOP/training prerequisites)
/// </summary>
public interface ITaskGatingService
{
    /// <summary>
    /// Check if a user can perform a specific task type
    /// </summary>
    Task<TaskGatingResult> CheckTaskGatingAsync(
        Guid userId,
        string taskType,
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all gating requirements for a task type
    /// </summary>
    Task<IEnumerable<TaskGatingRequirement>> GetRequirementsForTaskTypeAsync(
        string taskType,
        Guid? siteId = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for badge authentication
/// </summary>
public interface IBadgeAuthService
{
    /// <summary>
    /// Authenticate a user via badge scan
    /// </summary>
    Task<BadgeLoginResult> LoginWithBadgeAsync(
        string badgeCode,
        Guid siteId,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoke a badge
    /// </summary>
    Task<bool> RevokeBadgeAsync(
        Guid badgeId,
        Guid revokedBy,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active sessions for a user
    /// </summary>
    Task<IEnumerable<SessionInfo>> GetActiveSessionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoke a session (logout)
    /// </summary>
    Task<bool> RevokeSessionAsync(
        Guid sessionId,
        string? reason = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a badge login attempt
/// </summary>
public sealed class BadgeLoginResult
{
    public bool Success { get; init; }
    public Guid? SessionId { get; init; }
    public string? SessionToken { get; init; }
    public Guid? UserId { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Information about an active session
/// </summary>
public sealed class SessionInfo
{
    public Guid SessionId { get; init; }
    public Guid UserId { get; init; }
    public Guid? SiteId { get; init; }
    public DateTime SessionStart { get; init; }
    public DateTime LastActivity { get; init; }
    public DateTime ExpiresAt { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string LoginMethod { get; init; } = null!;
}
