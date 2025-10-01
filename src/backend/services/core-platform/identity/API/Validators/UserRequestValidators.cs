using System;
using FluentValidation;
using Harvestry.Identity.API.Controllers;
using Harvestry.Identity.API.Validation;

namespace Harvestry.Identity.API.Validators;

public sealed class CreateUserRequestValidator : AbstractValidator<UsersController.CreateUserRequest>
{

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
            .Must(phone => string.IsNullOrWhiteSpace(phone) || ValidationConstants.PhoneRegex.IsMatch(phone.Trim()))
            .WithMessage(ValidationConstants.ErrorMessages.InvalidPhoneFormat);

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
            .Must(phone => string.IsNullOrWhiteSpace(phone) || ValidationConstants.PhoneRegex.IsMatch(phone.Trim()))
            .WithMessage(ValidationConstants.ErrorMessages.InvalidPhoneFormat);

        RuleFor(request => request.ProfilePhotoUrl)
            .MaximumLength(512)
            .When(request => !string.IsNullOrWhiteSpace(request.ProfilePhotoUrl));

        RuleFor(request => request.LanguagePreference)
            .MaximumLength(16)
            .When(request => !string.IsNullOrWhiteSpace(request.LanguagePreference));

        RuleFor(request => request.Timezone)
            .MaximumLength(64)
            .When(request => !string.IsNullOrWhiteSpace(request.Timezone));

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
