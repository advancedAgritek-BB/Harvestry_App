using FluentValidation;
using Harvestry.Telemetry.Application.DTOs;

namespace Harvestry.Telemetry.API.Validators;

public sealed class CreateSensorStreamRequestValidator : AbstractValidator<CreateSensorStreamRequestDto>
{
    public CreateSensorStreamRequestValidator()
    {
        RuleFor(x => x.EquipmentId)
            .NotEmpty();

        RuleFor(x => x.StreamType)
            .IsInEnum();

        RuleFor(x => x.Unit)
            .IsInEnum();

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(200);
    }
}

public sealed class UpdateSensorStreamRequestValidator : AbstractValidator<UpdateSensorStreamRequestDto>
{
    public UpdateSensorStreamRequestValidator()
    {
        RuleFor(x => x.DisplayName)
            .MaximumLength(200)
            .When(x => x.DisplayName != null);
    }
}
