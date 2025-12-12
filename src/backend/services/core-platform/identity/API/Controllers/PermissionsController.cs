using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Application.DTOs;
using Harvestry.Identity.Application.Interfaces;
using Harvestry.Identity.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Identity.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/permissions")]
public sealed class PermissionsController : ControllerBase
{
    private readonly IPolicyEvaluationService _policyEvaluationService;

    public PermissionsController(IPolicyEvaluationService policyEvaluationService)
    {
        _policyEvaluationService = policyEvaluationService ?? throw new ArgumentNullException(nameof(policyEvaluationService));
    }

    /// <summary>
    /// Get the complete permission registry with all sections and bundles.
    /// Used by the admin UI to render the permission editor.
    /// </summary>
    [HttpGet("registry")]
    [ProducesResponseType(typeof(PermissionRegistryDto), 200)]
    public ActionResult<PermissionRegistryDto> GetRegistry()
    {
        var sections = PermissionSections.All.Select(s => new PermissionSectionDto
        {
            Id = s.Id,
            Label = s.Label,
            Description = s.Description,
            DisplayOrder = s.DisplayOrder,
            Permissions = s.Permissions.Select(p => new PermissionDto
            {
                Key = p.Key,
                Label = p.Label,
                Description = p.Description,
                RequiresTwoPersonApproval = p.RequiresTwoPersonApproval,
                RequiresReason = p.RequiresReason
            }).ToList()
        }).ToList();

        var bundles = PermissionBundles.All.Select(b => new PermissionBundleDto
        {
            Id = b.Id,
            Name = b.Name,
            Description = b.Description,
            Category = b.Category,
            DisplayOrder = b.DisplayOrder,
            PermissionCount = b.PermissionCount,
            Permissions = b.Permissions.ToList()
        }).ToList();

        var categories = PermissionBundles.GetCategories().ToList();

        return Ok(new PermissionRegistryDto
        {
            Sections = sections,
            Bundles = bundles,
            BundleCategories = categories
        });
    }

    [HttpPost("check")]
    public async Task<ActionResult<PolicyEvaluationResponse>> CheckPermission([FromBody] PermissionCheckRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var context = request.Context ?? new Dictionary<string, object>();
        var result = await _policyEvaluationService.EvaluatePermissionAsync(
            request.UserId,
            request.Action,
            request.ResourceType,
            request.SiteId,
            context,
            cancellationToken);

        return Ok(new PolicyEvaluationResponse
        {
            Granted = result.IsGranted,
            RequiresTwoPersonApproval = result.RequiresTwoPersonApproval,
            DenyReason = result.DenyReason
        });
    }

    [HttpPost("two-person-approval")]
    [Authorize]
    public async Task<ActionResult<TwoPersonApprovalResponse>> InitiateApproval([FromBody] InitiateApprovalRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var approvalRequest = new TwoPersonApprovalRequest(
            request.Action,
            request.ResourceType,
            request.ResourceId,
            request.SiteId,
            request.InitiatorUserId,
            request.Reason,
            request.Attestation,
            request.Context);

        var response = await _policyEvaluationService.InitiateTwoPersonApprovalAsync(approvalRequest, cancellationToken);
        return CreatedAtAction(nameof(GetPendingApprovals), new { siteId = request.SiteId }, response);
    }

    [HttpPut("two-person-approval/{approvalId:guid}/approve")]
    [Authorize]
    public async Task<IActionResult> Approve(Guid approvalId, [FromBody] ApproveRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var success = await _policyEvaluationService.ApproveTwoPersonRequestAsync(
            approvalId,
            request.ApproverUserId,
            request.Reason,
            request.Attestation,
            cancellationToken);

        return success ? NoContent() : NotFound();
    }

    [HttpPut("two-person-approval/{approvalId:guid}/reject")]
    [Authorize]
    public async Task<IActionResult> Reject(Guid approvalId, [FromBody] RejectRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var success = await _policyEvaluationService.RejectTwoPersonRequestAsync(
            approvalId,
            request.ApproverUserId,
            request.Reason,
            cancellationToken);

        return success ? NoContent() : NotFound();
    }

    [HttpGet("two-person-approval/pending")]
    [Authorize]
    public async Task<IActionResult> GetPendingApprovals([FromQuery][Required] Guid siteId, CancellationToken cancellationToken)
    {
        var approvals = await _policyEvaluationService.GetPendingApprovalsAsync(siteId, cancellationToken);
        return Ok(approvals);
    }

    public sealed class PermissionCheckRequest
    {
        [Required]
        public Guid UserId { get; init; }

        [Required]
        [StringLength(128)]
        public string Action { get; init; } = null!;

        [Required]
        [StringLength(128)]
        public string ResourceType { get; init; } = null!;

        [Required]
        public Guid SiteId { get; init; }

        public Dictionary<string, object>? Context { get; init; }
    }

    public sealed class PolicyEvaluationResponse
    {
        public bool Granted { get; init; }
        public bool RequiresTwoPersonApproval { get; init; }
        public string? DenyReason { get; init; }
    }

    public sealed class InitiateApprovalRequest
    {
        [Required]
        [StringLength(128, MinimumLength = 1)]
        public string Action { get; init; } = null!;

        [Required]
        [StringLength(128, MinimumLength = 1)]
        public string ResourceType { get; init; } = null!;

        [Required]
        public Guid ResourceId { get; init; }

        [Required]
        public Guid SiteId { get; init; }

        [Required]
        public Guid InitiatorUserId { get; init; }

        [Required]
        [StringLength(1024, MinimumLength = 3)]
        public string Reason { get; init; } = null!;

        [StringLength(1024)]
        public string? Attestation { get; init; }

        public Dictionary<string, object>? Context { get; init; }
    }

    public sealed class ApproveRequest
    {
        [Required]
        public Guid ApproverUserId { get; init; }

        [Required]
        [StringLength(512, MinimumLength = 3)]
        public string Reason { get; init; } = null!;

        public string? Attestation { get; init; }
    }

    public sealed class RejectRequest
    {
        [Required]
        public Guid ApproverUserId { get; init; }

        [Required]
        [StringLength(512, MinimumLength = 3)]
        public string Reason { get; init; } = null!;
    }
}
