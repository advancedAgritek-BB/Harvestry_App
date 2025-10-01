using FluentValidation;
using Harvestry.Spatial.Application.DTOs;

namespace Harvestry.Spatial.API.Validators;

public sealed class CreateValveZoneMappingRequestValidator : AbstractValidator<CreateValveZoneMappingRequest>
{
    public CreateValveZoneMappingRequestValidator()
    {
        RuleFor(x => x.SiteId).NotEmpty();
        RuleFor(x => x.EquipmentId).NotEmpty();
        RuleFor(x => x.ZoneLocationId).NotEmpty();
        RuleFor(x => x.RequestedByUserId).NotEmpty();

        RuleFor(x => x.Priority).GreaterThan(0).LessThanOrEqualTo(1000);
        RuleFor(x => x.ValveChannelCode).MaximumLength(100);
        RuleFor(x => x.InterlockGroup).MaximumLength(100);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

