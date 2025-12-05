namespace Harvestry.Genetics.Application.DTOs;

/// <summary>
/// Request to create a batch code rule
/// </summary>
public record CreateBatchCodeRuleRequest(
    string Name,
    Dictionary<string, object> RuleDefinition,
    string ResetPolicy,
    bool IsActive = true);

/// <summary>
/// Request to update a batch code rule
/// </summary>
public record UpdateBatchCodeRuleRequest(
    string Name,
    Dictionary<string, object> RuleDefinition,
    string ResetPolicy);

/// <summary>
/// Batch code rule response
/// </summary>
public record BatchCodeRuleResponse(
    Guid Id,
    Guid SiteId,
    string Name,
    Dictionary<string, object> RuleDefinition,
    string ResetPolicy,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid CreatedByUserId,
    Guid UpdatedByUserId);

/// <summary>
/// Request to validate a batch code against rules
/// </summary>
public record ValidateBatchCodeRequest(
    string BatchCode);

/// <summary>
/// Batch code validation response
/// </summary>
public record BatchCodeValidationResponse(
    bool IsValid,
    string? ErrorMessage = null,
    Guid? MatchedRuleId = null,
    string? MatchedRuleName = null);
