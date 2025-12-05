using FluentValidation;
using Harvestry.Genetics.Application.DTOs;

namespace Harvestry.Genetics.API.Validators;

/// <summary>
/// Validator for propagation settings updates.
/// </summary>
public class UpdatePropagationSettingsRequestValidator : AbstractValidator<UpdatePropagationSettingsRequest>
{
    public UpdatePropagationSettingsRequestValidator()
    {
        RuleFor(x => x.DailyLimit)
            .GreaterThan(0)
            .When(x => x.DailyLimit.HasValue);

        RuleFor(x => x.WeeklyLimit)
            .GreaterThan(0)
            .When(x => x.WeeklyLimit.HasValue);

        RuleFor(x => x)
            .Must(WeeklyGreaterOrEqualDaily)
            .WithMessage("Weekly limit must be greater than or equal to the daily limit when both are provided.");

        RuleFor(x => x.MotherPropagationLimit)
            .GreaterThan(0)
            .When(x => x.MotherPropagationLimit.HasValue);

        RuleFor(x => x.ApproverRole)
            .MaximumLength(100);
    }

    private static bool WeeklyGreaterOrEqualDaily(UpdatePropagationSettingsRequest request)
    {
        if (!request.DailyLimit.HasValue || !request.WeeklyLimit.HasValue)
        {
            return true;
        }

        return request.WeeklyLimit.Value >= request.DailyLimit.Value;
    }
}
