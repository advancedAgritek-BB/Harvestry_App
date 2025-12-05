using FluentValidation;
using Harvestry.Telemetry.Application.DTOs;

namespace Harvestry.Telemetry.API.Validators;

public sealed class IngestTelemetryRequestValidator : AbstractValidator<IngestTelemetryRequestDto>
{
    public IngestTelemetryRequestValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty();

        RuleFor(x => x.EquipmentId)
            .NotEmpty();

        RuleFor(x => x.Protocol)
            .IsInEnum();

        RuleFor(x => x.Readings)
            .NotNull()
            .NotEmpty();

        RuleForEach(x => x.Readings)
            .SetValidator(new SensorReadingPayloadValidator());
    }

    private sealed class SensorReadingPayloadValidator : AbstractValidator<SensorReadingDto>
    {
        public SensorReadingPayloadValidator()
        {
            RuleFor(x => x.StreamId)
                .NotEmpty();

            RuleFor(x => x.Unit)
                .IsInEnum();

            RuleFor(x => x.Value)
                .Must(val => !double.IsNaN(val) && !double.IsInfinity(val))
                .WithMessage("Value must be finite");

            RuleFor(x => x.MessageId)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.MessageId));
        }
    }
}

public sealed class QueryTelemetryRequestValidator : AbstractValidator<QueryTelemetryRequestDto>
{
    public QueryTelemetryRequestValidator()
    {
        RuleFor(x => x.StreamId)
            .NotEmpty();

        RuleFor(x => x.Start)
            .LessThan(x => x.End)
            .WithMessage("Start time must be before end time.");

        RuleFor(x => x.Limit)
            .GreaterThan(0)
            .When(x => x.Limit.HasValue);

        RuleFor(x => x.RollupInterval)
            .IsInEnum()
            .When(x => x.RollupInterval.HasValue);
    }
}
