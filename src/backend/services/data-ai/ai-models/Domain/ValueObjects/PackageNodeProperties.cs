using System.Text.Json;

namespace Harvestry.AiModels.Domain.ValueObjects;

/// <summary>
/// Properties specific to Package nodes in the traceability graph.
/// Captures key attributes for anomaly detection and lineage analysis.
/// </summary>
public sealed record PackageNodeProperties
{
    /// <summary>Package label (METRC tag)</summary>
    public string PackageLabel { get; init; } = string.Empty;

    /// <summary>Item name/SKU</summary>
    public string ItemName { get; init; } = string.Empty;

    /// <summary>Item category (Flower, Concentrate, etc.)</summary>
    public string ItemCategory { get; init; } = string.Empty;

    /// <summary>Current quantity</summary>
    public decimal Quantity { get; init; }

    /// <summary>Initial quantity when created</summary>
    public decimal InitialQuantity { get; init; }

    /// <summary>Unit of measure</summary>
    public string UnitOfMeasure { get; init; } = string.Empty;

    /// <summary>Current location ID</summary>
    public Guid? LocationId { get; init; }

    /// <summary>Current location name</summary>
    public string? LocationName { get; init; }

    /// <summary>Package status (Active, OnHold, etc.)</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Lab testing state</summary>
    public string? LabTestingState { get; init; }

    /// <summary>THC percentage</summary>
    public decimal? ThcPercent { get; init; }

    /// <summary>CBD percentage</summary>
    public decimal? CbdPercent { get; init; }

    /// <summary>Generation depth in lineage tree (0 = root)</summary>
    public int GenerationDepth { get; init; }

    /// <summary>Root ancestor package ID</summary>
    public Guid? RootAncestorId { get; init; }

    /// <summary>Hold reason if on hold</summary>
    public string? HoldReasonCode { get; init; }

    /// <summary>METRC sync status</summary>
    public string? MetrcSyncStatus { get; init; }

    /// <summary>Unit cost</summary>
    public decimal? UnitCost { get; init; }

    /// <summary>Quality grade</summary>
    public string? Grade { get; init; }

    /// <summary>Serialize to JSON for storage</summary>
    public string ToJson() => JsonSerializer.Serialize(this);

    /// <summary>Deserialize from JSON</summary>
    public static PackageNodeProperties? FromJson(string? json)
        => string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<PackageNodeProperties>(json);
}

/// <summary>
/// Properties specific to InventoryMovement nodes.
/// </summary>
public sealed record MovementNodeProperties
{
    /// <summary>Movement type (Transfer, Adjustment, etc.)</summary>
    public string MovementType { get; init; } = string.Empty;

    /// <summary>Movement status</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Package ID involved</summary>
    public Guid PackageId { get; init; }

    /// <summary>Package label</summary>
    public string? PackageLabel { get; init; }

    /// <summary>Quantity moved</summary>
    public decimal Quantity { get; init; }

    /// <summary>Unit of measure</summary>
    public string UnitOfMeasure { get; init; } = string.Empty;

    /// <summary>From location ID</summary>
    public Guid? FromLocationId { get; init; }

    /// <summary>From location path</summary>
    public string? FromLocationPath { get; init; }

    /// <summary>To location ID</summary>
    public Guid? ToLocationId { get; init; }

    /// <summary>To location path</summary>
    public string? ToLocationPath { get; init; }

    /// <summary>Reason code</summary>
    public string? ReasonCode { get; init; }

    /// <summary>Requires approval flag</summary>
    public bool RequiresApproval { get; init; }

    /// <summary>First approver ID</summary>
    public Guid? FirstApproverId { get; init; }

    /// <summary>Second approver ID (dual control)</summary>
    public Guid? SecondApproverId { get; init; }

    /// <summary>Verified by user ID</summary>
    public Guid? VerifiedByUserId { get; init; }

    /// <summary>Created by user ID</summary>
    public Guid CreatedByUserId { get; init; }

    /// <summary>Sales order ID if applicable</summary>
    public Guid? SalesOrderId { get; init; }

    /// <summary>Transfer ID if applicable</summary>
    public Guid? TransferId { get; init; }

    /// <summary>Processing job ID if applicable</summary>
    public Guid? ProcessingJobId { get; init; }

    /// <summary>Serialize to JSON</summary>
    public string ToJson() => JsonSerializer.Serialize(this);

    /// <summary>Deserialize from JSON</summary>
    public static MovementNodeProperties? FromJson(string? json)
        => string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<MovementNodeProperties>(json);
}
