using System;
using Harvestry.Spatial.Application.Exceptions;

namespace Harvestry.Spatial.Application.Common;

/// <summary>
/// Shared validation helpers for service-layer input validation.
/// </summary>
internal static class ValidationHelpers
{
    /// <summary>
    /// Ensures that a GUID value is not empty.
    /// </summary>
    /// <param name="value">The GUID value to check.</param>
    /// <param name="parameterName">The name of the parameter for error messages.</param>
    /// <exception cref="ArgumentException">Thrown if the value is <see cref="Guid.Empty"/>.</exception>
    public static void EnsureNotEmpty(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException($"{parameterName} cannot be empty", parameterName);
        }
    }

    /// <summary>
    /// Ensures that a route value matches the payload value (if the payload value is not empty).
    /// </summary>
    /// <param name="routeValue">The value from the route.</param>
    /// <param name="payloadValue">The value from the request payload.</param>
    /// <param name="propertyName">The name of the property for error messages.</param>
    /// <exception cref="ArgumentException">Thrown if the values don't match and the payload value is not empty.</exception>
    public static void EnsureRouteMatchesPayload(Guid routeValue, Guid payloadValue, string propertyName)
    {
        if (payloadValue != Guid.Empty && payloadValue != routeValue)
        {
            throw new ArgumentException($"{propertyName} mismatch between route and payload");
        }
    }

    /// <summary>
    /// Ensures that an entity belongs to the expected site.
    /// </summary>
    /// <param name="expectedSiteId">The site ID from the request route.</param>
    /// <param name="actualSiteId">The site ID the entity actually belongs to.</param>
    /// <param name="entityType">The type of entity (for error messages).</param>
    /// <param name="entityId">The ID of the entity (for error messages).</param>
    /// <exception cref="TenantMismatchException">Thrown if the site IDs don't match.</exception>
    public static void EnsureSameSite(Guid expectedSiteId, Guid actualSiteId, string entityType, Guid entityId)
    {
        if (expectedSiteId != actualSiteId)
        {
            throw new TenantMismatchException(expectedSiteId, actualSiteId,
                $"{entityType} {entityId} belongs to site {actualSiteId} but request targeted site {expectedSiteId}.");
        }
    }
}

