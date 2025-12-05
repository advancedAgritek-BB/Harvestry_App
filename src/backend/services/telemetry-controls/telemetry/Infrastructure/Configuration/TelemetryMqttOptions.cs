using System;

namespace Harvestry.Telemetry.Infrastructure.Configuration;

/// <summary>
/// Configuration options for the telemetry MQTT listener.
/// </summary>
public sealed class TelemetryMqttOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the MQTT listener should run.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// MQTT broker host name or IP address.
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// MQTT broker port.
    /// </summary>
    public int Port { get; set; } = 1883;

    /// <summary>
    /// Optional username for broker authentication.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Optional password for broker authentication.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Optional client identifier. Defaults to a machine specific value when not supplied.
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// Topic filter used for subscription.
    /// </summary>
    public string TopicFilter { get; set; } = "site/+/equipment/+/telemetry/#";

    /// <summary>
    /// Number of seconds to wait before retrying a failed connection.
    /// </summary>
    public int ReconnectIntervalSeconds { get; set; } = 5;

    /// <summary>
    /// Enables TLS when connecting to the broker.
    /// </summary>
    public bool UseTls { get; set; }

    /// <summary>
    /// Builds a reasonable default client identifier when one is not supplied.
    /// Client IDs are limited to 23 characters for MQTT 3.1/3.1.1 compatibility.
    /// </summary>
    public string ResolveClientId()
    {
        if (!string.IsNullOrWhiteSpace(ClientId))
        {
            if (ClientId.Length > 23)
            {
                throw new InvalidOperationException($"ClientId exceeds maximum length of 23 characters: {ClientId}");
            }
            return ClientId;
        }

        // Generate a default client ID, ensuring it fits within MQTT limits
        var machineSuffix = Environment.MachineName.Length > 8
            ? Environment.MachineName.Substring(0, 8)  // Truncate long machine names
            : Environment.MachineName;

        var defaultId = $"telemetry-mqtt-{machineSuffix}-{Environment.ProcessId}";

        // Ensure the result doesn't exceed 23 characters
        return defaultId.Length > 23 ? defaultId.Substring(0, 23) : defaultId;
    }
}
