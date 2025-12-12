using FluentValidation;
using Harvestry.Compliance.Metrc.Application.DTOs;
using Harvestry.Compliance.Metrc.Domain.Enums;

namespace Harvestry.Compliance.Metrc.API.Validators;

/// <summary>
/// Validator for license upsert requests
/// </summary>
public sealed class UpsertLicenseRequestValidator : AbstractValidator<UpsertLicenseRequest>
{
    private static readonly string[] ValidStateCodes = new[]
    {
        "AK", "AZ", "CA", "CO", "CT", "DC", "FL", "HI", "IL", "LA", "MA", "MD", "ME",
        "MI", "MO", "MT", "NV", "NJ", "NM", "NY", "OH", "OK", "OR", "PA", "WV"
    };

    public UpsertLicenseRequestValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site ID is required");

        RuleFor(x => x.LicenseNumber)
            .NotEmpty()
            .WithMessage("License number is required")
            .MaximumLength(50)
            .WithMessage("License number cannot exceed 50 characters")
            .Matches(@"^[A-Za-z0-9\-]+$")
            .WithMessage("License number must be alphanumeric (hyphens allowed)");

        RuleFor(x => x.StateCode)
            .NotEmpty()
            .WithMessage("State code is required")
            .Length(2)
            .WithMessage("State code must be 2 characters")
            .Must(code => ValidStateCodes.Contains(code.ToUpperInvariant()))
            .WithMessage("Invalid state code for METRC integration");

        RuleFor(x => x.FacilityName)
            .NotEmpty()
            .WithMessage("Facility name is required")
            .MaximumLength(200)
            .WithMessage("Facility name cannot exceed 200 characters");

        RuleFor(x => x.SyncIntervalMinutes)
            .InclusiveBetween(5, 1440)
            .WithMessage("Sync interval must be between 5 minutes and 24 hours (1440 minutes)");
    }
}

/// <summary>
/// Validator for credential setting requests
/// </summary>
public sealed class SetCredentialsRequestValidator : AbstractValidator<SetCredentialsRequest>
{
    public SetCredentialsRequestValidator()
    {
        RuleFor(x => x.LicenseId)
            .NotEmpty()
            .WithMessage("License ID is required");

        RuleFor(x => x.VendorApiKey)
            .NotEmpty()
            .WithMessage("Vendor API key is required")
            .MinimumLength(10)
            .WithMessage("Vendor API key is too short")
            .MaximumLength(200)
            .WithMessage("Vendor API key is too long");

        RuleFor(x => x.UserApiKey)
            .NotEmpty()
            .WithMessage("User API key is required")
            .MinimumLength(10)
            .WithMessage("User API key is too short")
            .MaximumLength(200)
            .WithMessage("User API key is too long");
    }
}

/// <summary>
/// Validator for sync start requests
/// </summary>
public sealed class StartSyncRequestValidator : AbstractValidator<StartSyncRequest>
{
    public StartSyncRequestValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site ID is required");

        RuleFor(x => x.LicenseNumber)
            .NotEmpty()
            .WithMessage("License number is required")
            .MaximumLength(50)
            .WithMessage("License number cannot exceed 50 characters");

        RuleFor(x => x.Direction)
            .IsInEnum()
            .WithMessage("Invalid sync direction");

        RuleFor(x => x.EntityTypes)
            .ForEach(et => et.IsInEnum())
            .When(x => x.EntityTypes != null && x.EntityTypes.Count > 0)
            .WithMessage("Invalid entity type specified");
    }
}

/// <summary>
/// Validator for reconciliation requests
/// </summary>
public sealed class ReconciliationRequestValidator : AbstractValidator<ReconciliationRequest>
{
    public ReconciliationRequestValidator()
    {
        RuleFor(x => x.LicenseId)
            .NotEmpty()
            .WithMessage("License ID is required");

        RuleFor(x => x.EntityTypes)
            .ForEach(et => et.IsInEnum())
            .When(x => x.EntityTypes != null && x.EntityTypes.Count > 0)
            .WithMessage("Invalid entity type specified");
    }
}
