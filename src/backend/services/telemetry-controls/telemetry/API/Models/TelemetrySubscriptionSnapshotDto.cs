using System;
using System.Collections.Generic;

namespace Harvestry.Telemetry.API.Models;

public sealed record TelemetrySubscriptionSnapshotDto(
    DateTimeOffset CapturedAt,
    int TotalConnections,
    int TotalSubscriptions,
    int ActiveStreamCount,
    IReadOnlyList<StreamSubscriptionSummaryDto> Streams);

public sealed record StreamSubscriptionSummaryDto(Guid StreamId, int SubscriberCount);
