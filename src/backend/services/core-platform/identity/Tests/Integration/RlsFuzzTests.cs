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

    [Fact]
    public async Task Operator_CanReadOwnRecord()
    {
        SetUserContext(DenverOperator, "operator", DenverSite);
        var repository = ServiceProvider.GetRequiredService<IUserRepository>();

        var user = await repository.GetByIdAsync(DenverOperator);

        Assert.NotNull(user);
        Assert.Equal(DenverOperator, user!.Id);
    }

    [Fact]
    public async Task Operator_CannotReadOtherSiteRecord()
    {
        SetUserContext(DenverOperator, "operator", DenverSite);
        var repository = ServiceProvider.GetRequiredService<IUserRepository>();

        var user = await repository.GetByIdAsync(BoulderOperator);

        Assert.Null(user);
    }

    [Fact]
    public async Task Admin_CanReadOtherSiteRecord()
    {
        SetUserContext(DenverAdmin, "admin", DenverSite);
        var repository = ServiceProvider.GetRequiredService<IUserRepository>();

        var user = await repository.GetByIdAsync(BoulderOperator);

        Assert.NotNull(user);
        Assert.Equal(BoulderOperator, user!.Id);
    }

    [Fact]
    public async Task ServiceAccount_BypassesRls()
    {
        SetUserContext(Guid.Empty, "service_account", BoulderSite);
        var badges = ServiceProvider.GetRequiredService<IBadgeRepository>();

        var results = await badges.GetActiveBySiteIdAsync(DenverSite);

        Assert.NotEmpty(results);
    }

    [Fact]
    public async Task Operator_CanReadOwnSiteBadges()
    {
        SetUserContext(DenverOperator, "operator", DenverSite);
        var badges = ServiceProvider.GetRequiredService<IBadgeRepository>();

        var results = await badges.GetActiveBySiteIdAsync(DenverSite);

        Assert.NotEmpty(results);
    }

    [Fact]
    public async Task Operator_CannotReadOtherSiteBadges()
    {
        SetUserContext(DenverOperator, "operator", DenverSite);
        var badges = ServiceProvider.GetRequiredService<IBadgeRepository>();

        var results = await badges.GetActiveBySiteIdAsync(BoulderSite);

        Assert.Empty(results);
    }

    [Fact]
    public async Task SessionWithoutContext_Fails()
    {
        SetUserContext(DenverOperator, "operator", BoulderSite);
        var repository = ServiceProvider.GetRequiredService<IUserRepository>();

        var user = await repository.GetByIdAsync(DenverOperator);

        Assert.Null(user);
    }

    [Fact]
    public async Task Operator_CannotRevokeOtherSiteBadge()
    {
        SetUserContext(DenverOperator, "operator", DenverSite);
        var badgeService = ServiceProvider.GetRequiredService<IBadgeAuthService>();

        var result = await badgeService.RevokeBadgeAsync(Guid.Parse("00000000-0000-0000-0000-00000000b003"), Guid.Empty, "Test");

        Assert.False(result);
    }

    [Fact]
    public async Task AuthorizationAudit_CrossSiteBlocked()
    {
        SetUserContext(DenverOperator, "operator", DenverSite);
        var db = ServiceProvider.GetRequiredService<IdentityDbContext>();
        var connection = await db.GetOpenConnectionAsync();

        await Assert.ThrowsAsync<NpgsqlException>(async () =>
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM authorization_audit";
            await command.ExecuteScalarAsync();
        });
    }

    [Fact]
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
