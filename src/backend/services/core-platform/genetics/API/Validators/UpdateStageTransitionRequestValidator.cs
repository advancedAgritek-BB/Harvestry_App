using FluentValidation;
using Harvestry.Genetics.Application.DTOs;

namespace Harvestry.Genetics.API.Validators;

public class UpdateStageTransitionRequestValidator : AbstractValidator<UpdateStageTransitionRequest>
{
    public UpdateStageTransitionRequestValidator()
    {
        When(x => !string.IsNullOrWhiteSpace(x.ApprovalRole), () =>
        {
            RuleFor(x => x.ApprovalRole!)
                .MaximumLength(100).WithMessage("Approval role cannot exceed 100 characters");
        });
    }
}

