using System;
using System.Security.Claims;
using Harvestry.Spatial.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Harvestry.Spatial.API.Middleware;

/// <summary>
/// Populates the spatial RLS context for each HTTP request using claims or fallback headers.
/// </summary>
public sealed class RlsContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRlsContextAccessor _rlsContextAccessor;
    private readonly ILogger<RlsContextMiddleware> _logger;

    public RlsContextMiddleware(
        RequestDelegate next,
        IRlsContextAccessor rlsContextAccessor,
        ILogger<RlsContextMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var userId = ResolveUserId(context);
        var role = ResolveRole(context);
        var siteId = ResolveSiteId(context);

        _rlsContextAccessor.Set(new RlsContext(userId, role, siteId));

        try
        {
            await _next(context);
        }
        finally
        {
            _rlsContextAccessor.Clear();
        }
    }

    private Guid ResolveUserId(HttpContext context)
    {
        var principal = context.User;
        var value = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? principal?.FindFirst("sub")?.Value
                    ?? principal?.FindFirst("oid")?.Value;

        if (!string.IsNullOrWhiteSpace(value) && Guid.TryParse(value, out var parsed))
        {
            return parsed;
        }

        if (context.Request.Headers.TryGetValue("X-User-Id", out var headerValues))
        {
            var headerValue = headerValues.ToString();
            if (!string.IsNullOrWhiteSpace(headerValue) && Guid.TryParse(headerValue, out parsed))
            {
                return parsed;
            }
        }

        _logger.LogDebug("Falling back to service account user for spatial RLS context");
        return Guid.Empty;
    }

    private string ResolveRole(HttpContext context)
    {
        var principal = context.User;
        var value = principal?.FindFirst(ClaimTypes.Role)?.Value
                    ?? principal?.FindFirst("role")?.Value;

        if (string.IsNullOrWhiteSpace(value) && context.Request.Headers.TryGetValue("X-Role", out var headerValues))
        {
            value = headerValues.ToString();
        }

        return string.IsNullOrWhiteSpace(value) ? "service_account" : value;
    }

    private Guid? ResolveSiteId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Site-Id", out var headerValues))
        {
            var candidate = headerValues.ToString();
            if (!string.IsNullOrWhiteSpace(candidate) && Guid.TryParse(candidate, out var parsed))
            {
                return parsed;
            }
        }

        if (context.Request.RouteValues.TryGetValue("siteId", out var routeSite) && routeSite is string routeValue && Guid.TryParse(routeValue, out var parsedSite))
        {
            return parsedSite;
        }

        return null;
    }
}
