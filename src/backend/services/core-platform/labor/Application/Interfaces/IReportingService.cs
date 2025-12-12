using Harvestry.Labor.Domain.Entities;

namespace Harvestry.Labor.Application.Interfaces;

public interface IReportingService
{
    Task<IEnumerable<LaborBudget>> GetBudgetRollupAsync(Guid siteId, DateOnly start, DateOnly end, CancellationToken ct);
    Task<IEnumerable<ProductivityRecord>> GetProductivityAsync(Guid siteId, DateTime fromUtc, DateTime toUtc, CancellationToken ct);
}



