using Harvestry.Spatial.Application.DTOs;
using FluentValidation;

namespace Harvestry.Spatial.API.Validators;

public sealed class UpdateEquipmentRequestValidator : AbstractValidator<UpdateEquipmentRequest>
{
    public UpdateEquipmentRequestValidator()
    {
        RuleFor(x => x.RequestedByUserId).NotEmpty();
        RuleFor(x => x.Manufacturer).MaximumLength(200);
        RuleFor(x => x.Model).MaximumLength(200);
        RuleFor(x => x.SerialNumber).MaximumLength(200);
        RuleFor(x => x.FirmwareVersion).MaximumLength(50);
    }
}
