using Harvestry.Compliance.BioTrack.Domain.Entities;
using Harvestry.Compliance.BioTrack.Domain.Enums;

namespace Harvestry.Compliance.BioTrack.Application.Interfaces;

/// <summary>
/// Service interface for BioTrack synchronization operations.
/// Mirrors METRC sync service for consistent patterns.
/// </summary>
public interface IBioTrackSyncService
{
    Task<BioTrackSyncResponse> StartSyncAsync(
        BioTrackSyncRequest request,
        Guid? initiatedByUserId = null,
        CancellationToken cancellationToken = default);

    Task<BioTrackSyncJob?> GetSyncJobAsync(
        Guid syncJobId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BioTrackSyncJob>> GetSyncJobsAsync(
        Guid siteId,
        int limit = 20,
        CancellationToken cancellationToken = default);

    Task<bool> CancelSyncJobAsync(
        Guid syncJobId,
        string? reason = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for BioTrack license management
/// </summary>
public interface IBioTrackLicenseService
{
    Task<BioTrackLicense?> GetLicenseAsync(Guid licenseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BioTrackLicense>> GetLicensesForSiteAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<BioTrackLicense> UpsertLicenseAsync(UpsertBioTrackLicenseRequest request, CancellationToken cancellationToken = default);
    Task<bool> SetCredentialsAsync(SetBioTrackCredentialsRequest request, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> TestConnectionAsync(Guid licenseId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Request to start a BioTrack sync
/// </summary>
public sealed record BioTrackSyncRequest
{
    public Guid SiteId { get; init; }
    public string LicenseNumber { get; init; } = string.Empty;
    public BioTrackSyncDirection Direction { get; init; }
    public IReadOnlyList<BioTrackEntityType>? EntityTypes { get; init; }
    public bool ForceFullSync { get; init; }
}

/// <summary>
/// Response from starting a BioTrack sync
/// </summary>
public sealed record BioTrackSyncResponse
{
    public Guid SyncJobId { get; init; }
    public BioTrackSyncStatus Status { get; init; }
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Request to create/update a BioTrack license
/// </summary>
public sealed record UpsertBioTrackLicenseRequest
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
/// Request to set BioTrack credentials
/// </summary>
public sealed record SetBioTrackCredentialsRequest
{
    public Guid LicenseId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
