using FluentValidation;
using Harvestry.Identity.API.Controllers;

namespace Harvestry.Identity.API.Validators;

public sealed class BadgeLoginRequestValidator : AbstractValidator<AuthController.BadgeLoginRequest>
{
    public BadgeLoginRequestValidator()
    {
        RuleFor(x => x.BadgeCode)
            .NotEmpty()
            .Length(4, 128);

        RuleFor(x => x.SiteId)
            .NotEmpty();
    }
}

public sealed class LogoutRequestValidator : AbstractValidator<AuthController.LogoutRequest>
{
    public LogoutRequestValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .MaximumLength(512);
    }
}
