using System;

namespace Harvestry.Tasks.Application.DTOs;

public sealed record SlackMessageResponse(
    string ChannelId,
    string Timestamp,
    string? ThreadTimestamp,
    DateTimeOffset ReceivedAt);
