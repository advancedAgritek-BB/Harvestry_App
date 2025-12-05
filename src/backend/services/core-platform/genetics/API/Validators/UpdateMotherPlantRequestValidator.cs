using FluentValidation;
using Harvestry.Genetics.Application.DTOs;

namespace Harvestry.Genetics.API.Validators;

/// <summary>
/// Validator for mother plant updates.
/// </summary>
public class UpdateMotherPlantRequestValidator : AbstractValidator<UpdateMotherPlantRequest>
{
    public UpdateMotherPlantRequestValidator()
    {
        RuleFor(x => x.MaxPropagationCount)
            .GreaterThan(0)
            .When(x => x.MaxPropagationCount.HasValue);

        RuleFor(x => x.StatusUpdate!.Reason)
            .NotEmpty()
            .When(x => x.StatusUpdate is not null && x.StatusUpdate.Action is MotherPlantStatusAction.Retire or MotherPlantStatusAction.Quarantine or MotherPlantStatusAction.Destroy);

        RuleFor(x => x)
            .Must(HasAtLeastOneChange)
            .WithMessage("At least one field must be provided to update the mother plant.");
    }

    private static bool HasAtLeastOneChange(UpdateMotherPlantRequest request)
    {
        return request.LocationId.HasValue
            || request.RoomId.HasValue
            || request.MaxPropagationCount.HasValue
            || !string.IsNullOrWhiteSpace(request.Notes)
            || request.Metadata is not null
            || request.StatusUpdate is not null;
    }
}
