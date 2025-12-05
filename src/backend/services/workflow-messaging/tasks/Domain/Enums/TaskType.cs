namespace Harvestry.Tasks.Domain.Enums;

/// <summary>
/// Represents the canonical task types supported by the workflow engine.
/// Custom task categories can be represented via <see cref="TaskType.Custom"/>
/// combined with a domain-specific descriptor.
/// </summary>
public enum TaskType
{
    Undefined = 0,
    Operational,
    Compliance,
    Maintenance,
    Harvest,
    Irrigation,
    Training,
    Investigation,
    Custom
}
