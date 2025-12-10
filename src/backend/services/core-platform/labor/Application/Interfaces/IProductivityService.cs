using Harvestry.Labor.Domain.Entities;

namespace Harvestry.Labor.Application.Interfaces;

public interface IProductivityService
{
    Task<ProductivityRecord> RecordAsync(Guid siteId, string metric, decimal value, string unit, DateTime observedAtUtc, string? referenceId, string? source, CancellationToken ct);
    Task<IEnumerable<ProductivityRecord>> GetByScopeAsync(Guid siteId, string referenceId, CancellationToken ct);
}


