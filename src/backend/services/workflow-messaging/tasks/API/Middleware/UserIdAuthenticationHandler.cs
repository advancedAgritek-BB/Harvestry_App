using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Harvestry.Tasks.API.Middleware;

public class UserIdAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ILogger<UserIdAuthenticationHandler> _logger;

    public UserIdAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
        _logger = logger.CreateLogger<UserIdAuthenticationHandler>();
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var requestId = Context.TraceIdentifier;
        var path = Context.Request.Path;
        var remoteIp = Context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (!Context.Request.Headers.TryGetValue("X-User-Id", out var userIdValues))
        {
            _logger.LogWarning("X-User-Id header missing from request. Scheme: {Scheme}, Request: {RequestId}, Path: {Path}, RemoteIP: {RemoteIp}",
                Scheme.Name, requestId, path, remoteIp);
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var userIdString = userIdValues.FirstOrDefault();
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId) || userId == Guid.Empty)
        {
            _logger.LogWarning("Invalid X-User-Id header value. Length: {HeaderLength}, Scheme: {Scheme}, Request: {RequestId}, Path: {Path}, RemoteIP: {RemoteIp}",
                userIdString?.Length ?? 0, Scheme.Name, requestId, path, remoteIp);
            return Task.FromResult(AuthenticateResult.Fail("Invalid X-User-Id header"));
        }

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        _logger.LogInformation("User authentication successful. UserId: {UserId}, Scheme: {Scheme}, Request: {RequestId}, Path: {Path}, RemoteIP: {RemoteIp}",
            userId, Scheme.Name, requestId, path, remoteIp);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
