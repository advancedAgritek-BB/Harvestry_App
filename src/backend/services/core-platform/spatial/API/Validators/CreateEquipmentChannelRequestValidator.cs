using Harvestry.Spatial.Application.DTOs;
using FluentValidation;

namespace Harvestry.Spatial.API.Validators;

public sealed class CreateEquipmentChannelRequestValidator : AbstractValidator<CreateEquipmentChannelRequest>
{
    public CreateEquipmentChannelRequestValidator()
    {
        RuleFor(x => x.ChannelCode)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Role)
            .MaximumLength(200);

        RuleFor(x => x.RequestedByUserId)
            .NotEmpty();
    }
}
