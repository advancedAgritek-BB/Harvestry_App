using System;
using FluentValidation;
using Harvestry.Identity.API.Controllers;

namespace Harvestry.Identity.API.Validators;

public sealed class BadgeLoginRequestValidator : AbstractValidator<AuthController.BadgeLoginRequest>
{
    public BadgeLoginRequestValidator()
    {
        RuleFor(request => request.BadgeCode)
            .NotEmpty()
            .Length(4, 128)
            .WithMessage("Badge code must be between 4 and 128 characters.");

        RuleFor(request => request.SiteId)
            .NotEmpty()
            .WithMessage("SiteId is required.");
    }
}

public sealed class LogoutRequestValidator : AbstractValidator<AuthController.LogoutRequest>
{
    public LogoutRequestValidator()
    {
        RuleFor(request => request.SessionId)
            .NotEmpty()
            .WithMessage("SessionId is required.");

        RuleFor(request => request.Reason)
            .MaximumLength(512)
            .WithMessage("Reason cannot exceed 512 characters.");
    }
}
