using System;
using FluentValidation;
using Harvestry.Identity.API.Controllers;

namespace Harvestry.Identity.API.Validators;

public sealed class IssueBadgeRequestValidator : AbstractValidator<BadgesController.IssueBadgeRequest>
{
    public IssueBadgeRequestValidator()
    {
        RuleFor(request => request.UserId)
            .NotEmpty();

        RuleFor(request => request.SiteId)
            .NotEmpty();

        RuleFor(request => request.BadgeCode)
            .Cascade(CascadeMode.Stop)
            .Length(4, 100)
            .When(request => !string.IsNullOrWhiteSpace(request.BadgeCode));

        RuleFor(request => request.BadgeType)
            .IsInEnum();

        RuleFor(request => request.ExpiresAt)
            .Must(expiration => expiration == null || expiration > DateTime.UtcNow)
            .WithMessage("Expiration must be in the future.")
            .When(request => request.ExpiresAt.HasValue);
    }
}

public sealed class RevokeBadgeRequestValidator : AbstractValidator<BadgesController.RevokeBadgeRequest>
{
    public RevokeBadgeRequestValidator()
    {
        RuleFor(request => request.RevokedBy)
            .NotEmpty();

        RuleFor(request => request.Reason)
            .NotEmpty()
            .Length(3, 512);
    }
}
