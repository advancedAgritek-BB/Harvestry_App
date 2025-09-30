using System;
using System.Text.RegularExpressions;
using FluentValidation;
using Harvestry.Identity.API.Controllers;

namespace Harvestry.Identity.API.Validators;

public sealed class CreateUserRequestValidator : AbstractValidator<UsersController.CreateUserRequest>
{
    private const string PhonePattern = @"^\+?[1-9]\d{0,14}$";

    public CreateUserRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .MaximumLength(255)
            .EmailAddress();

        RuleFor(request => request.FirstName)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(request => request.LastName)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(request => request.PhoneNumber)
            .Cascade(CascadeMode.Stop)
            .Must(phone => string.IsNullOrWhiteSpace(phone) || Regex.IsMatch(phone.Trim(), PhonePattern))
            .WithMessage("Phone number must be in E.164 format (max 15 digits).");

        When(request => request.SiteAssignments is { Length: > 0 }, () =>
        {
            RuleForEach(request => request.SiteAssignments!)
                .SetValidator(new SiteAssignmentRequestValidator());
        });
    }
}

public sealed class SiteAssignmentRequestValidator : AbstractValidator<UsersController.SiteAssignmentRequest>
{
    public SiteAssignmentRequestValidator()
    {
        RuleFor(request => request.SiteId)
            .NotEmpty();

        RuleFor(request => request.RoleId)
            .NotEmpty();

        RuleFor(request => request.AssignedBy)
            .NotEmpty();
    }
}

public sealed class UpdateUserRequestValidator : AbstractValidator<UsersController.UpdateUserRequest>
{
    private const string PhonePattern = @"^\+?[1-9]\d{0,14}$";

    public UpdateUserRequestValidator()
    {
        RuleFor(request => request.FirstName)
            .MaximumLength(128)
            .When(request => !string.IsNullOrWhiteSpace(request.FirstName));

        RuleFor(request => request.LastName)
            .MaximumLength(128)
            .When(request => !string.IsNullOrWhiteSpace(request.LastName));

        RuleFor(request => request.PhoneNumber)
            .Cascade(CascadeMode.Stop)
            .Must(phone => string.IsNullOrWhiteSpace(phone) || Regex.IsMatch(phone.Trim(), PhonePattern))
            .WithMessage("Phone number must be in E.164 format (max 15 digits).");

        RuleFor(request => request.ProfilePhotoUrl)
            .When(request => !string.IsNullOrWhiteSpace(request.ProfilePhotoUrl))
            .MaximumLength(512);

        RuleFor(request => request.LanguagePreference)
            .When(request => !string.IsNullOrWhiteSpace(request.LanguagePreference))
            .MaximumLength(16);

        RuleFor(request => request.Timezone)
            .When(request => !string.IsNullOrWhiteSpace(request.Timezone))
            .MaximumLength(64);

        RuleFor(request => request.UpdatedBy)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage("UpdatedBy must be a valid identifier when provided.");
    }
}

public sealed class SuspendUserRequestValidator : AbstractValidator<UsersController.SuspendUserRequest>
{
    public SuspendUserRequestValidator()
    {
        RuleFor(request => request.SuspendedBy)
            .NotEmpty();

        RuleFor(request => request.Reason)
            .NotEmpty()
            .Length(3, 1024);
    }
}

public sealed class UnlockUserRequestValidator : AbstractValidator<UsersController.UnlockUserRequest>
{
    public UnlockUserRequestValidator()
    {
        RuleFor(request => request.UnlockedBy)
            .NotEmpty();
    }
}
