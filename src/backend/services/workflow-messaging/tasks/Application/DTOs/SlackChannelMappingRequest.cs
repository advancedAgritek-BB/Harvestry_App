using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.Application.DTOs;

public sealed class SlackChannelMappingRequest
{
    /// <summary>
    /// Slack channel identifier (e.g. C12345).
    /// </summary>
    public string ChannelId { get; init; } = string.Empty;

    /// <summary>
    /// Human readable channel name (e.g. task-alerts).
    /// </summary>
    public string ChannelName { get; init; } = string.Empty;

    /// <summary>
    /// Notification type key (e.g. task_created, task_completed).
    /// </summary>
    [JsonConverter(typeof(SnakeCaseNotificationTypeConverter))]
    public NotificationType NotificationType { get; init; }

    /// <summary>
    /// Optional flag to deactivate mapping without deleting it.
    /// </summary>
    public bool IsActive { get; init; } = true;
}

public sealed class SlackChannelMappingResponse
{
    public Guid SlackChannelMappingId { get; init; }
    public Guid SlackWorkspaceId { get; init; }
    public Guid SiteId { get; init; }
    public string ChannelId { get; init; } = string.Empty;
    public string ChannelName { get; init; } = string.Empty;
    [JsonConverter(typeof(SnakeCaseNotificationTypeConverter))]
    public NotificationType NotificationType { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public Guid CreatedBy { get; init; }
}

public sealed class SnakeCaseNotificationTypeConverter : JsonConverter<NotificationType>
{
    public override NotificationType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var stringValue = reader.GetString();
        if (string.IsNullOrEmpty(stringValue))
        {
            return NotificationType.Undefined;
        }

        // Convert snake_case to PascalCase
        var pascalCase = ToPascalCase(stringValue);
        return Enum.TryParse<NotificationType>(pascalCase, true, out var result) ? result : NotificationType.Undefined;
    }

    public override void Write(Utf8JsonWriter writer, NotificationType value, JsonSerializerOptions options)
    {
        // Convert PascalCase to snake_case
        var snakeCase = ToSnakeCase(value.ToString());
        writer.WriteStringValue(snakeCase);
    }

    private static string ToPascalCase(string snakeCase)
    {
        if (string.IsNullOrEmpty(snakeCase))
        {
            return snakeCase;
        }

        var parts = snakeCase.Split('_');
        for (var i = 0; i < parts.Length; i++)
        {
            if (parts[i].Length > 0)
            {
                parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1).ToLower();
            }
        }
        return string.Join("", parts);
    }

    private static string ToSnakeCase(string pascalCase)
    {
        if (string.IsNullOrEmpty(pascalCase))
        {
            return pascalCase;
        }

        var result = "";
        for (var i = 0; i < pascalCase.Length; i++)
        {
            var c = pascalCase[i];
            if (i > 0 && char.IsUpper(c))
            {
                result += "_";
            }
            result += char.ToLower(c);
        }
        return result;
    }
}
