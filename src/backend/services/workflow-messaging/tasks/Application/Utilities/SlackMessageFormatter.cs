using System;
using System.Collections.Generic;
using System.Text.Json;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.Application.Utilities;

public static class SlackMessageFormatter
{
    public static string FormatNotification(NotificationType notificationType, object payload)
    {
        var summary = GetSummary(notificationType);
        var blocks = new List<object>
        {
            new
            {
                type = "section",
                text = new
                {
                    type = "mrkdwn",
                    text = summary
                }
            }
        };

        var payloadDetails = BuildDetails(payload);
        if (payloadDetails is not null)
        {
            blocks.Add(new
            {
                type = "section",
                text = new
                {
                    type = "mrkdwn",
                    text = $"```{payloadDetails}```"
                }
            });
        }

        var message = new
        {
            text = summary,
            blocks
        };

        return JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });
    }

    private static string? BuildDetails(object payload)
    {
        if (payload is null)
        {
            return null;
        }

        return payload switch
        {
            string s when !string.IsNullOrWhiteSpace(s) => s,
            _ => JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            })
        };
    }

    private static string GetSummary(NotificationType notificationType) => notificationType switch
    {
        NotificationType.TaskCreated => "*:seedling: Task Created*",
        NotificationType.TaskAssigned => "*:bust_in_silhouette: Task Assigned*",
        NotificationType.TaskCompleted => "*:white_check_mark: Task Completed*",
        NotificationType.TaskOverdue => "*:alarm_clock: Task Overdue*",
        NotificationType.TaskBlocked => "*:no_entry: Task Blocked*",
        NotificationType.ConversationMention => "*:speech_balloon: Conversation Mention*",
        NotificationType.AlertCritical => "*:rotating_light: Critical Alert*",
        NotificationType.AlertWarning => "*:warning: Warning Alert*",
        _ => "*:pushpin: Task Notification*"
    };
}
