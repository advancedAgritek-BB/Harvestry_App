using FluentValidation;
using Harvestry.Genetics.Application.DTOs;

namespace Harvestry.Genetics.API.Validators;

public class CreateBatchStageRequestValidator : AbstractValidator<CreateBatchStageRequest>
{
    public CreateBatchStageRequestValidator()
    {
        RuleFor(x => x.StageKey)
            .NotNull().WithMessage("Stage key is required");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required")
            .MaximumLength(100).WithMessage("Display name cannot exceed 100 characters");

        RuleFor(x => x.SequenceOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sequence order cannot be negative")
            .LessThan(1000).WithMessage("Sequence order cannot exceed 999");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Description))
            .WithMessage("Description cannot exceed 1000 characters");
    }
}
