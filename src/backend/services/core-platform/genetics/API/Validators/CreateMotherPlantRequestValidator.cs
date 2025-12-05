using FluentValidation;
using Harvestry.Genetics.Application.DTOs;

namespace Harvestry.Genetics.API.Validators;

/// <summary>
/// Validator for creating mother plants.
/// </summary>
public class CreateMotherPlantRequestValidator : AbstractValidator<CreateMotherPlantRequest>
{
    public CreateMotherPlantRequestValidator()
    {
        RuleFor(x => x.BatchId)
            .NotEmpty();

        RuleFor(x => x.StrainId)
            .NotEmpty();

        RuleFor(x => x.PlantTag)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.DateEstablished)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));

        RuleFor(x => x.MaxPropagationCount)
            .GreaterThan(0)
            .When(x => x.MaxPropagationCount.HasValue);
    }
}
