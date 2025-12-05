using FluentValidation;
using Harvestry.Genetics.Application.DTOs;

namespace Harvestry.Genetics.API.Validators;

public class UpdateBatchCodeRuleRequestValidator : AbstractValidator<UpdateBatchCodeRuleRequest>
{
    private static readonly string[] ValidResetPolicies = { "never", "annual", "monthly", "per_harvest" };

    public UpdateBatchCodeRuleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Rule name is required")
            .MaximumLength(100).WithMessage("Rule name cannot exceed 100 characters");

        RuleFor(x => x.RuleDefinition)
            .NotEmpty().WithMessage("Rule definition is required")
            .Must(dict => dict != null && dict.Count > 0).WithMessage("Rule definition cannot be empty");

        RuleFor(x => x.ResetPolicy)
            .NotEmpty().WithMessage("Reset policy is required")
            .Must(policy => !string.IsNullOrEmpty(policy) && ValidResetPolicies.Contains(policy.ToLowerInvariant()))
            .WithMessage($"Reset policy must be one of: {string.Join(", ", ValidResetPolicies)}");
    }
}

