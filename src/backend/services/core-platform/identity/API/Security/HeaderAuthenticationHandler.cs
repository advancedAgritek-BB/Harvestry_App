using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Harvestry.Identity.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Harvestry.Identity.API.Security;

public sealed class HeaderAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IRlsContextAccessor _rlsContextAccessor;

    public HeaderAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IRlsContextAccessor rlsContextAccessor)
        : base(options, logger, encoder, clock)
    {
        _rlsContextAccessor = rlsContextAccessor;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userIdHeader = Request.Headers["X-User-Id"].FirstOrDefault();
        var roleHeader = Request.Headers["X-User-Role"].FirstOrDefault();
        var siteHeader = Request.Headers["X-Site-Id"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(userIdHeader) || !Guid.TryParse(userIdHeader, out var userId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var role = string.IsNullOrWhiteSpace(roleHeader) ? "service_account" : roleHeader;
        Guid? siteId = null;
        if (!string.IsNullOrWhiteSpace(siteHeader) && Guid.TryParse(siteHeader, out var parsedSite))
        {
            siteId = parsedSite;
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        _rlsContextAccessor.Set(new RlsContext(userId, role, siteId));

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
