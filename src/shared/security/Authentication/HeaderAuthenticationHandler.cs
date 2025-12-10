using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Harvestry.Shared.Authentication;

/// <summary>
/// Authentication handler that validates requests using X-User-Id and X-User-Role headers.
/// This is a fallback for development environments where Supabase is not configured.
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

    public HeaderAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userIdHeader = Request.Headers["X-User-Id"].FirstOrDefault();
        var roleHeader = Request.Headers["X-User-Role"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(userIdHeader) || !Guid.TryParse(userIdHeader, out var userId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (string.IsNullOrWhiteSpace(roleHeader))
        {
            Logger.LogWarning("Missing X-User-Role header for user {UserId}", userId);
            return Task.FromResult(AuthenticateResult.Fail("Missing X-User-Role header"));
        }
        
        if (!AllowedRoles.Contains(roleHeader))
        {
            Logger.LogWarning("Invalid role '{Role}' provided for user {UserId}. Rejecting authentication.", 
                roleHeader, userId);
            return Task.FromResult(AuthenticateResult.Fail("Invalid role"));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("sub", userId.ToString()),
            new Claim(ClaimTypes.Role, roleHeader),
            new Claim("role", roleHeader)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

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







