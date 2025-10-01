using System;

namespace Harvestry.Spatial.Application.DTOs;

public sealed record RecordHeartbeatRequest
{
    public DateTimeOffset HeartbeatAt { get; init; }
    public int? SignalStrengthDbm { get; init; }
    public int? BatteryPercent { get; init; }
    public long? UptimeSeconds { get; init; }
}
