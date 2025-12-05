using FluentValidation;
using Harvestry.Genetics.Application.DTOs;

namespace Harvestry.Genetics.API.Validators;

public class TransitionBatchStageRequestValidator : AbstractValidator<TransitionBatchStageRequest>
{
    public TransitionBatchStageRequestValidator()
    {
        RuleFor(x => x.NewStageId)
            .NotEmpty().WithMessage("New stage ID is required");

        When(x => !string.IsNullOrWhiteSpace(x.TransitionNotes), () =>
        {
            RuleFor(x => x.TransitionNotes!)
                .MaximumLength(1000).WithMessage("Transition notes cannot exceed 1000 characters");
        });
    }
}

