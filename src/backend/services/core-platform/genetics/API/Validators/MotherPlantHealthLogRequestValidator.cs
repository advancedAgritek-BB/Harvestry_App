using FluentValidation;
using Harvestry.Genetics.Application.DTOs;

namespace Harvestry.Genetics.API.Validators;

/// <summary>
/// Validator for mother plant health log requests.
/// </summary>
public class MotherPlantHealthLogRequestValidator : AbstractValidator<MotherPlantHealthLogRequest>
{
    public MotherPlantHealthLogRequestValidator()
    {
        RuleFor(x => x.LogDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.LogDate.HasValue);

        RuleFor(x => x.PhotoUrls)
            .NotNull();

        RuleFor(x => x.NutrientDeficiencies)
            .NotNull();

        RuleFor(x => x.PhotoUrls)
            .Must(urls => urls.Count <= 10)
            .WithMessage("A maximum of 10 photo URLs can be attached to a single health log entry.");

        RuleForEach(x => x.PhotoUrls)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Photo URLs must be absolute URIs.");

        RuleForEach(x => x.NutrientDeficiencies)
            .MaximumLength(100);
    }
}
