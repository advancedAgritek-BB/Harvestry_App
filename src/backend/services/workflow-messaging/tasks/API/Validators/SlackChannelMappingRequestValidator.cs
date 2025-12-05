using System;
using FluentValidation;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.API.Validators;

public sealed class SlackChannelMappingRequestValidator : AbstractValidator<SlackChannelMappingRequest>
{
    private static readonly string[] AllowedNotificationTypes =
    {
        "task_created",
        "task_assigned",
        "task_completed",
        "task_overdue",
        "task_blocked",
        "conversation_mention",
        "alert_critical",
        "alert_warning"
    };

    public SlackChannelMappingRequestValidator()
    {
        RuleFor(x => x.ChannelId)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ChannelName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.NotificationType)
            .NotEqual(NotificationType.Undefined);
    }
}
