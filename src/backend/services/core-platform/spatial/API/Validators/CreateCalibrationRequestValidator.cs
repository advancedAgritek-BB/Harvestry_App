using FluentValidation;
using Harvestry.Spatial.Application.DTOs;

namespace Harvestry.Spatial.API.Validators;

public sealed class CreateCalibrationRequestValidator : AbstractValidator<CreateCalibrationRequest>
{
    public CreateCalibrationRequestValidator()
    {
        RuleFor(x => x.SiteId).NotEmpty();
        RuleFor(x => x.EquipmentId).NotEmpty();
        RuleFor(x => x.PerformedByUserId).NotEmpty();

        RuleFor(x => x.ChannelCode).MaximumLength(100);
        RuleFor(x => x.CoefficientsJson).MaximumLength(4000);
        RuleFor(x => x.FirmwareVersionAtCalibration).MaximumLength(50);
        RuleFor(x => x.AttachmentUrl).MaximumLength(500);
        RuleFor(x => x.Notes).MaximumLength(2000);

        RuleFor(x => x.IntervalDaysOverride)
            .GreaterThan(0)
            .LessThanOrEqualTo(365)
            .When(x => x.IntervalDaysOverride.HasValue);
    }
}
