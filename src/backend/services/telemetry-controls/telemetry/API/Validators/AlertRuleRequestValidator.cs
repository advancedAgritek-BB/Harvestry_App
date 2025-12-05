using FluentValidation;
using Harvestry.Telemetry.Application.DTOs;

namespace Harvestry.Telemetry.API.Validators;

public sealed class CreateAlertRuleRequestValidator : AbstractValidator<CreateAlertRuleRequestDto>
{
    public CreateAlertRuleRequestValidator()
    {
        RuleFor(x => x.RuleName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.StreamIds)
            .NotNull()
            .Must(list => list.Count > 0)
            .WithMessage("At least one stream id is required");

        RuleFor(x => x.ThresholdConfig)
            .Must(config => config.Validate())
            .WithMessage("Threshold configuration invalid for rule type");

        RuleFor(x => x.EvaluationWindowMinutes)
            .GreaterThan(0);

        RuleFor(x => x.CooldownMinutes)
            .GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateAlertRuleRequestValidator : AbstractValidator<UpdateAlertRuleRequestDto>
{
    public UpdateAlertRuleRequestValidator()
    {
        RuleFor(x => x.RuleName)
            .NotEmpty()
            .MaximumLength(200)
            .When(x => x.RuleName != null);

        RuleFor(x => x.StreamIds)
            .Must(list => list != null && list.Count > 0)
            .When(x => x.StreamIds != null)
            .WithMessage("If supplied, stream ids must contain at least one entry");

        RuleFor(x => x.ThresholdConfig)
            .Must(config => !config.HasValue || config.Value.Validate())
            .WithMessage("Threshold configuration invalid for rule type");

        RuleFor(x => x.EvaluationWindowMinutes)
            .GreaterThan(0)
            .When(x => x.EvaluationWindowMinutes.HasValue);

        RuleFor(x => x.CooldownMinutes)
            .GreaterThanOrEqualTo(0)
            .When(x => x.CooldownMinutes.HasValue);
    }
}

public sealed class AcknowledgeAlertRequestValidator : AbstractValidator<AcknowledgeAlertRequestDto>
{
    public AcknowledgeAlertRequestValidator()
    {
        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => x.Notes != null);
    }
}
