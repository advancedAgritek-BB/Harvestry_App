using System;

namespace Harvestry.Identity.Application.Interfaces;

/// <summary>
/// Provides the current Row-Level Security (RLS) context for database operations.
/// </summary>
public interface IRlsContextAccessor
{
    /// <summary>
    /// Gets the current RLS context. Implementations should never return a default value.
    /// </summary>
    RlsContext Current { get; }

    /// <summary>
    /// Set the active RLS context for the current async flow.
    /// </summary>
    void Set(RlsContext context);

    /// <summary>
    /// Clear the active RLS context so the fallback is used.
    /// </summary>
    void Clear();
}

/// <summary>
/// Represents the values that must be applied to PostgreSQL session variables
/// before executing queries guarded by Row-Level Security policies.
/// </summary>
/// <param name="UserId">Authenticated user identifier or a service account identifier.</param>
/// <param name="Role">Effective role name (e.g., "admin", "service_account").</param>
/// <param name="SiteId">Optional site scope. Use the active site for site-scoped tables.</param>
public readonly record struct RlsContext(Guid UserId, string Role, Guid? SiteId);
