using System;
using FluentValidation;
using Harvestry.Identity.API.Controllers;
using Harvestry.Identity.Domain.Enums;

namespace Harvestry.Identity.API.Validators;

public sealed class IssueBadgeRequestValidator : AbstractValidator<BadgesController.IssueBadgeRequest>
{
    public IssueBadgeRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SiteId).NotEmpty();

        RuleFor(x => x.BadgeCode)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.BadgeCode));

        RuleFor(x => x.BadgeType)
            .Must(type => Enum.IsDefined(typeof(BadgeType), type))
            .WithMessage("Badge type is invalid.");

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(_ => DateTime.UtcNow)
            .When(x => x.ExpiresAt.HasValue)
            .WithMessage("Expiration must be in the future.");
    }
}

public sealed class RevokeBadgeRequestValidator : AbstractValidator<BadgesController.RevokeBadgeRequest>
{
    public RevokeBadgeRequestValidator()
    {
        RuleFor(x => x.RevokedBy).NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(512);
    }
}
