namespace Harvestry.Compliance.Metrc.Domain.Enums;

/// <summary>
/// Direction of data synchronization with METRC
/// </summary>
public enum SyncDirection
{
    /// <summary>
    /// Pull data from METRC into Harvestry
    /// </summary>
    Inbound = 1,

    /// <summary>
    /// Push data from Harvestry to METRC
    /// </summary>
    Outbound = 2,

    /// <summary>
    /// Bidirectional synchronization (reconciliation)
    /// </summary>
    Bidirectional = 3
}
