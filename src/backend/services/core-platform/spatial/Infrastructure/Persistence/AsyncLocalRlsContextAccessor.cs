using System;
using System.Threading;
using Harvestry.Spatial.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Harvestry.Spatial.Infrastructure.Persistence;

/// <summary>
/// Stores the RLS context in an AsyncLocal so each async flow has isolated values.
/// </summary>
public sealed class AsyncLocalRlsContextAccessor : IRlsContextAccessor
{
    private static readonly AsyncLocal<RlsContext?> CurrentContext = new();
    private readonly ILogger<AsyncLocalRlsContextAccessor> _logger;
    private readonly RlsContext _fallback = new(Guid.Empty, "service_account", null);

    public AsyncLocalRlsContextAccessor(ILogger<AsyncLocalRlsContextAccessor> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public RlsContext Current => CurrentContext.Value ?? _fallback;

    public void Set(RlsContext context)
    {
        CurrentContext.Value = context;
        _logger.LogDebug("Spatial RLS context set: user={UserId}, role={Role}, site={SiteId}", context.UserId, context.Role, context.SiteId);
    }

    public void Clear()
    {
        CurrentContext.Value = null;
        _logger.LogDebug("Spatial RLS context cleared");
    }
}
