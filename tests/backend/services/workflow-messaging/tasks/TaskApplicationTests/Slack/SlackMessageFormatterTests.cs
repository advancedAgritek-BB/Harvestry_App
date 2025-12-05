using System;
using Harvestry.Tasks.Application.Utilities;
using Harvestry.Tasks.Domain.Enums;
using Xunit;

namespace Harvestry.Tasks.Application.Tests.Slack;

public sealed class SlackMessageFormatterTests
{
    [Fact]
    public void FormatNotification_IncludesSummaryHeader()
    {
        // Arrange
        var payload = new { taskId = Guid.NewGuid(), title = "Calibrate sensors" };

        // Act
        var json = SlackMessageFormatter.FormatNotification(NotificationType.TaskCreated, payload);

        // Assert
        Assert.Contains("Task Created", json);
        Assert.Contains("calibrate sensors", json.ToLowerInvariant());
    }

    [Fact]
    public void FormatNotification_HandlesNullPayload()
    {
        // Act
        var json = SlackMessageFormatter.FormatNotification(NotificationType.AlertWarning, null!);

        // Assert
        Assert.Contains("warning", json.ToLowerInvariant());
    }
}
