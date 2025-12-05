using System;
using FluentValidation;
using Harvestry.Telemetry.Application.DTOs;

namespace Harvestry.Telemetry.API.Validators;

/// <summary>
/// Validates HTTP ingest payloads before they reach the adapter.
/// </summary>
public sealed class HttpIngestRequestValidator : AbstractValidator<HttpIngestRequestDto>
{
    private const int MaxReadingsPerRequest = 1000;

    public HttpIngestRequestValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty();

        RuleFor(x => x.Readings)
            .NotNull()
            .NotEmpty()
            .Must(readings => readings.Count <= MaxReadingsPerRequest)
            .WithMessage($"Maximum of {MaxReadingsPerRequest} readings allowed per request");

        RuleForEach(x => x.Readings)
            .SetValidator(new SensorReadingPayloadValidator());
    }

    private sealed class SensorReadingPayloadValidator : AbstractValidator<SensorReadingDto>
    {
        private static readonly TimeSpan MaxFutureSkew = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan MaxPastWindow = TimeSpan.FromDays(90);

        public SensorReadingPayloadValidator()
        {
            RuleFor(x => x.StreamId)
                .NotEmpty();

            RuleFor(x => x.Unit)
                .IsInEnum();

            RuleFor(x => x.Value)
                .Must(value => !double.IsNaN(value) && !double.IsInfinity(value))
                .WithMessage("Value must be a finite number.");

            RuleFor(x => x.MessageId)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.MessageId));

            RuleFor(x => x.Time)
                .Must(time => time <= DateTimeOffset.UtcNow + MaxFutureSkew)
                .WithMessage($"Timestamp cannot be more than {MaxFutureSkew.TotalMinutes:F0} minutes in the future.")
                .Must(time => time >= DateTimeOffset.UtcNow - MaxPastWindow)
                .WithMessage($"Timestamp cannot be older than {MaxPastWindow.TotalDays:F0} days.");
        }
    }
}
