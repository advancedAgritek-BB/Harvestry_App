using System.Text.Json;
using FluentValidation;
using Harvestry.Spatial.Application.DTOs;

namespace Harvestry.Spatial.API.Validators;

public sealed class UpdateLocationRequestValidator : AbstractValidator<UpdateLocationRequest>
{
    public UpdateLocationRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Barcode)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Barcode));

        RuleFor(x => x.LengthFt)
            .GreaterThanOrEqualTo(0)
            .When(x => x.LengthFt.HasValue);

        RuleFor(x => x.WidthFt)
            .GreaterThanOrEqualTo(0)
            .When(x => x.WidthFt.HasValue);

        RuleFor(x => x.HeightFt)
            .GreaterThanOrEqualTo(0)
            .When(x => x.HeightFt.HasValue);

        RuleFor(x => x.PlantCapacity)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PlantCapacity.HasValue);

        RuleFor(x => x.WeightCapacityLbs)
            .GreaterThanOrEqualTo(0)
            .When(x => x.WeightCapacityLbs.HasValue);

        RuleFor(x => x.RowNumber)
            .GreaterThanOrEqualTo(0)
            .When(x => x.RowNumber.HasValue);

        RuleFor(x => x.ColumnNumber)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ColumnNumber.HasValue);

        RuleFor(x => x.MetadataJson)
            .Must(BeValidJson)
            .When(x => !string.IsNullOrWhiteSpace(x.MetadataJson))
            .WithMessage("MetadataJson must be valid JSON.");
    }

    private static bool BeValidJson(string? metadata)
    {
        if (string.IsNullOrWhiteSpace(metadata))
        {
            return true;
        }
        try
        {
            using var doc = JsonDocument.Parse(metadata);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
