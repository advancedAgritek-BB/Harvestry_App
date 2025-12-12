using Harvestry.Compliance.Metrc.Domain.Enums;

namespace Harvestry.Compliance.Metrc.Application.Interfaces;

/// <summary>
/// Interface for mapping between Harvestry entities and METRC DTOs
/// </summary>
public interface IMetrcEntityMapper
{
    /// <summary>
    /// Maps a Harvestry plant batch to METRC create request
    /// </summary>
    Task<object> MapPlantBatchToMetrcAsync(
        Guid harvestryBatchId,
        string licenseNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Maps a Harvestry plant to METRC create request
    /// </summary>
    Task<object> MapPlantToMetrcAsync(
        Guid harvestryPlantId,
        string licenseNumber,
        MetrcOperationType operationType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Maps a Harvestry harvest to METRC create request
    /// </summary>
    Task<object> MapHarvestToMetrcAsync(
        Guid harvestryHarvestId,
        string licenseNumber,
        MetrcOperationType operationType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Maps a Harvestry package to METRC create request
    /// </summary>
    Task<object> MapPackageToMetrcAsync(
        Guid harvestryPackageId,
        string licenseNumber,
        MetrcOperationType operationType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Maps a Harvestry item to METRC create request
    /// </summary>
    Task<object> MapItemToMetrcAsync(
        Guid harvestryItemId,
        string licenseNumber,
        MetrcOperationType operationType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Maps a Harvestry strain to METRC create request
    /// </summary>
    Task<object> MapStrainToMetrcAsync(
        Guid harvestryStrainId,
        string licenseNumber,
        MetrcOperationType operationType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Maps a Harvestry location to METRC create request
    /// </summary>
    Task<object> MapLocationToMetrcAsync(
        Guid harvestryLocationId,
        string licenseNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Maps a METRC plant batch response to Harvestry entity update
    /// </summary>
    Task<PlantBatchSyncResult> MapMetrcToPlantBatchAsync(
        object metrcPlantBatch,
        string licenseNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Maps a METRC plant response to Harvestry entity update
    /// </summary>
    Task<PlantSyncResult> MapMetrcToPlantAsync(
        object metrcPlant,
        string licenseNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Maps a METRC harvest response to Harvestry entity update
    /// </summary>
    Task<HarvestSyncResult> MapMetrcToHarvestAsync(
        object metrcHarvest,
        string licenseNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Maps a METRC package response to Harvestry entity update
    /// </summary>
    Task<PackageSyncResult> MapMetrcToPackageAsync(
        object metrcPackage,
        string licenseNumber,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of syncing a plant batch from METRC
/// </summary>
public sealed record PlantBatchSyncResult
{
    public Guid? HarvestryBatchId { get; init; }
    public long MetrcId { get; init; }
    public string BatchName { get; init; } = string.Empty;
    public bool IsNew { get; init; }
    public bool HasChanges { get; init; }
    public IReadOnlyList<string> ChangedFields { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Result of syncing a plant from METRC
/// </summary>
public sealed record PlantSyncResult
{
    public Guid? HarvestryPlantId { get; init; }
    public long MetrcId { get; init; }
    public string? PlantLabel { get; init; }
    public bool IsNew { get; init; }
    public bool HasChanges { get; init; }
    public IReadOnlyList<string> ChangedFields { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Result of syncing a harvest from METRC
/// </summary>
public sealed record HarvestSyncResult
{
    public Guid? HarvestryHarvestId { get; init; }
    public long MetrcId { get; init; }
    public string HarvestName { get; init; } = string.Empty;
    public bool IsNew { get; init; }
    public bool HasChanges { get; init; }
    public IReadOnlyList<string> ChangedFields { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Result of syncing a package from METRC
/// </summary>
public sealed record PackageSyncResult
{
    public Guid? HarvestryPackageId { get; init; }
    public long MetrcId { get; init; }
    public string PackageLabel { get; init; } = string.Empty;
    public bool IsNew { get; init; }
    public bool HasChanges { get; init; }
    public IReadOnlyList<string> ChangedFields { get; init; } = Array.Empty<string>();
}
