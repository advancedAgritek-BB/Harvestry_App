using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Harvestry.Shared.Authentication;

/// <summary>
/// Middleware that sets PostgreSQL RLS context based on the authenticated user.
/// This middleware extracts user information from the JWT claims and sets
/// session-level parameters that RLS policies can use for row filtering.
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

    public async Task InvokeAsync(HttpContext context, NpgsqlDataSource dataSource)
    {
        var userId = ResolveUserId(context.User);
        var role = ResolveRole(context.User);
        var siteId = ResolveSiteId(context.Request);

        // Set RLS context if we have a valid user
        if (userId != Guid.Empty)
        {
            await SetRlsContextAsync(dataSource, userId, role, siteId);
        }

        await _next(context);
    }

    private Guid ResolveUserId(ClaimsPrincipal user)
    {
        var userId = user.GetUserId();
        
        if (userId.HasValue)
        {
            return userId.Value;
        }

        _logger.LogDebug("No user ID found in claims, falling back to service account context");
        return Guid.Empty;
    }

    private string ResolveRole(ClaimsPrincipal user)
    {
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

    private async Task SetRlsContextAsync(
        NpgsqlDataSource dataSource,
        Guid userId,
        string role,
        Guid? siteId)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        
        var commands = new List<string>
        {
            $"SELECT set_config('app.current_user_id', '{userId}', true)",
            $"SELECT set_config('app.user_role', '{role}', true)"
        };

        if (siteId.HasValue)
        {
            commands.Add($"SELECT set_config('app.current_site_id', '{siteId.Value}', true)");
        }

        foreach (var sql in commands)
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            await cmd.ExecuteNonQueryAsync();
        }

        _logger.LogDebug(
            "RLS context set: user={UserId}, role={Role}, site={SiteId}",
            userId, role, siteId);
    }
}

/// <summary>
/// Alternative middleware that uses IRlsContextAccessor for services that
/// manage their own connection lifecycle.
/// </summary>
public sealed class RlsContextAccessorMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RlsContextAccessorMiddleware> _logger;

    public RlsContextAccessorMiddleware(
        RequestDelegate next,
        ILogger<RlsContextAccessorMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context, IRlsContextAccessor rlsContextAccessor)
    {
        var userId = context.User.GetUserId() ?? Guid.Empty;
        var role = context.User.GetRole();
        var siteId = ResolveSiteId(context.Request);

        var rlsContext = new RlsContext(userId, role, siteId);
        rlsContextAccessor.Set(rlsContext);

        _logger.LogDebug(
            "RLS context accessor set: user={UserId}, role={Role}, site={SiteId}",
            userId, role, siteId);

        try
        {
            await _next(context);
        }
        finally
        {
            rlsContextAccessor.Clear();
        }
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

/// <summary>
/// Represents the current RLS context for the request.
/// </summary>
public sealed record RlsContext(Guid UserId, string Role, Guid? SiteId);

/// <summary>
/// Interface for accessing and setting the RLS context.
/// Implementations should be thread-safe and scoped to the request.
/// </summary>
public interface IRlsContextAccessor
{
    /// <summary>
    /// Gets the current RLS context.
    /// </summary>
    RlsContext? Current { get; }
    
    /// <summary>
    /// Sets the RLS context for the current scope.
    /// </summary>
    void Set(RlsContext context);
    
    /// <summary>
    /// Clears the RLS context.
    /// </summary>
    void Clear();
}

/// <summary>
/// AsyncLocal-based implementation of IRlsContextAccessor.
/// Safe for async/await patterns.
/// </summary>
public sealed class AsyncLocalRlsContextAccessor : IRlsContextAccessor
{
    private static readonly AsyncLocal<RlsContext?> _current = new();

    public RlsContext? Current => _current.Value;

    public void Set(RlsContext context)
    {
        _current.Value = context;
    }

    public void Clear()
    {
        _current.Value = null;
    }
}



