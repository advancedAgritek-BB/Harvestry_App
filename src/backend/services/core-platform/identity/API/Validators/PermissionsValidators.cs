using FluentValidation;
using Harvestry.Identity.API.Controllers;

namespace Harvestry.Identity.API.Validators;

public sealed class PermissionCheckRequestValidator : AbstractValidator<PermissionsController.PermissionCheckRequest>
{
    public PermissionCheckRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SiteId).NotEmpty();

        RuleFor(x => x.Action)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.ResourceType)
            .NotEmpty()
            .MaximumLength(128);
    }
}

public sealed class InitiateApprovalRequestValidator : AbstractValidator<PermissionsController.InitiateApprovalRequest>
{
    public InitiateApprovalRequestValidator()
    {
        RuleFor(x => x.Action)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.ResourceType)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.ResourceId)
            .NotEmpty();

        RuleFor(x => x.SiteId)
            .NotEmpty();

        RuleFor(x => x.InitiatorUserId)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(1024);

        RuleFor(x => x.Attestation)
            .MaximumLength(1024)
            .When(x => !string.IsNullOrWhiteSpace(x.Attestation));
    }
}

public sealed class ApproveRequestValidator : AbstractValidator<PermissionsController.ApproveRequest>
{
    public ApproveRequestValidator()
    {
        RuleFor(x => x.ApproverUserId)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(512);

        RuleFor(x => x.Attestation)
            .MaximumLength(1024)
            .When(x => !string.IsNullOrWhiteSpace(x.Attestation));
    }
}

public sealed class RejectRequestValidator : AbstractValidator<PermissionsController.RejectRequest>
{
    public RejectRequestValidator()
    {
        RuleFor(x => x.ApproverUserId)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(512);
    }
}
