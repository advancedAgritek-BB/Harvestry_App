using FluentValidation;
using Harvestry.Spatial.Application.DTOs;
using Harvestry.Spatial.Domain.Enums;

namespace Harvestry.Spatial.API.Validators;

public sealed class CreateRoomRequestValidator : AbstractValidator<CreateRoomRequest>
{
    public CreateRoomRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.CustomRoomType)
            .NotEmpty()
            .When(x => x.RoomType == RoomType.Custom)
            .WithMessage("Custom room type name is required when room type is Custom.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.AreaSqft)
            .GreaterThanOrEqualTo(0)
            .When(x => x.AreaSqft.HasValue);

        RuleFor(x => x.HeightFt)
            .GreaterThanOrEqualTo(0)
            .When(x => x.HeightFt.HasValue);

        RuleFor(x => x.TargetHumidityPct)
            .InclusiveBetween(0, 100)
            .When(x => x.TargetHumidityPct.HasValue);

        RuleFor(x => x.TargetCo2Ppm)
            .GreaterThanOrEqualTo(0)
            .When(x => x.TargetCo2Ppm.HasValue);
    }
}
