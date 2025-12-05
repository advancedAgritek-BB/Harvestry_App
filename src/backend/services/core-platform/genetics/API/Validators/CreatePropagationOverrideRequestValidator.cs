using FluentValidation;
using Harvestry.Genetics.Application.DTOs;

namespace Harvestry.Genetics.API.Validators;

/// <summary>
/// Validator for creating propagation override requests.
/// </summary>
public class CreatePropagationOverrideRequestValidator : AbstractValidator<CreatePropagationOverrideRequest>
{
    public CreatePropagationOverrideRequestValidator()
    {
        RuleFor(x => x.RequestedQuantity)
            .GreaterThan(0)
            .LessThanOrEqualTo(5000);

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(x => x)
            .Must(HasMotherOrBatch)
            .WithMessage("A mother plant or batch identifier must be supplied when requesting an override.");
    }

    private static bool HasMotherOrBatch(CreatePropagationOverrideRequest request)
    {
        return request.MotherPlantId.HasValue || request.BatchId.HasValue;
    }
}
