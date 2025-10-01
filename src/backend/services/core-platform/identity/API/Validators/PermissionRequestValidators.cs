using System;
using FluentValidation;
using Harvestry.Identity.API.Controllers;

namespace Harvestry.Identity.API.Validators;

public sealed class PermissionCheckRequestValidator : AbstractValidator<PermissionsController.PermissionCheckRequest>
{
    public PermissionCheckRequestValidator()
    {
        RuleFor(request => request.UserId)
            .NotEmpty()
            .Must(id => id != Guid.Empty)
            .WithMessage("UserId cannot be empty GUID");

        RuleFor(request => request.SiteId)
            .NotEmpty()
            .Must(id => id != Guid.Empty)
            .WithMessage("SiteId cannot be empty GUID");

        RuleFor(request => request.Action)
            .NotEmpty()
            .Length(1, 128);

        RuleFor(request => request.ResourceType)
            .NotEmpty()
            .Length(1, 128);
    }
}

public sealed class InitiateApprovalRequestValidator : AbstractValidator<PermissionsController.InitiateApprovalRequest>
{
    public InitiateApprovalRequestValidator()
    {
        RuleFor(request => request.Action)
            .NotEmpty()
            .Length(1, 128);

        RuleFor(request => request.ResourceType)
            .NotEmpty()
            .Length(1, 128);

        RuleFor(request => request.ResourceId)
            .NotEmpty()
            .Must(id => id != Guid.Empty)
            .WithMessage("ResourceId cannot be empty GUID");

        RuleFor(request => request.SiteId)
            .NotEmpty()
            .Must(id => id != Guid.Empty)
            .WithMessage("SiteId cannot be empty GUID");

        RuleFor(request => request.InitiatorUserId)
            .NotEmpty()
            .Must(id => id != Guid.Empty)
            .WithMessage("InitiatorUserId cannot be empty GUID");

        RuleFor(request => request.Reason)
            .NotEmpty()
            .Length(3, 1024);

        RuleFor(request => request.Attestation)
            .MaximumLength(1024)
            .When(request => !string.IsNullOrWhiteSpace(request.Attestation));
    }
}

public sealed class ApproveRequestValidator : AbstractValidator<PermissionsController.ApproveRequest>
{
    public ApproveRequestValidator()
    {
        RuleFor(request => request.ApproverUserId)
            .NotEmpty()
            .Must(id => id != Guid.Empty)
            .WithMessage("ApproverUserId cannot be empty GUID");

        RuleFor(request => request.Reason)
            .NotEmpty()
            .Length(3, 512);

        RuleFor(request => request.Attestation)
            .MaximumLength(1024)
            .When(request => !string.IsNullOrWhiteSpace(request.Attestation));
    }
}

public sealed class RejectRequestValidator : AbstractValidator<PermissionsController.RejectRequest>
{
    public RejectRequestValidator()
    {
        RuleFor(request => request.ApproverUserId)
            .NotEmpty()
            .Must(id => id != Guid.Empty)
            .WithMessage("ApproverUserId cannot be empty GUID");

        RuleFor(request => request.Reason)
            .NotEmpty()
            .Length(3, 512);
    }
}
