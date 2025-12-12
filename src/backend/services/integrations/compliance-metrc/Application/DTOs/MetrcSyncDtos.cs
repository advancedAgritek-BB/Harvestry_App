using Harvestry.Compliance.Metrc.Domain.Enums;

namespace Harvestry.Compliance.Metrc.Application.DTOs;

/// <summary>
/// Request to start a manual sync job
/// </summary>
public sealed record StartSyncRequest
{
    public Guid SiteId { get; init; }
    public string LicenseNumber { get; init; } = string.Empty;
    public SyncDirection Direction { get; init; }
    public IReadOnlyList<MetrcEntityType>? EntityTypes { get; init; }
    public bool ForceFullSync { get; init; }
}

/// <summary>
/// Response from starting a sync job
/// </summary>
public sealed record StartSyncResponse
{
    public Guid SyncJobId { get; init; }
    public SyncStatus Status { get; init; }
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// DTO for sync job details
/// </summary>
public sealed record SyncJobDto
{
    public Guid Id { get; init; }
    public Guid SiteId { get; init; }
    public string LicenseNumber { get; init; } = string.Empty;
    public string StateCode { get; init; } = string.Empty;
    public SyncDirection Direction { get; init; }
    public SyncStatus Status { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? StartedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public int TotalItems { get; init; }
    public int ProcessedItems { get; init; }
    public int SuccessfulItems { get; init; }
    public int FailedItems { get; init; }
    public int RetryCount { get; init; }
    public string? ErrorMessage { get; init; }
    public double ProgressPercent => TotalItems > 0 ? (ProcessedItems * 100.0 / TotalItems) : 0;
    public TimeSpan? Duration { get; init; }
}

/// <summary>
/// DTO for queue item details
/// </summary>
public sealed record QueueItemDto
{
    public Guid Id { get; init; }
    public Guid SyncJobId { get; init; }
    public MetrcEntityType EntityType { get; init; }
    public MetrcOperationType OperationType { get; init; }
    public Guid HarvestryEntityId { get; init; }
    public long? MetrcId { get; init; }
    public string? MetrcLabel { get; init; }
    public SyncStatus Status { get; init; }
    public int Priority { get; init; }
    public int RetryCount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ProcessedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }
}

/// <summary>
/// DTO for license configuration
/// </summary>
public sealed record LicenseDto
{
    public Guid Id { get; init; }
    public Guid SiteId { get; init; }
    public string LicenseNumber { get; init; } = string.Empty;
    public string StateCode { get; init; } = string.Empty;
    public string FacilityName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool UseSandbox { get; init; }
    public bool AutoSyncEnabled { get; init; }
    public int SyncIntervalMinutes { get; init; }
    public bool HasCredentials { get; init; }
    public DateTimeOffset? LastSyncAt { get; init; }
    public DateTimeOffset? LastSuccessfulSyncAt { get; init; }
    public string? LastSyncError { get; init; }
    public bool IsSyncDue { get; init; }
}

/// <summary>
/// Request to create or update a license
/// </summary>
public sealed record UpsertLicenseRequest
{
    public Guid SiteId { get; init; }
    public string LicenseNumber { get; init; } = string.Empty;
    public string StateCode { get; init; } = string.Empty;
    public string FacilityName { get; init; } = string.Empty;
    public bool UseSandbox { get; init; }
    public bool AutoSyncEnabled { get; init; } = true;
    public int SyncIntervalMinutes { get; init; } = 15;
}

/// <summary>
/// Request to set license credentials
/// </summary>
public sealed record SetCredentialsRequest
{
    public Guid LicenseId { get; init; }
    public string VendorApiKey { get; init; } = string.Empty;
    public string UserApiKey { get; init; } = string.Empty;
}

/// <summary>
/// DTO for sync status summary
/// </summary>
public sealed record SyncStatusSummaryDto
{
    public Guid LicenseId { get; init; }
    public string LicenseNumber { get; init; } = string.Empty;
    public DateTimeOffset? LastSyncAt { get; init; }
    public DateTimeOffset? LastSuccessfulSyncAt { get; init; }
    public bool IsSyncInProgress { get; init; }
    public Guid? CurrentSyncJobId { get; init; }
    public int PendingQueueItems { get; init; }
    public int FailedQueueItems { get; init; }
    public IReadOnlyList<EntitySyncStatusDto> EntityStatuses { get; init; } = Array.Empty<EntitySyncStatusDto>();
}

/// <summary>
/// DTO for per-entity-type sync status
/// </summary>
public sealed record EntitySyncStatusDto
{
    public MetrcEntityType EntityType { get; init; }
    public DateTimeOffset? LastSyncAt { get; init; }
    public int LastSyncItemCount { get; init; }
    public int ConsecutiveFailures { get; init; }
    public string? LastError { get; init; }
}

/// <summary>
/// DTO for reconciliation request
/// </summary>
public sealed record ReconciliationRequest
{
    public Guid LicenseId { get; init; }
    public IReadOnlyList<MetrcEntityType> EntityTypes { get; init; } = Array.Empty<MetrcEntityType>();
    public bool IncludeDetails { get; init; } = true;
}

/// <summary>
/// DTO for reconciliation response
/// </summary>
public sealed record ReconciliationResponseDto
{
    public Guid LicenseId { get; init; }
    public string LicenseNumber { get; init; } = string.Empty;
    public DateTimeOffset ReconciliationTimestamp { get; init; }
    public TimeSpan Duration { get; init; }
    public bool IsInSync { get; init; }
    public IReadOnlyList<EntityReconciliationDto> EntityResults { get; init; } = Array.Empty<EntityReconciliationDto>();
}

/// <summary>
/// DTO for per-entity reconciliation result
/// </summary>
public sealed record EntityReconciliationDto
{
    public MetrcEntityType EntityType { get; init; }
    public int HarvestryCount { get; init; }
    public int MetrcCount { get; init; }
    public int MatchedCount { get; init; }
    public int HarvestryOnlyCount { get; init; }
    public int MetrcOnlyCount { get; init; }
    public int DiscrepancyCount { get; init; }
    public bool IsInSync => DiscrepancyCount == 0 && HarvestryOnlyCount == 0 && MetrcOnlyCount == 0;
    public IReadOnlyList<DiscrepancyDto>? Discrepancies { get; init; }
}

/// <summary>
/// DTO for a single discrepancy
/// </summary>
public sealed record DiscrepancyDto
{
    public Guid HarvestryEntityId { get; init; }
    public long? MetrcId { get; init; }
    public string? MetrcLabel { get; init; }
    public string Type { get; init; } = string.Empty;
    public string? FieldName { get; init; }
    public string? HarvestryValue { get; init; }
    public string? MetrcValue { get; init; }
    public string Description { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string? RecommendedAction { get; init; }
}
