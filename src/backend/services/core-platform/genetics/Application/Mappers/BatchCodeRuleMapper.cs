using System;
using System.Linq;
using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Domain.Entities;

namespace Harvestry.Genetics.Application.Mappers;

/// <summary>
/// Mapper for BatchCodeRule entity and DTOs
/// </summary>
public static class BatchCodeRuleMapper
{
    /// <summary>
    /// Map BatchCodeRule entity to response DTO
    /// </summary>
    public static BatchCodeRuleResponse ToResponse(BatchCodeRule rule)
    {
        if (rule == null)
            throw new ArgumentNullException(nameof(rule));

        return new BatchCodeRuleResponse(
            Id: rule.Id,
            SiteId: rule.SiteId,
            Name: rule.Name,
            RuleDefinition: rule.RuleDefinition,
            ResetPolicy: rule.ResetPolicy,
            IsActive: rule.IsActive,
            CreatedAt: rule.CreatedAt,
            UpdatedAt: rule.UpdatedAt,
            CreatedByUserId: rule.CreatedByUserId,
            UpdatedByUserId: rule.UpdatedByUserId
        );
    }

    /// <summary>
    /// Map list of BatchCodeRule entities to response DTOs
    /// </summary>
    public static IReadOnlyList<BatchCodeRuleResponse> ToResponseList(IEnumerable<BatchCodeRule> rules)
    {
        if (rules == null)
            throw new ArgumentNullException(nameof(rules));

        return rules.Where(r => r != null).Select(ToResponse).ToArray();
    }
}
