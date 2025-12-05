using Harvestry.Telemetry.Application.Interfaces;

namespace Harvestry.Telemetry.Infrastructure;

/// <summary>
/// System time provider implementation.
/// </summary>
public class SystemTimeProvider : ITimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
