using System;

namespace Harvestry.Spatial.Application.Exceptions;

/// <summary>
/// Thrown when a requested entity belongs to a different tenant/site than indicated by the caller.
/// </summary>
public sealed class TenantMismatchException : Exception
{
    public TenantMismatchException(Guid expectedSiteId, Guid actualSiteId, string message)
        : base(message)
    {
        ExpectedSiteId = expectedSiteId;
        ActualSiteId = actualSiteId;
    }

    public Guid ExpectedSiteId { get; }

    public Guid ActualSiteId { get; }
}
