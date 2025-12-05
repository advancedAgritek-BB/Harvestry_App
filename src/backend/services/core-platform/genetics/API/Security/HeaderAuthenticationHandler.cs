using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Harvestry.Genetics.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Harvestry.Genetics.API.Security;

/// <summary>
/// Lightweight header-based authentication used while full identity integration is wired up.
/// Validates required headers and promotes them to the ASP.NET authentication pipeline.
/// </summary>
public sealed class HeaderAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "operator",
        "supervisor",
        "manager",
        "admin",
        "service_account"
    };

    private readonly IRlsContextAccessor _rlsContextAccessor;

#pragma warning disable CS0618 // ISystemClock is marked obsolete; maintained until shared auth package lands
    public HeaderAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IRlsContextAccessor rlsContextAccessor)
        : base(options, logger, encoder, clock)
#pragma warning restore CS0618
    {
        _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userIdHeader = Request.Headers["X-User-Id"].FirstOrDefault();
        var userRoleHeader = Request.Headers["X-User-Role"].FirstOrDefault();
        var siteHeader = Request.Headers["X-Site-Id"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(userIdHeader) || !Guid.TryParse(userIdHeader, out var userId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (string.IsNullOrWhiteSpace(userRoleHeader))
        {
            Logger.LogWarning("Missing X-User-Role header for user {UserId}", userId);
            return Task.FromResult(AuthenticateResult.Fail("Missing X-User-Role header"));
        }

        if (!AllowedRoles.Contains(userRoleHeader))
        {
            Logger.LogWarning("Invalid role '{Role}' provided for user {UserId}. Rejecting authentication.", userRoleHeader, userId);
            return Task.FromResult(AuthenticateResult.Fail("Invalid role"));
        }

        Guid? siteId = null;
        if (!string.IsNullOrWhiteSpace(siteHeader) && Guid.TryParse(siteHeader, out var parsedSite))
        {
            siteId = parsedSite;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Role, userRoleHeader)
        };

        if (siteId.HasValue)
        {
            claims.Add(new Claim("site_id", siteId.Value.ToString()));
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        _rlsContextAccessor.Set(new RlsContext(userId, userRoleHeader, siteId));

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    }
}
