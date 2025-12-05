using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Application.Mappers;
using Harvestry.Genetics.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Harvestry.Genetics.Application.Services;

/// <summary>
/// Service for managing batch code rules and validation
/// </summary>
public class BatchCodeRuleService : IBatchCodeRuleService
{
    private readonly IBatchCodeRuleRepository _ruleRepository;
    private readonly IBatchRepository _batchRepository;
    private readonly ILogger<BatchCodeRuleService> _logger;

    public BatchCodeRuleService(
        IBatchCodeRuleRepository ruleRepository,
        IBatchRepository batchRepository,
        ILogger<BatchCodeRuleService> logger)
    {
        _ruleRepository = ruleRepository;
        _batchRepository = batchRepository;
        _logger = logger;
    }

    // ===== Code Rule Operations =====

    public async Task<BatchCodeRuleResponse> CreateRuleAsync(CreateBatchCodeRuleRequest request, Guid siteId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating batch code rule {RuleName} for site {SiteId}", request.Name, siteId);

        // Check if rule name already exists
        var ruleNameExists = await _ruleRepository.RuleNameExistsAsync(request.Name, siteId, null, cancellationToken);
        if (ruleNameExists)
            throw new InvalidOperationException($"Rule name {request.Name} already exists");

        var rule = BatchCodeRule.Create(
            siteId: siteId,
            name: request.Name,
            ruleDefinition: request.RuleDefinition,
            resetPolicy: request.ResetPolicy,
            createdByUserId: userId,
            isActive: request.IsActive);

        var createdRule = await _ruleRepository.CreateAsync(rule, cancellationToken);

        _logger.LogInformation("Created batch code rule {RuleId}", createdRule.Id);
        return BatchCodeRuleMapper.ToResponse(createdRule);
    }

    public async Task<BatchCodeRuleResponse> GetRuleByIdAsync(Guid ruleId, Guid siteId, CancellationToken cancellationToken = default)
    {
        var rule = await _ruleRepository.GetByIdAsync(ruleId, siteId, cancellationToken);
        if (rule == null)
            throw new KeyNotFoundException($"Rule {ruleId} not found");

        return BatchCodeRuleMapper.ToResponse(rule);
    }

