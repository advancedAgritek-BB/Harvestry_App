using System;

namespace Harvestry.Spatial.Application.Interfaces;

/// <summary>
/// Provides the current Row-Level Security (RLS) context for spatial database operations.
/// </summary>
public interface IRlsContextAccessor
{
    RlsContext Current { get; }

    void Set(RlsContext context);

    void Clear();
}

public readonly record struct RlsContext(Guid UserId, string Role, Guid? SiteId);
