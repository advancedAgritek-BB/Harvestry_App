using FluentValidation;
using Harvestry.Genetics.Application.DTOs;

namespace Harvestry.Genetics.API.Validators;

/// <summary>
/// Validator for propagation override decision requests.
/// </summary>
public class PropagationOverrideDecisionRequestValidator : AbstractValidator<PropagationOverrideDecisionRequest>
{
    public PropagationOverrideDecisionRequestValidator()
    {
        RuleFor(x => x.Decision)
            .IsInEnum();

        RuleFor(x => x.Notes)
            .NotEmpty()
            .When(x => x.Decision == PropagationOverrideDecision.Reject)
            .WithMessage("A rejection reason is required when rejecting an override request.");
    }
}
