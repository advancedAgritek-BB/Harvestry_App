using System;
using System.Security.Claims;
using Harvestry.Identity.Application.Interfaces;
using Harvestry.Shared.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Harvestry.Identity.API.Middleware;

/// <summary>
/// Middleware that populates the database RLS context based on the current HTTP request.
/// Extracts user information from JWT claims (Supabase) or headers (development fallback).
/// Falls back to the service account context when claims or headers are missing.
/// </summary>
public sealed class RlsContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RlsContextMiddleware> _logger;

    public RlsContextMiddleware(
        RequestDelegate next,
        ILogger<RlsContextMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context, IRlsContextAccessor rlsContextAccessor)
    {
        var userId = ResolveUserId(context.User);
        var role = ResolveRole(context.User);
        var siteId = ResolveSiteId(context.Request);

        var rlsContext = new RlsContext(userId, role, siteId);
        rlsContextAccessor.Set(rlsContext);

        try
        {
            await _next(context);
        }
        finally
        {
            rlsContextAccessor.Clear();
        }
    }

    private Guid ResolveUserId(ClaimsPrincipal user)
    {
        // Use the shared extension method for JWT claim extraction
        var userId = user.GetUserId();
        
        if (userId.HasValue)
        {
            return userId.Value;
        }

        _logger.LogDebug("Falling back to service account user for RLS context");
        return Guid.Empty;
    }

    private string ResolveRole(ClaimsPrincipal user)
    {
        // Use the shared extension method for JWT claim extraction
        return user.GetRole();
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
