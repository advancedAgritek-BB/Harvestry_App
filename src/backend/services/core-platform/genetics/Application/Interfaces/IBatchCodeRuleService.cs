using Harvestry.Genetics.Application.DTOs;

namespace Harvestry.Genetics.Application.Interfaces;

/// <summary>
/// Service for managing batch code rules and validation
/// </summary>
public interface IBatchCodeRuleService
{
    // Code Rule Operations
    Task<BatchCodeRuleResponse> CreateRuleAsync(CreateBatchCodeRuleRequest request, Guid siteId, Guid userId, CancellationToken cancellationToken = default);
    Task<BatchCodeRuleResponse> GetRuleByIdAsync(Guid ruleId, Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchCodeRuleResponse>> GetAllRulesAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchCodeRuleResponse>> GetActiveRulesAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<BatchCodeRuleResponse> UpdateRuleAsync(Guid ruleId, UpdateBatchCodeRuleRequest request, Guid siteId, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteRuleAsync(Guid ruleId, Guid siteId, Guid userId, CancellationToken cancellationToken = default);
    Task<BatchCodeRuleResponse> ActivateRuleAsync(Guid ruleId, Guid siteId, Guid userId, CancellationToken cancellationToken = default);
    Task<BatchCodeRuleResponse> DeactivateRuleAsync(Guid ruleId, Guid siteId, Guid userId, CancellationToken cancellationToken = default);

    // Code Validation
    Task<BatchCodeValidationResponse> ValidateBatchCodeAsync(string batchCode, Guid siteId, CancellationToken cancellationToken = default);
    Task<bool> IsBatchCodeUniqueAsync(string batchCode, Guid siteId, CancellationToken cancellationToken = default);

    // Rule Management
    Task UpdateRulePrioritiesAsync(Dictionary<Guid, int> rulePriorities, Guid siteId, Guid userId, CancellationToken cancellationToken = default);
}

