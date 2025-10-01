using FluentValidation;
using Harvestry.Spatial.API.Contracts;
using Harvestry.Spatial.Domain.Enums;

namespace Harvestry.Spatial.API.Validators;

public sealed class UpdateRoomStatusRequestValidator : AbstractValidator<UpdateRoomStatusRequest>
{
    public UpdateRoomStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage($"Status must be one of: {string.Join(", ", Enum.GetNames(typeof(RoomStatus)))}");
    }
}
