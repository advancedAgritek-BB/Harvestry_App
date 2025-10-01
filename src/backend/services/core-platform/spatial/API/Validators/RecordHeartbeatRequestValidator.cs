using Harvestry.Spatial.Application.DTOs;
using FluentValidation;

namespace Harvestry.Spatial.API.Validators;

public sealed class RecordHeartbeatRequestValidator : AbstractValidator<RecordHeartbeatRequest>
{
    public RecordHeartbeatRequestValidator()
    {
        RuleFor(x => x.HeartbeatAt)
            .NotEmpty();

        RuleFor(x => x.BatteryPercent)
            .InclusiveBetween(0, 100)
            .When(x => x.BatteryPercent.HasValue);
    }
}
