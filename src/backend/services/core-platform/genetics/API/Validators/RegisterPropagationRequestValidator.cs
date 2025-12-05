using FluentValidation;
using Harvestry.Genetics.Application.DTOs;

namespace Harvestry.Genetics.API.Validators;

/// <summary>
/// Validator for propagation registration requests.
/// </summary>
public class RegisterPropagationRequestValidator : AbstractValidator<RegisterPropagationRequest>
{
    public RegisterPropagationRequestValidator()
    {
        RuleFor(x => x.PropagatedCount)
            .GreaterThan(0)
            .LessThanOrEqualTo(1000);
    }
}
