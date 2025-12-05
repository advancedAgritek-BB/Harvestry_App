using System;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Application.DTOs;
using Harvestry.Identity.Application.Interfaces;
using Harvestry.Identity.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Harvestry.Identity.Tests.Integration;

[Collection("IntegrationTests")]
public sealed class TwoPersonApprovalIntegrationTests : IntegrationTestBase
{
    private static readonly Guid DenverManager = Guid.Parse("00000000-0000-0000-0000-000000000102");
    private static readonly Guid DenverAdmin = Guid.Parse("00000000-0000-0000-0000-000000000103");

    [IntegrationFact]
    public async Task TwoPersonApprovalLifecycle_Works()
    {
        var policyService = ServiceProvider.GetRequiredService<IPolicyEvaluationService>();

        var denverSite = await GetSiteIdAsync("DEN-001");

        SetUserContext(Guid.Empty, "service_account", denverSite);
        var request = new TwoPersonApprovalRequest(
            "inventory:destroy",
            "lot",
            Guid.NewGuid(),
            denverSite,
            DenverManager,
            "Destroy expired lot");

        var approval = await policyService.InitiateTwoPersonApprovalAsync(request);
        Assert.Equal("pending", approval.Status);

        SetUserContext(Guid.Empty, "service_account", denverSite);
        var approved = await policyService.ApproveTwoPersonRequestAsync(
            approval.ApprovalId,
            DenverAdmin,
            "Reviewed and approved");

        Assert.True(approved);
        var pending = await policyService.GetPendingApprovalsAsync(denverSite);
        Assert.Empty(pending);
    }

    [IntegrationFact]
    public async Task TwoPersonApproval_RejectFlow_Works()
    {
        var policyService = ServiceProvider.GetRequiredService<IPolicyEvaluationService>();

        var denverSite = await GetSiteIdAsync("DEN-001");

        SetUserContext(Guid.Empty, "service_account", denverSite);
        var request = new TwoPersonApprovalRequest(
            "inventory:destroy",
            "lot",
            Guid.NewGuid(),
            denverSite,
            DenverManager,
            "Destroy damaged lot");

        var approval = await policyService.InitiateTwoPersonApprovalAsync(request);

        SetUserContext(Guid.Empty, "service_account", denverSite);
        var rejected = await policyService.RejectTwoPersonRequestAsync(
            approval.ApprovalId,
            DenverAdmin,
            "Hold for review");

        Assert.True(rejected);
    }

    [IntegrationFact]
    public async Task TwoPersonApproval_SameUserApprove_Throws()
    {
        var policyService = ServiceProvider.GetRequiredService<IPolicyEvaluationService>();

        var denverSite = await GetSiteIdAsync("DEN-001");

        SetUserContext(Guid.Empty, "service_account", denverSite);
        var request = new TwoPersonApprovalRequest(
            "inventory:destroy",
            "lot",
            Guid.NewGuid(),
            denverSite,
            DenverManager,
            "Destroy damaged lot");

        var approval = await policyService.InitiateTwoPersonApprovalAsync(request);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            policyService.ApproveTwoPersonRequestAsync(approval.ApprovalId, DenverManager, "Self approval"));
    }

    [IntegrationFact]
    public async Task TwoPersonApproval_Expired_Fails()
    {
        var policyService = ServiceProvider.GetRequiredService<IPolicyEvaluationService>();

        var denverSite = await GetSiteIdAsync("DEN-001");

        SetUserContext(Guid.Empty, "service_account", denverSite);
        var request = new TwoPersonApprovalRequest(
            "inventory:destroy",
            "lot",
            Guid.NewGuid(),
            denverSite,
            DenverManager,
            "Destroy damaged lot");

        var approval = await policyService.InitiateTwoPersonApprovalAsync(request);

        await ExecuteSqlAsync(
            "UPDATE two_person_approvals SET expires_at = NOW() - INTERVAL '1 minute' WHERE approval_id = @approvalId",
            new { approvalId = approval.ApprovalId });

        var approved = await policyService.ApproveTwoPersonRequestAsync(approval.ApprovalId, DenverAdmin, "Late approval");

        Assert.False(approved);

        var dbContext = ServiceProvider.GetRequiredService<IdentityDbContext>();
        await dbContext.SetRlsContextAsync(Guid.Empty, "service_account", denverSite);
        await using var connection = await dbContext.GetOpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT deny_reason
            FROM authorization_audit
            WHERE user_id = @user_id
              AND action = @action
              AND resource_type = @resource_type
            ORDER BY occurred_at DESC
            LIMIT 1;";
        command.Parameters.AddWithValue("@user_id", DenverAdmin);
        command.Parameters.AddWithValue("@action", approval.Action);
        command.Parameters.AddWithValue("@resource_type", approval.ResourceType);

        var denyReason = await command.ExecuteScalarAsync();
        Assert.Equal("Approval expired", denyReason?.ToString());
        await dbContext.ResetRlsContextAsync();
    }
}
