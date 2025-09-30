using System;
using System.Collections.Generic;

namespace Harvestry.Identity.Application.DTOs;

/// <summary>
/// Result of an ABAC permission evaluation
/// </summary>
public sealed class PolicyEvaluationResult
{
    private PolicyEvaluationResult(
        bool isGranted,
        bool requiresTwoPersonApproval,
        string? denyReason)
    {
        IsGranted = isGranted;
        RequiresTwoPersonApproval = requiresTwoPersonApproval;
        DenyReason = denyReason;
    }

    /// <summary>
    /// Was the permission granted?
    /// </summary>
    public bool IsGranted { get; }

    /// <summary>
    /// Does this action require two-person approval?
    /// </summary>
    public bool RequiresTwoPersonApproval { get; }

    /// <summary>
    /// Reason why the permission was denied (if applicable)
    /// </summary>
    public string? DenyReason { get; }

    /// <summary>
    /// Create a granted result
    /// </summary>
    public static PolicyEvaluationResult Grant(bool requiresTwoPersonApproval = false)
    {
        return new PolicyEvaluationResult(true, requiresTwoPersonApproval, null);
    }

    /// <summary>
    /// Create a denied result
    /// </summary>
    public static PolicyEvaluationResult Deny(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Deny reason is required", nameof(reason));

        return new PolicyEvaluationResult(false, false, reason);
    }
}

/// <summary>
/// Result of a task gating check
/// </summary>
public sealed class TaskGatingResult
{
    private TaskGatingResult(
        bool isAllowed,
        List<TaskGatingRequirement> missingRequirements)
    {
        IsAllowed = isAllowed;
        // Create defensive copy to ensure immutability
        MissingRequirements = new List<TaskGatingRequirement>(missingRequirements).AsReadOnly();
    }

    /// <summary>
    /// Is the user allowed to perform this task?
    /// </summary>
    public bool IsAllowed { get; }

    /// <summary>
    /// List of missing requirements (empty if allowed)
    /// </summary>
    public IReadOnlyList<TaskGatingRequirement> MissingRequirements { get; }

    /// <summary>
    /// Create an allowed result
    /// </summary>
    public static TaskGatingResult Allow()
    {
        return new TaskGatingResult(true, new List<TaskGatingRequirement>());
    }

    /// <summary>
    /// Create a blocked result with missing requirements
    /// </summary>
    public static TaskGatingResult Block(List<TaskGatingRequirement> missingRequirements)
    {
        if (missingRequirements == null || missingRequirements.Count == 0)
            throw new ArgumentException("At least one missing requirement is required", nameof(missingRequirements));

        return new TaskGatingResult(false, missingRequirements);
    }
}

/// <summary>
/// A missing requirement for task gating
/// </summary>
public sealed class TaskGatingRequirement
{
    public TaskGatingRequirement(
        string requirementType,
        Guid? requirementId,
        string reason)
    {
        RequirementType = requirementType;
        RequirementId = requirementId;
        Reason = reason;
    }

    /// <summary>
    /// Type of requirement: "sop", "training", "permission"
    /// </summary>
    public string RequirementType { get; }

    /// <summary>
    /// ID of the required SOP or training module (if applicable)
    /// </summary>
    public Guid? RequirementId { get; }

    /// <summary>
    /// Human-readable reason for the requirement
    /// </summary>
    public string Reason { get; }
}

/// <summary>
/// Request to initiate a two-person approval
/// </summary>
public sealed class TwoPersonApprovalRequest
{
    public TwoPersonApprovalRequest(
        string action,
        string resourceType,
        Guid resourceId,
        Guid siteId,
        Guid initiatorUserId,
        string reason,
        string? attestation = null,
        Dictionary<string, object>? context = null)
    {
        Action = action;
        ResourceType = resourceType;
        ResourceId = resourceId;
        SiteId = siteId;
        InitiatorUserId = initiatorUserId;
        Reason = reason;
        Attestation = attestation;
        // Create immutable copy of context dictionary
        Context = context != null 
            ? new Dictionary<string, object>(context) 
            : new Dictionary<string, object>();
    }

    public string Action { get; }
    public string ResourceType { get; }
    public Guid ResourceId { get; }
    public Guid SiteId { get; }
    public Guid InitiatorUserId { get; }
    public string Reason { get; }
    public string? Attestation { get; }
    public IReadOnlyDictionary<string, object> Context { get; }
}

/// <summary>
/// Response when creating a two-person approval
/// </summary>
public sealed class TwoPersonApprovalResponse
{
    public TwoPersonApprovalResponse(
        Guid approvalId,
        string status,
        DateTime expiresAt)
    {
        ApprovalId = approvalId;
        Status = status;
        ExpiresAt = expiresAt;
    }

    public Guid ApprovalId { get; }
    public string Status { get; }
    public DateTime ExpiresAt { get; }
    public DateTime? InitiatedAt { get; init; }
}
