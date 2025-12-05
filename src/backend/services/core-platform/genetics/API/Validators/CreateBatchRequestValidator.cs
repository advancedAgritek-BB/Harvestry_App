using FluentValidation;
using Harvestry.Genetics.Application.DTOs;

namespace Harvestry.Genetics.API.Validators;

public class CreateBatchRequestValidator : AbstractValidator<CreateBatchRequest>
{
    public CreateBatchRequestValidator()
    {
        RuleFor(x => x.StrainId)
            .NotEmpty().WithMessage("Strain ID is required");

        RuleFor(x => x.BatchCode)
            .NotEmpty().WithMessage("Batch code is required")
            .MaximumLength(50).WithMessage("Batch code cannot exceed 50 characters");

        RuleFor(x => x.BatchName)
            .NotEmpty().WithMessage("Batch name is required")
            .MaximumLength(200).WithMessage("Batch name cannot exceed 200 characters");

        RuleFor(x => x.PlantCount)
            .GreaterThan(0).WithMessage("Plant count must be greater than 0")
            .LessThanOrEqualTo(100000).WithMessage("Plant count cannot exceed 100,000");

        RuleFor(x => x.CurrentStageId)
            .NotEmpty().WithMessage("Current stage ID is required");

        RuleFor(x => x.Generation)
            .GreaterThan(0).WithMessage("Generation must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Generation cannot exceed 100");

        When(x => x.TargetPlantCount.HasValue, () =>
        {
            RuleFor(x => x.TargetPlantCount!.Value)
                .GreaterThan(0).WithMessage("Target plant count must be greater than 0")
                .LessThanOrEqualTo(100000).WithMessage("Target plant count cannot exceed 100,000");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Notes), () =>
        {
            RuleFor(x => x.Notes!)
                .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters");
        });
    }
}

