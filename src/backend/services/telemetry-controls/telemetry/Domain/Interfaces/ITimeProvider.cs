namespace Harvestry.Telemetry.Domain.Interfaces;

/// <summary>
/// Provides the current time for domain operations.
/// </summary>
public interface ITimeProvider
{
    DateTimeOffset UtcNow { get; }
}
