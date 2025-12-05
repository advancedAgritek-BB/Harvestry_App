using FluentValidation;
using Harvestry.Tasks.Application.DTOs;

namespace Harvestry.Tasks.API.Validators;

public sealed class AssignTaskRequestValidator : AbstractValidator<AssignTaskRequest>
{
    public AssignTaskRequestValidator()
    {
        RuleFor(x => x)
            .Must(request => request.UserId.HasValue || !string.IsNullOrWhiteSpace(request.Role))
            .WithMessage("At least one of UserId or Role must be provided.");

        RuleFor(x => x.Role)
            .MaximumLength(100);
    }
}
