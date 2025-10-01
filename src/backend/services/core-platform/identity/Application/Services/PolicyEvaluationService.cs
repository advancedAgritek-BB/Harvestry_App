using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Application.DTOs;
using Harvestry.Identity.Application.Interfaces;
using Harvestry.Identity.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Harvestry.Identity.Application.Services;

/// <summary>
/// Service for evaluating ABAC permissions and managing two-person approvals
/// </summary>
public sealed class PolicyEvaluationService : IPolicyEvaluationService
{
    private readonly IDatabaseRepository _databaseRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISiteRepository _siteRepository;
    private readonly ITwoPersonApprovalRepository _twoPersonApprovalRepository;
    private readonly IAuthorizationAuditRepository _authorizationAuditRepository;
    private readonly ILogger<PolicyEvaluationService> _logger;

    public PolicyEvaluationService(
        IDatabaseRepository databaseRepository,
        IUserRepository userRepository,
        ISiteRepository siteRepository,
        ITwoPersonApprovalRepository twoPersonApprovalRepository,
        IAuthorizationAuditRepository authorizationAuditRepository,
        ILogger<PolicyEvaluationService> logger)
    {
        _databaseRepository = databaseRepository ?? throw new ArgumentNullException(nameof(databaseRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _siteRepository = siteRepository ?? throw new ArgumentNullException(nameof(siteRepository));
        _twoPersonApprovalRepository = twoPersonApprovalRepository ?? throw new ArgumentNullException(nameof(twoPersonApprovalRepository));
        _authorizationAuditRepository = authorizationAuditRepository ?? throw new ArgumentNullException(nameof(authorizationAuditRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Check if a user has permission to perform an action
    /// </summary>
    public async Task<PolicyEvaluationResult> EvaluatePermissionAsync(
        Guid userId,
        string action,
        string resourceType,
        Guid siteId,
        IReadOnlyDictionary<string, object>? context = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Evaluating permission for user {UserId} to perform {Action} on {ResourceType} at site {SiteId}",
            userId, action, resourceType, siteId);

        try
        {
            // Verify user exists
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Permission denied: User {UserId} not found", userId);
                return PolicyEvaluationResult.Deny("User not found");
            }

            // Verify site exists
            var site = await _siteRepository.GetByIdAsync(siteId, cancellationToken);
            if (site == null)
            {
                _logger.LogWarning("Permission denied: Site {SiteId} not found", siteId);
                return PolicyEvaluationResult.Deny("Site not found");
            }

            // Sanitize and validate context
            var sanitizedContext = SanitizeContext(context);

            // Call database ABAC function
            var (granted, requiresTwoPersonApproval, denyReason) = await _databaseRepository.CheckAbacPermissionAsync(
                userId,
                action,
                resourceType,
                siteId,
                sanitizedContext,
                cancellationToken);

            if (granted)
            {
                _logger.LogInformation(
                    "Permission granted for user {UserId} to perform {Action} on {ResourceType}. Two-person approval required: {RequiresTwoPersonApproval}",
                    userId, action, resourceType, requiresTwoPersonApproval);

                return PolicyEvaluationResult.Grant(requiresTwoPersonApproval);
            }

            _logger.LogWarning(
                "Permission denied for user {UserId} to perform {Action} on {ResourceType}. Reason: {Reason}",
                userId, action, resourceType, denyReason);

            return PolicyEvaluationResult.Deny(denyReason ?? "Permission denied");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error evaluating permission for user {UserId} to perform {Action} on {ResourceType}",
                userId, action, resourceType);

            throw;
        }
    }

    /// <summary>
    /// Initiate a two-person approval workflow
    /// </summary>
    public async Task<TwoPersonApprovalResponse> InitiateTwoPersonApprovalAsync(
        TwoPersonApprovalRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        _logger.LogInformation(
            "Initiating two-person approval for {Action} on {ResourceType} {ResourceId} by user {InitiatorUserId}",
            request.Action, request.ResourceType, request.ResourceId, request.InitiatorUserId);

        try
        {
            // Validate initiator has permission (first person)
            var evaluation = await EvaluatePermissionAsync(
                request.InitiatorUserId,
                request.Action,
                request.ResourceType,
                request.SiteId,
                request.Context,
                cancellationToken);

            if (!evaluation.IsGranted)
            {
                throw new InvalidOperationException($"Initiator does not have permission: {evaluation.DenyReason}");
            }

            if (!evaluation.RequiresTwoPersonApproval)
            {
                throw new InvalidOperationException("This action does not require two-person approval");
            }

            var expiresAt = DateTime.UtcNow.AddHours(24);
            var approval = await _twoPersonApprovalRepository.CreateAsync(request, expiresAt, cancellationToken);

            _logger.LogInformation(
                "Two-person approval {ApprovalId} initiated successfully. Expires at {ExpiresAt}",
                approval.ApprovalId, approval.ExpiresAt);

            return approval;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error initiating two-person approval for {Action} on {ResourceType} {ResourceId}",
                request.Action, request.ResourceType, request.ResourceId);

            throw;
        }
    }

    /// <summary>
    /// Approve a two-person approval request
    /// </summary>
    public async Task<bool> ApproveTwoPersonRequestAsync(
        Guid approvalId,
        Guid approverUserId,
        string approverReason,
        string? attestation = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(approverReason))
            throw new ArgumentException("Approver reason is required", nameof(approverReason));

        _logger.LogInformation(
            "User {ApproverUserId} approving two-person approval {ApprovalId}",
            approverUserId, approvalId);

        try
        {
            var record = await _twoPersonApprovalRepository.GetByIdAsync(approvalId, cancellationToken);
            if (record is null)
            {
                return false;
            }

            if (!string.Equals(record.Status, "pending", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Approval {ApprovalId} is not pending (status {Status})", approvalId, record.Status);
                return false;
            }

            if (record.ExpiresAt <= DateTime.UtcNow)
            {
                _logger.LogWarning("Approval {ApprovalId} expired at {ExpiresAt}", approvalId, record.ExpiresAt);
                var auditContext = new Dictionary<string, object?>
                {
                    ["approval_id"] = approvalId,
                    ["existing_status"] = record.Status,
                    ["expires_at"] = record.ExpiresAt
                };

                var auditEntry = AuthorizationAuditEntry.Denied(
                    approverUserId,
                    record.SiteId,
                    record.Action,
                    record.ResourceType,
                    record.ResourceId,
                    "Approval expired",
                    auditContext);

                await _authorizationAuditRepository.LogAsync(auditEntry, cancellationToken).ConfigureAwait(false);
                return false;
            }

            if (record.InitiatorUserId == approverUserId)
            {
                throw new InvalidOperationException("Approver must be different from initiator");
            }

            var evaluation = await EvaluatePermissionAsync(
                approverUserId,
                record.Action,
                record.ResourceType,
                record.SiteId,
                null,
                cancellationToken);

            if (!evaluation.IsGranted)
            {
                throw new InvalidOperationException($"Approver does not have permission: {evaluation.DenyReason}");
            }

            var success = await _twoPersonApprovalRepository.ApproveAsync(
                approvalId,
                approverUserId,
                approverReason,
                attestation,
                cancellationToken);

            if (success)
            {
                _logger.LogInformation(
                    "Two-person approval {ApprovalId} approved by user {ApproverUserId}",
                    approvalId, approverUserId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error approving two-person approval {ApprovalId} by user {ApproverUserId}",
                approvalId, approverUserId);

            throw;
        }
    }

    /// <summary>
    /// Reject a two-person approval request
    /// </summary>
    public async Task<bool> RejectTwoPersonRequestAsync(
        Guid approvalId,
        Guid approverUserId,
        string rejectReason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rejectReason))
            throw new ArgumentException("Reject reason is required", nameof(rejectReason));

        var record = await _twoPersonApprovalRepository.GetByIdAsync(approvalId, cancellationToken);
        if (record is null)
        {
            return false;
        }

        if (!string.Equals(record.Status, "pending", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Approval {ApprovalId} is not pending (status {Status})", approvalId, record.Status);
            return false;
        }

        // Check expiration (matching approval flow)
        if (record.ExpiresAt <= DateTime.UtcNow)
        {
            _logger.LogWarning("Approval {ApprovalId} expired at {ExpiresAt}", approvalId, record.ExpiresAt);
            var auditContext = new Dictionary<string, object?>
            {
                ["approval_id"] = approvalId,
                ["existing_status"] = record.Status,
                ["expires_at"] = record.ExpiresAt
            };

            var auditEntry = AuthorizationAuditEntry.Denied(
                approverUserId,
                record.SiteId,
                record.Action,
                record.ResourceType,
                record.ResourceId,
                "Approval expired",
                auditContext);

            await _authorizationAuditRepository.LogAsync(auditEntry, cancellationToken).ConfigureAwait(false);
            return false;
        }

        if (record.InitiatorUserId == approverUserId)
        {
            throw new InvalidOperationException("Initiator cannot reject their own approval request");
        }

        // Check permission (matching approval flow)
        var evaluation = await EvaluatePermissionAsync(
            approverUserId,
            record.Action,
            record.ResourceType,
            record.SiteId,
            null,
            cancellationToken);

        if (!evaluation.IsGranted)
        {
            throw new InvalidOperationException($"Approver does not have permission to reject: {evaluation.DenyReason}");
        }

        var success = await _twoPersonApprovalRepository.RejectAsync(
            approvalId,
            approverUserId,
            rejectReason,
            cancellationToken);

        if (success)
        {
            _logger.LogInformation(
                "Two-person approval {ApprovalId} rejected by user {ApproverUserId}",
                approvalId, approverUserId);
        }

        return success;
    }

    /// <summary>
    /// Get pending two-person approvals for a site
    /// </summary>
    public async Task<IEnumerable<TwoPersonApprovalResponse>> GetPendingApprovalsAsync(
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        return await _twoPersonApprovalRepository.GetPendingAsync(siteId, cancellationToken);
    }

    /// <summary>
    /// Sanitize and validate context dictionary for ABAC evaluation
    /// </summary>
    private Dictionary<string, object>? SanitizeContext(IReadOnlyDictionary<string, object>? context)
    {
        if (context == null || context.Count == 0)
            return null;

        var sanitized = new Dictionary<string, object>();
        var allowedKeys = new HashSet<string>
        {
            "resource_id", "site_id", "role", "department", "shift",
            "timestamp", "location", "amount", "category", "priority",
            "status", "owner_id", "created_by", "updated_by"
        };

        foreach (var kvp in context)
        {
            // Skip unexpected keys
            if (!allowedKeys.Contains(kvp.Key))
            {
                _logger.LogWarning("Context key '{Key}' is not in allow-list, dropping", kvp.Key);
                continue;
            }

            // Skip null values
            if (kvp.Value == null)
                continue;

            // Validate and sanitize value types
            var value = kvp.Value;
            if (value is string strValue)
            {
                // Trim and limit length
                var trimmed = strValue.Trim();
                if (trimmed.Length > 500)
                {
                    _logger.LogWarning(
                        "Context key '{Key}' value too long ({OriginalLength} chars), truncating to {MaxLength}",
                        kvp.Key, strValue.Length, 500);
                    trimmed = trimmed.Substring(0, 500);
                }
                sanitized[kvp.Key] = trimmed;
            }
            else if (value is int || value is long || value is decimal || value is double ||
                     value is bool || value is Guid || value is DateTime)
            {
                // Safe primitive types
                sanitized[kvp.Key] = value;
            }
            else
            {
                _logger.LogWarning("Context key '{Key}' has unsupported type {Type}, dropping",
                    kvp.Key, value.GetType().Name);
            }
        }

        return sanitized.Count > 0 ? sanitized : null;
    }
}
