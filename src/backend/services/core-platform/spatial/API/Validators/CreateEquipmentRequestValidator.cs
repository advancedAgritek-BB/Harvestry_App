using Harvestry.Spatial.Application.DTOs;
using FluentValidation;

namespace Harvestry.Spatial.API.Validators;

public sealed class CreateEquipmentRequestValidator : AbstractValidator<CreateEquipmentRequest>
{
    public CreateEquipmentRequestValidator()
    {
        RuleFor(x => x.SiteId).NotEmpty();
        RuleFor(x => x.LocationId).NotEmpty();
        RuleFor(x => x.RequestedByUserId).NotEmpty();
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.TypeCode)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.Manufacturer).MaximumLength(200);
        RuleFor(x => x.Model).MaximumLength(200);
        RuleFor(x => x.SerialNumber).MaximumLength(200);
        RuleFor(x => x.FirmwareVersion).MaximumLength(50);
    }
}