    public async Task<IReadOnlyList<BatchCodeRuleResponse>> GetAllRulesAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var rules = await _ruleRepository.GetAllAsync(siteId, cancellationToken);
        return BatchCodeRuleMapper.ToResponseList(rules);
    }

    public async Task<IReadOnlyList<BatchCodeRuleResponse>> GetActiveRulesAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var rules = await _ruleRepository.GetActiveAsync(siteId, cancellationToken);
        return BatchCodeRuleMapper.ToResponseList(rules);
    }

    public async Task<BatchCodeRuleResponse> UpdateRuleAsync(Guid ruleId, UpdateBatchCodeRuleRequest request, Guid siteId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating batch code rule {RuleId}", ruleId);

        var rule = await _ruleRepository.GetByIdAsync(ruleId, siteId, cancellationToken);
        if (rule == null)
            throw new KeyNotFoundException($"Rule {ruleId} not found");

        // Check if new rule name conflicts with existing rule
        var ruleNameExists = await _ruleRepository.RuleNameExistsAsync(request.Name, siteId, ruleId, cancellationToken);
        if (ruleNameExists)
            throw new InvalidOperationException($"Rule name {request.Name} already exists");

        rule.Update(
            name: request.Name,
            ruleDefinition: request.RuleDefinition,
            resetPolicy: request.ResetPolicy,
            updatedByUserId: userId);

        var updatedRule = await _ruleRepository.UpdateAsync(rule, cancellationToken);

        _logger.LogInformation("Updated batch code rule {RuleId}", ruleId);
        return BatchCodeRuleMapper.ToResponse(updatedRule);
    }

    public async Task DeleteRuleAsync(Guid ruleId, Guid siteId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting batch code rule {RuleId}", ruleId);

        var rule = await _ruleRepository.GetByIdAsync(ruleId, siteId, cancellationToken);
        if (rule == null)
            throw new KeyNotFoundException($"Rule {ruleId} not found");

        await _ruleRepository.DeleteAsync(ruleId, siteId, cancellationToken);
        _logger.LogInformation("Deleted batch code rule {RuleId}", ruleId);
    }

    public async Task<BatchCodeRuleResponse> ActivateRuleAsync(Guid ruleId, Guid siteId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Activating batch code rule {RuleId}", ruleId);

        var rule = await _ruleRepository.GetByIdAsync(ruleId, siteId, cancellationToken);
        if (rule == null)
            throw new KeyNotFoundException($"Rule {ruleId} not found");

        rule.Activate(userId);

        var updatedRule = await _ruleRepository.UpdateAsync(rule, cancellationToken);

        _logger.LogInformation("Activated batch code rule {RuleId}", ruleId);
        return BatchCodeRuleMapper.ToResponse(updatedRule);
    }

    public async Task<BatchCodeRuleResponse> DeactivateRuleAsync(Guid ruleId, Guid siteId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deactivating batch code rule {RuleId}", ruleId);

        var rule = await _ruleRepository.GetByIdAsync(ruleId, siteId, cancellationToken);
        if (rule == null)
            throw new KeyNotFoundException($"Rule {ruleId} not found");

        rule.Deactivate(userId);

        var updatedRule = await _ruleRepository.UpdateAsync(rule, cancellationToken);

        _logger.LogInformation("Deactivated batch code rule {RuleId}", ruleId);
        return BatchCodeRuleMapper.ToResponse(updatedRule);
    }

    // ===== Code Validation =====

    public async Task<BatchCodeValidationResponse> ValidateBatchCodeAsync(string batchCode, Guid siteId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Validating batch code {BatchCode} for site {SiteId}", batchCode, siteId);

        if (string.IsNullOrWhiteSpace(batchCode))
            return new BatchCodeValidationResponse(false, "Batch code cannot be empty");

        // Get active rules ordered by priority
        var rules = await _ruleRepository.GetOrderedByPriorityAsync(siteId, cancellationToken);

        if (rules.Count == 0)
        {
            // No rules defined - perform basic validation only
            if (batchCode.Length > 50)
                return new BatchCodeValidationResponse(false, "Batch code cannot exceed 50 characters");

            return new BatchCodeValidationResponse(true);
        }

        // Validate against rules in priority order
        foreach (var rule in rules)
        {
            if (!rule.IsActive)
                continue;

            // Extract pattern from rule definition
            if (!rule.RuleDefinition.TryGetValue("format", out var formatObj))
                continue;

            var format = formatObj?.ToString();
            if (string.IsNullOrWhiteSpace(format))
                continue;

            // Convert format to regex pattern
            // Example: "{SitePrefix}-{StrainCode}-{YYMMDD}-{Seq}" -> "^.+-.+-.+-.+$"
            // This is a simplified implementation - you may want to make this more robust
            var regexPattern = ConvertFormatToRegex(format);

            try
            {
                if (Regex.IsMatch(batchCode, regexPattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1)))
                {
                    _logger.LogDebug("Batch code {BatchCode} matched rule {RuleName}", batchCode, rule.Name);
                    return new BatchCodeValidationResponse(
                        IsValid: true,
                        MatchedRuleId: rule.Id,
                        MatchedRuleName: rule.Name);
                }
            }
            catch (RegexMatchTimeoutException)
            {
                _logger.LogWarning("Regex timeout while validating batch code {BatchCode} against rule {RuleName}", batchCode, rule.Name);
                continue;
            }
        }

        return new BatchCodeValidationResponse(false, "Batch code does not match any active rules");
    }

    public async Task<bool> IsBatchCodeUniqueAsync(string batchCode, Guid siteId, CancellationToken cancellationToken = default)
    {
        var exists = await _batchRepository.BatchCodeExistsAsync(batchCode, siteId, cancellationToken);
        return !exists;
    }

    // ===== Rule Management =====

    public async Task UpdateRulePrioritiesAsync(Dictionary<Guid, int> rulePriorities, Guid siteId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating priorities for {Count} rules", rulePriorities.Count);

        // Note: The BatchCodeRule entity doesn't have a Priority property yet.
        // This would need to be added to the entity if priority management is needed.
        // For now, this is a placeholder implementation.

        _logger.LogWarning("Rule priority management not yet implemented - requires Priority property on BatchCodeRule entity");

        // TODO: Add Priority property to BatchCodeRule entity, then implement:
        // foreach (var (ruleId, priority) in rulePriorities)
        // {
        //     var rule = await _ruleRepository.GetByIdAsync(ruleId, siteId, cancellationToken);
        //     if (rule != null)
        //     {
        //         rule.UpdatePriority(priority, userId);
        //         await _ruleRepository.UpdateAsync(rule, cancellationToken);
        //     }
        // }

        await Task.CompletedTask;
    }

    // ===== Private Helper Methods =====

    private static string ConvertFormatToRegex(string format)
    {
        // Convert format placeholders to regex patterns
        // Example: "{SitePrefix}-{StrainCode}-{YYMMDD}-{Seq}" -> "^[A-Z0-9]+-[A-Z0-9]+-[0-9]{6}-[0-9]+$"
        
        var pattern = format;

        // Replace common placeholders with regex patterns
        pattern = Regex.Replace(pattern, @"\{SitePrefix\}", "[A-Z0-9]+", RegexOptions.IgnoreCase);
        pattern = Regex.Replace(pattern, @"\{StrainCode\}", "[A-Z0-9]+", RegexOptions.IgnoreCase);
        pattern = Regex.Replace(pattern, @"\{YYMMDD\}", "[0-9]{6}");
        pattern = Regex.Replace(pattern, @"\{YYYYMMDD\}", "[0-9]{8}");
        pattern = Regex.Replace(pattern, @"\{Seq\}", "[0-9]+");
        pattern = Regex.Replace(pattern, @"\{[^}]+\}", ".+"); // Generic placeholder

        // Escape special regex characters that might be in the format
        pattern = pattern.Replace("(", "\\(").Replace(")", "\\)");

        // Anchor the pattern
        return "^" + pattern + "$";
    }
}

