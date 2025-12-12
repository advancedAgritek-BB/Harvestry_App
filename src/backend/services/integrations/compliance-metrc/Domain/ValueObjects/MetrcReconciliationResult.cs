using Harvestry.Compliance.Metrc.Domain.Enums;

namespace Harvestry.Compliance.Metrc.Domain.ValueObjects;

/// <summary>
/// Represents the result of a METRC reconciliation operation.
/// Used to track discrepancies between Harvestry and METRC data.
/// </summary>
public sealed record MetrcReconciliationResult
{
    /// <summary>
    /// The type of entity being reconciled
    /// </summary>
    public MetrcEntityType EntityType { get; init; }

    /// <summary>
    /// Total count of entities in Harvestry
    /// </summary>
    public int HarvestryCount { get; init; }

    /// <summary>
    /// Total count of entities in METRC
    /// </summary>
    public int MetrcCount { get; init; }

    /// <summary>
    /// Count of entities that match between systems
    /// </summary>
    public int MatchedCount { get; init; }

    /// <summary>
    /// Count of entities only in Harvestry (missing in METRC)
    /// </summary>
    public int HarvestryOnlyCount { get; init; }

    /// <summary>
    /// Count of entities only in METRC (missing in Harvestry)
    /// </summary>
    public int MetrcOnlyCount { get; init; }

    /// <summary>
    /// Count of entities with data discrepancies
    /// </summary>
    public int DiscrepancyCount { get; init; }

    /// <summary>
    /// Detailed discrepancies found
    /// </summary>
    public IReadOnlyList<ReconciliationDiscrepancy> Discrepancies { get; init; } = Array.Empty<ReconciliationDiscrepancy>();

    /// <summary>
    /// Timestamp when reconciliation was performed
    /// </summary>
    public DateTimeOffset ReconciliationTimestamp { get; init; }

    /// <summary>
    /// Duration of the reconciliation operation
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Whether the reconciliation was successful
    /// </summary>
    public bool IsSuccessful => DiscrepancyCount == 0 && HarvestryOnlyCount == 0 && MetrcOnlyCount == 0;

    /// <summary>
    /// Summary message for the reconciliation result
    /// </summary>
    public string Summary => IsSuccessful
        ? $"Reconciliation successful: {MatchedCount} {EntityType} entities in sync"
        : $"Reconciliation found issues: {DiscrepancyCount} discrepancies, {HarvestryOnlyCount} Harvestry-only, {MetrcOnlyCount} METRC-only";

    public static MetrcReconciliationResult Create(
        MetrcEntityType entityType,
        int harvestryCount,
        int metrcCount,
        int matchedCount,
        int harvestryOnlyCount,
        int metrcOnlyCount,
        int discrepancyCount,
        IReadOnlyList<ReconciliationDiscrepancy> discrepancies,
        TimeSpan duration)
    {
        return new MetrcReconciliationResult
        {
            EntityType = entityType,
            HarvestryCount = harvestryCount,
            MetrcCount = metrcCount,
            MatchedCount = matchedCount,
            HarvestryOnlyCount = harvestryOnlyCount,
            MetrcOnlyCount = metrcOnlyCount,
            DiscrepancyCount = discrepancyCount,
            Discrepancies = discrepancies,
            ReconciliationTimestamp = DateTimeOffset.UtcNow,
            Duration = duration
        };
    }
}

/// <summary>
/// Represents a single discrepancy found during reconciliation
/// </summary>
public sealed record ReconciliationDiscrepancy
{
    /// <summary>
    /// The Harvestry entity ID
    /// </summary>
    public Guid HarvestryEntityId { get; init; }

    /// <summary>
    /// The METRC entity ID (if known)
    /// </summary>
    public long? MetrcId { get; init; }

    /// <summary>
    /// The METRC label/tag (if applicable)
    /// </summary>
    public string? MetrcLabel { get; init; }

    /// <summary>
    /// Type of discrepancy
    /// </summary>
    public DiscrepancyType Type { get; init; }

    /// <summary>
    /// Field that has the discrepancy (for value mismatches)
    /// </summary>
    public string? FieldName { get; init; }

    /// <summary>
    /// Value in Harvestry
    /// </summary>
    public string? HarvestryValue { get; init; }

    /// <summary>
    /// Value in METRC
    /// </summary>
    public string? MetrcValue { get; init; }

    /// <summary>
    /// Human-readable description of the discrepancy
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Recommended action to resolve the discrepancy
    /// </summary>
    public string? RecommendedAction { get; init; }

    /// <summary>
    /// Severity of the discrepancy
    /// </summary>
    public DiscrepancySeverity Severity { get; init; }
}

/// <summary>
/// Types of reconciliation discrepancies
/// </summary>
public enum DiscrepancyType
{
    /// <summary>
    /// Entity exists in Harvestry but not in METRC
    /// </summary>
    MissingInMetrc = 1,

    /// <summary>
    /// Entity exists in METRC but not in Harvestry
    /// </summary>
    MissingInHarvestry = 2,

    /// <summary>
    /// Entity exists in both but values don't match
    /// </summary>
    ValueMismatch = 3,

    /// <summary>
    /// Entity status differs between systems
    /// </summary>
    StatusMismatch = 4,

    /// <summary>
    /// Quantity/weight doesn't match
    /// </summary>
    QuantityMismatch = 5,

    /// <summary>
    /// Location/room doesn't match
    /// </summary>
    LocationMismatch = 6,

    /// <summary>
    /// Timestamp/date doesn't match
    /// </summary>
    TimestampMismatch = 7
}

/// <summary>
/// Severity levels for discrepancies
/// </summary>
public enum DiscrepancySeverity
{
    /// <summary>
    /// Informational only, may not require action
    /// </summary>
    Info = 1,

    /// <summary>
    /// Warning - should be reviewed
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Error - requires attention
    /// </summary>
    Error = 3,

    /// <summary>
    /// Critical - compliance risk, requires immediate action
    /// </summary>
    Critical = 4
}
