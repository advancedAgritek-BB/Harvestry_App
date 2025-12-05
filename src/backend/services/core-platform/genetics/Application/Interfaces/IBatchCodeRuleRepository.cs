using Harvestry.Genetics.Domain.Entities;

namespace Harvestry.Genetics.Application.Interfaces;

/// <summary>
/// Repository interface for BatchCodeRule operations
/// </summary>
public interface IBatchCodeRuleRepository
{
    Task<BatchCodeRule> CreateAsync(BatchCodeRule rule, CancellationToken cancellationToken = default);
    Task<BatchCodeRule?> GetByIdAsync(Guid ruleId, Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchCodeRule>> GetAllAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchCodeRule>> GetActiveAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchCodeRule>> GetOrderedByPriorityAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<BatchCodeRule> UpdateAsync(BatchCodeRule rule, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid ruleId, Guid siteId, CancellationToken cancellationToken = default);
    Task<bool> RuleNameExistsAsync(string ruleName, Guid siteId, Guid? excludeRuleId = null, CancellationToken cancellationToken = default);
}

