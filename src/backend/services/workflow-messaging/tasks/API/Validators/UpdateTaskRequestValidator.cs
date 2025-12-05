using FluentValidation;
using Harvestry.Tasks.Application.DTOs;

namespace Harvestry.Tasks.API.Validators;

public sealed class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(500);

        RuleFor(x => x.Description)
            .MaximumLength(4000);

        RuleFor(x => x.RelatedEntityType)
            .MaximumLength(100);

        RuleFor(x => x)
            .Must(request => request.RelatedEntityType is null || request.RelatedEntityId.HasValue)
            .WithMessage("RelatedEntityId is required when RelatedEntityType is provided.");
    }
}
