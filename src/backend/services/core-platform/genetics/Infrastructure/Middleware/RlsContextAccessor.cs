using Harvestry.Genetics.Application.Interfaces;

namespace Harvestry.Genetics.Infrastructure.Middleware;

/// <summary>
/// Thread-safe RLS context accessor using AsyncLocal storage
/// </summary>
public sealed class RlsContextAccessor : IRlsContextAccessor
{
    private static readonly AsyncLocal<RlsContext> _current = new();

    public RlsContext Current => _current.Value;

    public void Set(RlsContext context)
    {
        _current.Value = context;
    }

    public void Clear()
    {
        _current.Value = default;
    }
}

