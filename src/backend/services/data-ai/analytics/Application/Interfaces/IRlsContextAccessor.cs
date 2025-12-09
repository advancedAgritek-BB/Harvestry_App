namespace Harvestry.Analytics.Application.Interfaces;

public interface IRlsContextAccessor
{
    RlsContext Current { get; }
}

public record RlsContext(Guid UserId, string Role, Guid SiteId);
