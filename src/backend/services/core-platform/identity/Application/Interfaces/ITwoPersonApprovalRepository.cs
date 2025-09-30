using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Application.DTOs;

namespace Harvestry.Identity.Application.Interfaces;

public interface ITwoPersonApprovalRepository
{
    Task<TwoPersonApprovalResponse> CreateAsync(
        TwoPersonApprovalRequest request,
        DateTime expiresAt,
        CancellationToken cancellationToken = default);

    Task<TwoPersonApprovalRecord?> GetByIdAsync(
        Guid approvalId,
        CancellationToken cancellationToken = default);

    Task<bool> ApproveAsync(
        Guid approvalId,
        Guid approverUserId,
        string reason,
        string? attestation,
        CancellationToken cancellationToken = default);

    Task<bool> RejectAsync(
        Guid approvalId,
        Guid approverUserId,
        string reason,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<TwoPersonApprovalResponse>> GetPendingAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);
}

public sealed record TwoPersonApprovalRecord(
    Guid ApprovalId,
    string Action,
    string ResourceType,
    Guid ResourceId,
    Guid SiteId,
    Guid InitiatorUserId,
    string InitiatorReason,
    string? InitiatorAttestation,
    DateTime InitiatedAt,
    string Status,
    DateTime ExpiresAt,
    Guid? ApproverUserId,
    DateTime? ApprovedAt);
