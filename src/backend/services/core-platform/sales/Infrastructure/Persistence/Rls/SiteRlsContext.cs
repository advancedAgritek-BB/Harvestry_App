namespace Harvestry.Sales.Infrastructure.Persistence.Rls;

public readonly record struct SiteRlsContext(Guid UserId, string Role, Guid SiteId);

