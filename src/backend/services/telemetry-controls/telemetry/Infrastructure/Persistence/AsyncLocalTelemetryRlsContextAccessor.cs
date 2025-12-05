using System;
using System.Threading;
using Harvestry.Telemetry.Application.Interfaces;

namespace Harvestry.Telemetry.Infrastructure.Persistence;

/// <summary>
/// AsyncLocal-backed telemetry RLS context accessor.
/// </summary>
public sealed class AsyncLocalTelemetryRlsContextAccessor : ITelemetryRlsContextAccessor
{
    private static readonly AsyncLocal<TelemetryRlsContext?> ContextHolder = new();

    public TelemetryRlsContext Current => ContextHolder.Value ?? new TelemetryRlsContext(Guid.Empty, "service_account", null);

    public void Set(TelemetryRlsContext context)
    {
        ContextHolder.Value = context;
    }

    public void Clear()
    {
        ContextHolder.Value = null;
    }
}
