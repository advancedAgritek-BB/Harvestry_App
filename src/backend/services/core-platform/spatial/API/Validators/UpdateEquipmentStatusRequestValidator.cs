using Harvestry.Spatial.Application.DTOs;
using FluentValidation;

namespace Harvestry.Spatial.API.Validators;

public sealed class UpdateEquipmentStatusRequestValidator : AbstractValidator<UpdateEquipmentStatusRequest>
{
    public UpdateEquipmentStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}
