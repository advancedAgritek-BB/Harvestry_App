using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Labor.Domain.Entities;

public sealed class LaborBudget : AggregateRoot<Guid>
{
    private LaborBudget(Guid id) : base(id) { }

    private LaborBudget(
        Guid id,
        Guid siteId,
        string scope,
        decimal budgetAmount,
        DateOnly startDate,
        DateOnly endDate) : base(id)
    {
        SiteId = siteId;
        Scope = scope;
        BudgetAmount = budgetAmount;
        StartDate = startDate;
        EndDate = endDate;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid SiteId { get; private set; }
    public string Scope { get; private set; } = string.Empty;
    public decimal BudgetAmount { get; private set; }
    public decimal ActualAmount { get; private set; }
    public decimal Variance => ActualAmount - BudgetAmount;
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static LaborBudget Create(
        Guid siteId,
        string scope,
        decimal budgetAmount,
        DateOnly startDate,
        DateOnly endDate)
    {
        return new LaborBudget(Guid.NewGuid(), siteId, scope, budgetAmount, startDate, endDate);
    }

    public void ApplyActual(decimal amount)
    {
        ActualAmount = amount;
        UpdatedAt = DateTime.UtcNow;
    }
}


