using Harvestry.Spatial.Application.DTOs;
using FluentValidation;

namespace Harvestry.Spatial.API.Validators;

public sealed class UpdateNetworkRequestValidator : AbstractValidator<UpdateNetworkRequest>
{
    public UpdateNetworkRequestValidator()
    {
        RuleFor(x => x.RequestedByUserId).NotEmpty();

        RuleFor(x => x.IpAddress)
            .MaximumLength(100);

        RuleFor(x => x.MacAddress)
            .MaximumLength(50);

        RuleFor(x => x.MqttTopic)
            .MaximumLength(500);
    }
}
