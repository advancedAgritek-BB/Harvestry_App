using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Application.Interfaces;
using Harvestry.Identity.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Harvestry.Identity.Tests.Integration;

[Collection("IntegrationTests")]
public sealed class BadgeLoginFlowTests : IntegrationTestBase
{
    private static readonly Guid DenverSite = Guid.Parse("00000000-0000-0000-0000-000000000a01");
    private static readonly Guid DenverOperator = Guid.Parse("00000000-0000-0000-0000-000000000101");

    public override async Task DisposeAsync()
    {
        // Clean up only test-created sessions for Denver site users
        await ExecuteSqlAsync($"DELETE FROM sessions WHERE user_id IN ('{DenverOperator}', '00000000-0000-0000-0000-000000000102');");
        await base.DisposeAsync();
    }

    [IntegrationFact]
    public async Task BadgeLogin_ApiEndpoint_ReturnsSessionToken()
    {
        await using var apiClient = await ApiClient.CreateAsync();

        var response = await apiClient.Client.PostAsJsonAsync("api/auth/badge-login", new
        {
            badgeCode = "DEN-OP-001",
            siteId = DenverSite
        });

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<BadgeLoginResponse>();
        Assert.NotNull(payload);
        var loginResponse = payload!;
        Assert.NotEqual(Guid.Empty, loginResponse.SessionId);
        Assert.Equal(DenverOperator, loginResponse.UserId);
        Assert.False(string.IsNullOrWhiteSpace(loginResponse.SessionToken));

        SetUserContext(Guid.Empty, "service_account", DenverSite);
        var badgeService = ServiceProvider.GetRequiredService<IBadgeAuthService>();
        var sessions = await badgeService.GetActiveSessionsAsync(DenverOperator);

        Assert.Contains(sessions, s => s.SessionId == loginResponse.SessionId);
    }

    [IntegrationFact]
    public async Task BadgeLogin_EndToEnd_Works()
    {
        SetUserContext(Guid.Empty, "service_account", DenverSite);
        var badgeService = ServiceProvider.GetRequiredService<IBadgeAuthService>();

        var result = await badgeService.LoginWithBadgeAsync("DEN-OP-001", DenverSite);

        Assert.True(result.Success);
        Assert.NotNull(result.SessionToken);
        Assert.Equal(DenverOperator, result.UserId);

        var sessions = await badgeService.GetActiveSessionsAsync(DenverOperator);
        Assert.Single(sessions);
    }

    [IntegrationFact]
    public async Task BadgeLogin_LockoutAfterFailedAttempts()
    {
        SetUserContext(Guid.Empty, "service_account", DenverSite);
        var badgeService = ServiceProvider.GetRequiredService<IBadgeAuthService>();
        var userRepository = ServiceProvider.GetRequiredService<IUserRepository>();

        var user = await userRepository.GetByIdAsync(DenverOperator);
        Assert.NotNull(user);
        for (var i = 0; i < 5; i++)
        {
            user!.RecordFailedLoginAttempt();
        }
        await userRepository.UpdateAsync(user!, CancellationToken.None);

        var result = await badgeService.LoginWithBadgeAsync("DEN-OP-001", DenverSite);

        Assert.False(result.Success);
        Assert.Contains("Account is temporarily locked", result.ErrorMessage);
    }

    [IntegrationFact]
    public async Task BadgeRevocation_RemovesSessions()
    {
        SetUserContext(Guid.Empty, "service_account", DenverSite);
        var badgeService = ServiceProvider.GetRequiredService<IBadgeAuthService>();
        var badgeRepository = ServiceProvider.GetRequiredService<IBadgeRepository>();

        var login = await badgeService.LoginWithBadgeAsync("DEN-MN-001", DenverSite);
        Assert.True(login.Success);

        var badge = await badgeRepository.GetByCodeAsync(BadgeCode.Create("DEN-MN-001"));
        Assert.NotNull(badge);

        var revoked = await badgeService.RevokeBadgeAsync(badge!.Id, Guid.Empty, "Compromised");
        Assert.True(revoked);

        var sessions = await badgeService.GetActiveSessionsAsync(Guid.Parse("00000000-0000-0000-0000-000000000102"));
        Assert.Empty(sessions);
    }

    [IntegrationFact]
    public async Task SessionExpires_RemovedFromActiveList()
    {
        SetUserContext(Guid.Empty, "service_account", DenverSite);
        var badgeService = ServiceProvider.GetRequiredService<IBadgeAuthService>();
        var login = await badgeService.LoginWithBadgeAsync("DEN-OP-001", DenverSite);
        Assert.True(login.Success);

        var sessionId = login.SessionId ?? throw new Xunit.Sdk.XunitException("Session not created");
        
        // Use parameterized query to avoid SQL injection
        await using var connection = await GetOpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE sessions SET expires_at = NOW() - INTERVAL '1 minute' WHERE session_id = @sessionId";
        var param = command.CreateParameter();
        param.ParameterName = "@sessionId";
        param.Value = sessionId;
        command.Parameters.Add(param);
        await command.ExecuteNonQueryAsync();

        var sessions = await badgeService.GetActiveSessionsAsync(DenverOperator);
        Assert.Empty(sessions);
    }

    [IntegrationFact]
    public async Task BadgeLogin_InvalidRequest_ReturnsProblemDetails()
    {
        await using var apiClient = await ApiClient.CreateAsync();

        var response = await apiClient.Client.PostAsJsonAsync("api/auth/badge-login", new
        {
            badgeCode = string.Empty,
            siteId = Guid.Empty
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status400BadRequest, problem!.Status);
        Assert.Equal("Validation failed", problem.Title);
        Assert.True(problem.Extensions.ContainsKey("traceId"));
    }
}

internal sealed class BadgeLoginResponse
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public string SessionToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
