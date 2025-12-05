using FluentValidation;
using Harvestry.Tasks.Application.DTOs;

namespace Harvestry.Tasks.API.Validators;

public sealed class SlackWorkspaceRequestValidator : AbstractValidator<SlackWorkspaceRequest>
{
    public SlackWorkspaceRequestValidator()
    {
        RuleFor(x => x.WorkspaceId)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.WorkspaceName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.BotToken)
            .MaximumLength(4000);

        RuleFor(x => x.RefreshToken)
            .MaximumLength(4000);
    }
}
