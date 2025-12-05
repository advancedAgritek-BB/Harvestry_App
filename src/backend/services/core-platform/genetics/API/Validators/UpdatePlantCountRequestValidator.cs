using FluentValidation;
using Harvestry.Genetics.Application.DTOs;

namespace Harvestry.Genetics.API.Validators;

public class UpdatePlantCountRequestValidator : AbstractValidator<UpdatePlantCountRequest>
{
    public UpdatePlantCountRequestValidator()
    {
        RuleFor(x => x.NewPlantCount)
            .GreaterThanOrEqualTo(0).WithMessage("Plant count cannot be negative")
            .LessThanOrEqualTo(100000).WithMessage("Plant count cannot exceed 100,000");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required for plant count changes")
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");
    }
}

