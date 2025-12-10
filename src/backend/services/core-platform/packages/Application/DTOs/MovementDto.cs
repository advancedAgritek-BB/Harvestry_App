namespace Harvestry.Packages.Application.DTOs;

/// <summary>
/// Full movement DTO
/// </summary>
public record MovementDto
{
    public Guid Id { get; init; }
    public Guid SiteId { get; init; }
    public string MovementType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;

    // Package
    public Guid PackageId { get; init; }
    public string PackageLabel { get; init; } = string.Empty;
    public Guid? ItemId { get; init; }
    public string? ItemName { get; init; }

    // Locations
    public Guid? FromLocationId { get; init; }
    public string? FromLocationPath { get; init; }
    public Guid? ToLocationId { get; init; }
    public string? ToLocationPath { get; init; }

    // Quantity
    public decimal Quantity { get; init; }
    public string UnitOfMeasure { get; init; } = string.Empty;
    public decimal? QuantityBefore { get; init; }
    public decimal? QuantityAfter { get; init; }

    // Cost
    public decimal? UnitCost { get; init; }
    public decimal? TotalCost { get; init; }

    // Reason
    public string? ReasonCode { get; init; }
    public string? ReasonNotes { get; init; }

    // Processing
    public Guid? ProcessingJobId { get; init; }
    public string? ProcessingJobNumber { get; init; }

    // Order
    public Guid? SalesOrderId { get; init; }
    public string? SalesOrderNumber { get; init; }

    // Compliance
    public string? MetrcManifestId { get; init; }
    public string SyncStatus { get; init; } = "pending";

    // Verification
    public Guid? VerifiedByUserId { get; init; }
    public DateTime? VerifiedAt { get; init; }
    public string? BarcodeScanned { get; init; }

    // Notes
    public string? Notes { get; init; }
    public List<string> PhotoUrls { get; init; } = new();

    // Approval
    public bool RequiresApproval { get; init; }
    public Guid? FirstApproverId { get; init; }
    public DateTime? FirstApprovedAt { get; init; }
    public Guid? SecondApproverId { get; init; }
    public DateTime? SecondApprovedAt { get; init; }

    // Batch
    public Guid? BatchMovementId { get; init; }
    public int? BatchSequence { get; init; }

    // Audit
    public DateTime CreatedAt { get; init; }
    public Guid CreatedByUserId { get; init; }
    public DateTime? CompletedAt { get; init; }
    public Guid? CompletedByUserId { get; init; }
}

/// <summary>
/// Summary movement DTO for lists
/// </summary>
public record MovementSummaryDto
{
    public Guid Id { get; init; }
    public string MovementType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string PackageLabel { get; init; } = string.Empty;
    public string? ItemName { get; init; }
    public string? FromLocationPath { get; init; }
    public string? ToLocationPath { get; init; }
    public decimal Quantity { get; init; }
    public string UnitOfMeasure { get; init; } = string.Empty;
    public string? ReasonCode { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid CreatedByUserId { get; init; }
    public string SyncStatus { get; init; } = "pending";
}

/// <summary>
/// Paginated movement list response
/// </summary>
public record MovementListResponse
{
    public List<MovementSummaryDto> Movements { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

/// <summary>
/// Request to create batch movements
/// </summary>
public record BatchMovementRequest
{
    public string BatchType { get; init; } = string.Empty;
    public string? Description { get; init; }
    public List<BatchMovementItem> Movements { get; init; } = new();
}

public record BatchMovementItem
{
    public Guid PackageId { get; init; }
    public Guid? ToLocationId { get; init; }
    public decimal? Quantity { get; init; }
    public string? ReasonCode { get; init; }
    public string? Notes { get; init; }
}



