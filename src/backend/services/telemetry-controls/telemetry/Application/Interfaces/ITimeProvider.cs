namespace Harvestry.Telemetry.Application.Interfaces;

/// <summary>
/// Provides the current time for domain operations.
/// </summary>
public interface ITimeProvider
{
    DateTimeOffset UtcNow { get; }
}
