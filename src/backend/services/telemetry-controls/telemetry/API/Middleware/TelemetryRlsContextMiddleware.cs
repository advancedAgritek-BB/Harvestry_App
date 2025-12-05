using System;
using System.Security.Claims;
using Harvestry.Telemetry.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Harvestry.Telemetry.API.Middleware;

/// <summary>
/// Populates the telemetry RLS context for each HTTP request using claims or fallback headers.
/// </summary>
public sealed class TelemetryRlsContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITelemetryRlsContextAccessor _rlsContextAccessor;
    private readonly ILogger<TelemetryRlsContextMiddleware> _logger;

    public TelemetryRlsContextMiddleware(
        RequestDelegate next,
        ITelemetryRlsContextAccessor rlsContextAccessor,
        ILogger<TelemetryRlsContextMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var currentUser = ResolveUserId(context);
        var role = ResolveRole(context);
        var siteId = ResolveSiteId(context);

        _rlsContextAccessor.Set(new TelemetryRlsContext(currentUser, role, siteId));

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

        _logger.LogDebug("Falling back to service account user for telemetry RLS context");
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
            var headerValue = headerValues.ToString();
            if (!string.IsNullOrWhiteSpace(headerValue) && Guid.TryParse(headerValue, out var parsed))
            {
                return parsed;
            }
        }

        if (context.Request.RouteValues.TryGetValue("siteId", out var routeValue) && routeValue is string routeSite && Guid.TryParse(routeSite, out var parsedRoute))
        {
            return parsedRoute;
        }

        return null;
    }
}
