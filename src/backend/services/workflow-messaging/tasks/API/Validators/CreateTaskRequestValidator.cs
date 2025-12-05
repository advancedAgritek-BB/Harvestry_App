using FluentValidation;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.API.Validators;

public sealed class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.CustomTaskType)
            .NotEmpty()
            .When(x => x.TaskType == TaskType.Custom)
            .MaximumLength(100);

        RuleFor(x => x.AssignedToRole)
            .MaximumLength(100);

        RuleFor(x => x)
            .Must(request => !(request.AssignedToUserId.HasValue && !string.IsNullOrWhiteSpace(request.AssignedToRole)))
            .WithMessage("AssignedToUserId and AssignedToRole are mutually exclusive; provide only one.");

        RuleFor(x => x.RelatedEntityType)
            .MaximumLength(100);
    }
}
