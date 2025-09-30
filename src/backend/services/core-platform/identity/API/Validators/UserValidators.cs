using FluentValidation;
using Harvestry.Identity.API.Controllers;

namespace Harvestry.Identity.API.Validators;

public sealed class CreateUserRequestValidator : AbstractValidator<UsersController.CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(255)
            .EmailAddress();

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(32)
            .Matches("^[0-9+().\\s-]+$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Phone number may only contain digits and formatting characters.");

        RuleForEach(x => x.SiteAssignments)
            .SetValidator(new SiteAssignmentRequestValidator());
    }
}

public sealed class SiteAssignmentRequestValidator : AbstractValidator<UsersController.SiteAssignmentRequest>
{
    public SiteAssignmentRequestValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty();

        RuleFor(x => x.RoleId)
            .NotEmpty();

        RuleFor(x => x.AssignedBy)
            .NotEmpty();
    }
}

public sealed class UpdateUserRequestValidator : AbstractValidator<UsersController.UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(32)
            .Matches("^[0-9+().\\s-]+$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.ProfilePhotoUrl)
            .MaximumLength(512)
            .When(x => !string.IsNullOrWhiteSpace(x.ProfilePhotoUrl));

        RuleFor(x => x.LanguagePreference)
            .MaximumLength(10)
            .When(x => !string.IsNullOrWhiteSpace(x.LanguagePreference));

        RuleFor(x => x.Timezone)
            .MaximumLength(64)
            .When(x => !string.IsNullOrWhiteSpace(x.Timezone));

        RuleFor(x => x)
            .Must(HasAnyUpdates)
            .WithMessage("At least one field must be provided for update.");
    }

    private static bool HasAnyUpdates(UsersController.UpdateUserRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.FirstName) ||
        !string.IsNullOrWhiteSpace(request.LastName) ||
        !string.IsNullOrWhiteSpace(request.PhoneNumber) ||
        !string.IsNullOrWhiteSpace(request.ProfilePhotoUrl) ||
        !string.IsNullOrWhiteSpace(request.LanguagePreference) ||
        !string.IsNullOrWhiteSpace(request.Timezone) ||
        request.UpdatedBy.HasValue;
    }
}

public sealed class SuspendUserRequestValidator : AbstractValidator<UsersController.SuspendUserRequest>
{
    public SuspendUserRequestValidator()
    {
        RuleFor(x => x.SuspendedBy)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(1024);
    }
}

public sealed class UnlockUserRequestValidator : AbstractValidator<UsersController.UnlockUserRequest>
{
    public UnlockUserRequestValidator()
    {
        RuleFor(x => x.UnlockedBy)
            .NotEmpty();
    }
}
