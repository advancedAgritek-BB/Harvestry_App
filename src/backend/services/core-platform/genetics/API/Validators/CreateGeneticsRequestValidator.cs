using FluentValidation;
using Harvestry.Genetics.Application.DTOs;

namespace Harvestry.Genetics.API.Validators;

/// <summary>
/// Validator for CreateGeneticsRequest
/// </summary>
public sealed class CreateGeneticsRequestValidator : AbstractValidator<CreateGeneticsRequest>
{
    public CreateGeneticsRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Genetics name is required.")
            .MaximumLength(200)
            .WithMessage("Genetics name cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required.")
            .MaximumLength(2000)
            .WithMessage("Description cannot exceed 2000 characters.");

        RuleFor(x => x.GeneticType)
            .IsInEnum()
            .WithMessage("Invalid genetic type.");

        RuleFor(x => x.ThcMin)
            .GreaterThanOrEqualTo(0)
            .WithMessage("THC minimum must be >= 0.")
            .LessThanOrEqualTo(100)
            .WithMessage("THC minimum cannot exceed 100%.");

        RuleFor(x => x.ThcMax)
            .GreaterThanOrEqualTo(0)
            .WithMessage("THC maximum must be >= 0.")
            .LessThanOrEqualTo(100)
            .WithMessage("THC maximum cannot exceed 100%.")
            .GreaterThanOrEqualTo(x => x.ThcMin)
            .WithMessage("THC maximum must be >= THC minimum.");

        RuleFor(x => x.CbdMin)
            .GreaterThanOrEqualTo(0)
            .WithMessage("CBD minimum must be >= 0.")
            .LessThanOrEqualTo(100)
            .WithMessage("CBD minimum cannot exceed 100%.");

        RuleFor(x => x.CbdMax)
            .GreaterThanOrEqualTo(0)
            .WithMessage("CBD maximum must be >= 0.")
            .LessThanOrEqualTo(100)
            .WithMessage("CBD maximum cannot exceed 100%.")
            .GreaterThanOrEqualTo(x => x.CbdMin)
            .WithMessage("CBD maximum must be >= CBD minimum.");

        RuleFor(x => x.FloweringTimeDays)
            .GreaterThan(0)
            .WithMessage("Flowering time must be > 0 days.")
            .LessThanOrEqualTo(365)
            .WithMessage("Flowering time cannot exceed 365 days.")
            .When(x => x.FloweringTimeDays.HasValue);

        RuleFor(x => x.YieldPotential)
            .IsInEnum()
            .WithMessage("Invalid yield potential.");

        RuleFor(x => x.BreedingNotes)
            .MaximumLength(5000)
            .WithMessage("Breeding notes cannot exceed 5000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.BreedingNotes));
    }
}

