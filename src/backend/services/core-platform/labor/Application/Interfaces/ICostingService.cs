using Harvestry.Labor.Domain.Entities;

namespace Harvestry.Labor.Application.Interfaces;

public interface ICostingService
{
    Task<LaborBudget> CreateBudgetAsync(Guid siteId, string scope, decimal budgetAmount, DateOnly start, DateOnly end, CancellationToken ct);
    Task ApplyActualsAsync(Guid budgetId, decimal actualAmount, CancellationToken ct);
    Task<decimal> CalculateCostAsync(Guid timeEntryId, CancellationToken ct);
}


