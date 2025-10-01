using FluentValidation;
using Harvestry.Spatial.Application.DTOs;

namespace Harvestry.Spatial.API.Validators;

public sealed class UpdateRoomRequestValidator : AbstractValidator<UpdateRoomRequest>
{
    public UpdateRoomRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.AreaSqft)
            .GreaterThanOrEqualTo(0)
            .When(x => x.AreaSqft.HasValue);

        RuleFor(x => x.HeightFt)
            .GreaterThanOrEqualTo(0)
            .When(x => x.HeightFt.HasValue);

        RuleFor(x => x.TargetTempF)
            .InclusiveBetween(32, 122)
            .When(x => x.TargetTempF.HasValue)
            .WithMessage("Target temperature must be between 32°F and 122°F for cultivation environments.");

        RuleFor(x => x.TargetHumidityPct)
            .InclusiveBetween(0, 100)
            .When(x => x.TargetHumidityPct.HasValue);

        RuleFor(x => x.TargetCo2Ppm)
            .GreaterThanOrEqualTo(0)
            .When(x => x.TargetCo2Ppm.HasValue);
    }
}
