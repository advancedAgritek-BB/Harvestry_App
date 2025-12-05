using FluentValidation;
using Harvestry.Genetics.Application.DTOs;

namespace Harvestry.Genetics.API.Validators;

public class CreateStageTransitionRequestValidator : AbstractValidator<CreateStageTransitionRequest>
{
    public CreateStageTransitionRequestValidator()
    {
        RuleFor(x => x.FromStageId)
            .NotEmpty().WithMessage("From stage ID is required");

        RuleFor(x => x.ToStageId)
            .NotEmpty().WithMessage("To stage ID is required");

        RuleFor(x => x)
            .Must(x => x.FromStageId != x.ToStageId)
            .WithMessage("From stage and to stage cannot be the same");

        When(x => !string.IsNullOrWhiteSpace(x.ApprovalRole), () =>
        {
            RuleFor(x => x.ApprovalRole!)
                .MaximumLength(100).WithMessage("Approval role cannot exceed 100 characters");
        });
    }
}

