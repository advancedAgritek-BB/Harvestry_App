namespace Harvestry.Tasks.Domain.Enums;

/// <summary>
/// Enumerates the supported dependency relationships between tasks.
/// </summary>
public enum DependencyType
{
    Undefined = 0,
    FinishToStart,
    FinishToFinish,
    StartToStart
}
