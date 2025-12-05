using FluentValidation;
using Harvestry.Tasks.Application.DTOs;

namespace Harvestry.Tasks.API.Validators;

public sealed class ModifyWatcherRequestValidator : AbstractValidator<ModifyWatcherRequest>
{
    public ModifyWatcherRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId must be a valid, non-empty GUID.");
    }
}
