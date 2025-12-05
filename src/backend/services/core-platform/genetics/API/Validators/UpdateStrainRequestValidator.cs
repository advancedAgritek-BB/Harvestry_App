using FluentValidation;
using Harvestry.Genetics.Application.DTOs;

namespace Harvestry.Genetics.API.Validators;

/// <summary>
/// Validator for UpdateStrainRequest
/// </summary>
public sealed class UpdateStrainRequestValidator : AbstractValidator<UpdateStrainRequest>
{
    public UpdateStrainRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required.")
            .MaximumLength(2000)
            .WithMessage("Description cannot exceed 2000 characters.");

        RuleFor(x => x.Breeder)
            .MaximumLength(200)
            .WithMessage("Breeder name cannot exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Breeder));

        RuleFor(x => x.SeedBank)
            .MaximumLength(200)
            .WithMessage("Seed bank name cannot exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.SeedBank));

        RuleFor(x => x.CultivationNotes)
            .MaximumLength(5000)
            .WithMessage("Cultivation notes cannot exceed 5000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.CultivationNotes));

        RuleFor(x => x.ExpectedHarvestWindowDays)
            .GreaterThan(0)
            .WithMessage("Expected harvest window must be > 0 days.")
            .LessThanOrEqualTo(365)
            .WithMessage("Expected harvest window cannot exceed 365 days.")
            .When(x => x.ExpectedHarvestWindowDays.HasValue);
    }
}

