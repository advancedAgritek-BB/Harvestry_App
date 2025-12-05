using FluentValidation;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.API.Validators;

public sealed class CreateConversationRequestValidator : AbstractValidator<CreateConversationRequest>
{
    public CreateConversationRequestValidator()
    {
        RuleFor(x => x.Type)
            .NotEqual(ConversationType.Undefined);

        RuleFor(x => x.Title)
            .MaximumLength(200);

        RuleFor(x => x.RelatedEntityType)
            .MaximumLength(100);

        RuleFor(x => x)
            .Must(request => request.RelatedEntityType is null || request.RelatedEntityId.HasValue)
            .WithMessage("RelatedEntityId is required when RelatedEntityType is provided.");
    }
}
