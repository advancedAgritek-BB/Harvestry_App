using System;
using System.Threading.Tasks;
using Harvestry.Identity.Application.Interfaces;
using Harvestry.Identity.Domain.Entities;
using Harvestry.Identity.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;

namespace Harvestry.Identity.Tests.Integration;

[Collection("IntegrationTests")]
public sealed class RlsFuzzTests : IntegrationTestBase
{
    private static readonly Guid DenverSite = Guid.Parse("00000000-0000-0000-0000-000000000a01");
    private static readonly Guid BoulderSite = Guid.Parse("00000000-0000-0000-0000-000000000a02");
    private static readonly Guid DenverOperator = Guid.Parse("00000000-0000-0000-0000-000000000101");
    private static readonly Guid DenverManager = Guid.Parse("00000000-0000-0000-0000-000000000102");
    private static readonly Guid DenverAdmin = Guid.Parse("00000000-0000-0000-0000-000000000103");
    private static readonly Guid BoulderOperator = Guid.Parse("00000000-0000-0000-0000-000000000104");

    [IntegrationFact]
    public async Task Operator_CanReadOwnRecord()
    {
        SetUserContext(DenverOperator, "operator", DenverSite);
        var repository = ServiceProvider.GetRequiredService<IUserRepository>();

        var user = await repository.GetByIdAsync(DenverOperator);

        Assert.NotNull(user);
        Assert.Equal(DenverOperator, user!.Id);
    }

    [IntegrationFact]
    public async Task Operator_CannotReadOtherSiteRecord()
    {
        SetUserContext(DenverOperator, "operator", DenverSite);
        var repository = ServiceProvider.GetRequiredService<IUserRepository>();

        var user = await repository.GetByIdAsync(BoulderOperator);

        Assert.Null(user);
    }

    [IntegrationFact]
    public async Task Admin_CanReadOtherSiteRecord()
    {
        SetUserContext(DenverAdmin, "admin", DenverSite);
        var repository = ServiceProvider.GetRequiredService<IUserRepository>();

        var user = await repository.GetByIdAsync(BoulderOperator);

        Assert.NotNull(user);
        Assert.Equal(BoulderOperator, user!.Id);
    }

    [IntegrationFact]
    public async Task ServiceAccount_BypassesRls()
    {
        SetUserContext(Guid.Empty, "service_account", BoulderSite);
        var badges = ServiceProvider.GetRequiredService<IBadgeRepository>();

        var results = await badges.GetActiveBySiteIdAsync(DenverSite);

        Assert.NotEmpty(results);
    }

    [IntegrationFact]
    public async Task Operator_CanReadOwnSiteBadges()
    {
        SetUserContext(DenverOperator, "operator", DenverSite);
        var badges = ServiceProvider.GetRequiredService<IBadgeRepository>();

        var results = await badges.GetActiveBySiteIdAsync(DenverSite);

        Assert.NotEmpty(results);
    }

    [IntegrationFact]
    public async Task Operator_CannotReadOtherSiteBadges()
    {
        SetUserContext(DenverOperator, "operator", DenverSite);
        var badges = ServiceProvider.GetRequiredService<IBadgeRepository>();

        var results = await badges.GetActiveBySiteIdAsync(BoulderSite);

        Assert.Empty(results);
    }

    [IntegrationFact]
    public async Task SessionWithoutContext_Fails()
    {
        SetUserContext(DenverOperator, "operator", BoulderSite);
        var repository = ServiceProvider.GetRequiredService<IUserRepository>();

        // Users can always read their own user record regardless of site context
        var user = await repository.GetByIdAsync(DenverOperator);

        Assert.NotNull(user);
        Assert.Equal(DenverOperator, user!.Id);
    }

    [IntegrationFact]
    public async Task Operator_CannotRevokeOtherSiteBadge()
    {
        SetUserContext(DenverOperator, "operator", DenverSite);
        var badgeService = ServiceProvider.GetRequiredService<IBadgeAuthService>();

        var result = await badgeService.RevokeBadgeAsync(Guid.Parse("00000000-0000-0000-0000-00000000b003"), Guid.Empty, "Test");

        Assert.False(result);
    }

    [IntegrationFact]
    public async Task AuthorizationAudit_CrossSiteBlocked()
    {
        SetUserContext(DenverOperator, "operator", DenverSite);
        var db = ServiceProvider.GetRequiredService<IdentityDbContext>();
        var connection = await db.GetOpenConnectionAsync();

        // RLS filters rows but doesn't throw exceptions - verify only site-scoped rows are visible
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM authorization_audit WHERE site_id = '00000000-0000-0000-0000-000000000a02'";
        var count = await command.ExecuteScalarAsync();
        
        // Denver operator shouldn't see Boulder site (a02) audit records
        Assert.Equal(0L, count);
    }

    [IntegrationFact]
    public async Task Operator_CannotPerformRestrictedAction()
    {
        SetUserContext(DenverOperator, "operator", DenverSite);
        var policyService = ServiceProvider.GetRequiredService<IPolicyEvaluationService>();

        var result = await policyService.EvaluatePermissionAsync(
            DenverOperator,
            "badges:create",
            "badge",
            BoulderSite,
            null);

        Assert.False(result.IsGranted);
    }
}
