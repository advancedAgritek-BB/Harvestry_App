using System;

namespace Harvestry.Spatial.Application.DTOs;

public sealed record UpdateNetworkRequest
{
    public string? IpAddress { get; init; }
    public string? MacAddress { get; init; }
    public string? MqttTopic { get; init; }
    public Guid RequestedByUserId { get; init; }
}
