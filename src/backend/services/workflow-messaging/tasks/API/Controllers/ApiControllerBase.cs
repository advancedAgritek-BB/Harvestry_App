using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Harvestry.Tasks.API.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    private readonly ILogger<ApiControllerBase> _logger;
    private readonly bool _allowGatewayInjectedUser;

    protected ApiControllerBase(ILogger<ApiControllerBase> logger, IConfiguration configuration)
    {
        _logger = logger;
        _allowGatewayInjectedUser = configuration.GetValue<bool>("Security:AllowGatewayInjectedUser", false);
    }

    protected Guid ResolveUserId()
    {
        var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(userIdClaim) && Guid.TryParse(userIdClaim, out var claimUserId))
        {
            _logger.LogInformation("User ID resolved from authenticated principal: {UserId}, Request: {RequestId}, Path: {Path}, IP: {RemoteIp}",
                claimUserId, HttpContext.TraceIdentifier, HttpContext.Request.Path, HttpContext.Connection.RemoteIpAddress);
            return claimUserId;
        }

        // Only allow gateway-injected user ID in development/test environments with explicit configuration
        if (_allowGatewayInjectedUser && Request.Headers.TryGetValue("X-User-Id", out var headerValues))
        {
            var raw = headerValues.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(raw) && Guid.TryParse(raw, out var headerUserId) && headerUserId != Guid.Empty)
            {
                _logger.LogWarning("Gateway-injected user ID used (only allowed in configured environments): {UserId}, Request: {RequestId}, Path: {Path}, IP: {RemoteIp}",
                    headerUserId, HttpContext.TraceIdentifier, HttpContext.Request.Path, HttpContext.Connection.RemoteIpAddress);
                return headerUserId;
            }
            else
            {
                _logger.LogWarning("Invalid gateway-injected X-User-Id header attempted, Request: {RequestId}, Path: {Path}, IP: {RemoteIp}, HeaderLength: {HeaderLength}",
                    HttpContext.TraceIdentifier, HttpContext.Request.Path, HttpContext.Connection.RemoteIpAddress, raw?.Length ?? 0);
            }
        }

        _logger.LogWarning("User ID resolution failed - no authenticated principal found, Request: {RequestId}, Path: {Path}, IP: {RemoteIp}",
            HttpContext.TraceIdentifier, HttpContext.Request.Path, HttpContext.Connection.RemoteIpAddress);
        return Guid.Empty;
    }

    protected static ProblemDetails CreateProblem(string title, string detail, int status = StatusCodes.Status400BadRequest)
    {
        return new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = status
        };
    }
}
