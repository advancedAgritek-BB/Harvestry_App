using FluentValidation;
using Harvestry.Genetics.Application.DTOs;

namespace Harvestry.Genetics.API.Validators;

public class SplitBatchRequestValidator : AbstractValidator<SplitBatchRequest>
{
    public SplitBatchRequestValidator()
    {
        RuleFor(x => x.PlantCountToSplit)
            .GreaterThan(0).WithMessage("Plant count to split must be greater than 0")
            .LessThanOrEqualTo(50000).WithMessage("Plant count to split cannot exceed 50,000");

        RuleFor(x => x.NewBatchName)
            .NotEmpty().WithMessage("New batch name is required")
            .MaximumLength(200).WithMessage("New batch name cannot exceed 200 characters");

        When(x => !string.IsNullOrWhiteSpace(x.SplitReason), () =>
        {
            RuleFor(x => x.SplitReason!)
                .MaximumLength(500).WithMessage("Split reason cannot exceed 500 characters");
        });
    }
}

