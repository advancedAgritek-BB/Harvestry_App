using Microsoft.AspNetCore.Http;

namespace Harvestry.Sales.Infrastructure.Persistence.Rls;

public interface ISiteRlsContextAccessor
{
    SiteRlsContext Current { get; }
}

public sealed class HttpSiteRlsContextAccessor : ISiteRlsContextAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpSiteRlsContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public SiteRlsContext Current
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return new SiteRlsContext(Guid.Empty, "service_account", Guid.Empty);
            }

            var siteId = TryGetGuidHeader(httpContext, "X-Site-Id") ?? Guid.Empty;
            var userId = TryGetGuidHeader(httpContext, "X-User-Id") ?? Guid.Empty;
            var role = TryGetHeader(httpContext, "X-Role") ?? "service_account";

            return new SiteRlsContext(userId, NormalizeRole(role), siteId);
        }
    }

    private static Guid? TryGetGuidHeader(HttpContext context, string name)
    {
        var value = TryGetHeader(context, name);
        return Guid.TryParse(value, out var parsed) ? parsed : null;
    }

    private static string? TryGetHeader(HttpContext context, string name)
    {
        return context.Request.Headers.TryGetValue(name, out var values)
            ? values.ToString()
            : null;
    }

    private static string NormalizeRole(string role)
        => string.IsNullOrWhiteSpace(role) ? "service_account" : role.Trim().ToLowerInvariant();
}

