using FluentValidation;
using Harvestry.Spatial.Application.DTOs;

namespace Harvestry.Spatial.API.Validators;

public sealed class UpdateValveZoneMappingRequestValidator : AbstractValidator<UpdateValveZoneMappingRequest>
{
    public UpdateValveZoneMappingRequestValidator()
    {
        RuleFor(x => x.RequestedByUserId).NotEmpty();
        RuleFor(x => x.Priority).GreaterThan(0).LessThanOrEqualTo(1000);
        RuleFor(x => x.InterlockGroup).MaximumLength(100);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.ValveChannelCode).MaximumLength(100);
    }
}

