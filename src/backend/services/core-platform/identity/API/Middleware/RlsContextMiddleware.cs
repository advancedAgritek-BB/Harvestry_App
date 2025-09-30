using System;
using System.Security.Claims;
using Harvestry.Identity.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Harvestry.Identity.API.Middleware;

/// <summary>
/// Middleware that populates the database RLS context based on the current HTTP request.
/// For now it falls back to the service account context when claims or headers are missing.
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
        var userId = ResolveUserId(context.User);
        var role = ResolveRole(context.User);
        var siteId = ResolveSiteId(context.Request);

        var rlsContext = new RlsContext(userId, role, siteId);
        _rlsContextAccessor.Set(rlsContext);

        try
        {
            await _next(context);
        }
        finally
        {
            _rlsContextAccessor.Clear();
        }
    }

    private Guid ResolveUserId(ClaimsPrincipal user)
    {
        var value = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user?.FindFirst("sub")?.Value;

        if (!string.IsNullOrWhiteSpace(value) && Guid.TryParse(value, out var parsed))
        {
            return parsed;
        }

        _logger.LogDebug("Falling back to service account user for RLS context");
        return Guid.Empty;
    }

    private string ResolveRole(ClaimsPrincipal user)
    {
        var value = user?.FindFirst(ClaimTypes.Role)?.Value
            ?? user?.FindFirst("role")?.Value;

        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return "service_account";
    }

    private Guid? ResolveSiteId(HttpRequest request)
    {
        if (request.Headers.TryGetValue("X-Site-Id", out var headerValues))
        {
            var candidate = headerValues.ToString();
            if (!string.IsNullOrWhiteSpace(candidate) && Guid.TryParse(candidate, out var parsed))
            {
                return parsed;
            }
        }

        return null;
    }
}
