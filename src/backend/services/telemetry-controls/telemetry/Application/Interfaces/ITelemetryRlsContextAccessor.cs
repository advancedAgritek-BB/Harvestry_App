using System;

namespace Harvestry.Telemetry.Application.Interfaces;

/// <summary>
/// Provides access to the current Row-Level Security (RLS) context for telemetry data operations.
/// </summary>
public interface ITelemetryRlsContextAccessor
{
    TelemetryRlsContext Current { get; }

    void Set(TelemetryRlsContext context);

    void Clear();
}

public readonly record struct TelemetryRlsContext
{
    public TelemetryRlsContext(Guid userId, string role, Guid? siteId)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("Role cannot be null, empty, or whitespace.", nameof(role));
        }

        UserId = userId;
        Role = role;
        SiteId = siteId;
    }

    public Guid UserId { get; init; }

    public string Role { get; init; }

    public Guid? SiteId { get; init; }
}
