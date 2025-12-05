using FluentValidation;
using Harvestry.Tasks.Application.DTOs;

namespace Harvestry.Tasks.API.Validators;

public sealed class CancelTaskRequestValidator : AbstractValidator<CancelTaskRequest>
{
    public CancelTaskRequestValidator()
    {
        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .When(x => x.Reason is not null);
    }
}
