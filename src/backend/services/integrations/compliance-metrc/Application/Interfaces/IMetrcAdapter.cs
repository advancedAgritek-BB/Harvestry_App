using Harvestry.Compliance.Metrc.Infrastructure.External;

namespace Harvestry.Compliance.Metrc.Application.Interfaces;

/// <summary>
/// Base interface for all METRC API adapters
/// </summary>
public interface IMetrcAdapter
{
    /// <summary>
    /// The METRC module name (e.g., "plants", "packages")
    /// </summary>
    string ModuleName { get; }
}

/// <summary>
/// Adapter for METRC Facilities API
/// </summary>
public interface IMetrcFacilitiesAdapter : IMetrcAdapter
{
    Task<MetrcApiResponse<IEnumerable<MetrcFacilityDto>>> GetFacilitiesAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Adapter for METRC Plant Batches API
/// </summary>
public interface IMetrcPlantBatchesAdapter : IMetrcAdapter
{
    Task<MetrcApiResponse<IEnumerable<MetrcPlantBatchDto>>> GetActiveAsync(
        string licenseNumber,
        CancellationToken cancellationToken = default);

    Task<MetrcApiResponse<IEnumerable<MetrcPlantBatchDto>>> GetInactiveAsync(
        string licenseNumber,
        DateOnly lastModifiedStart,
        DateOnly lastModifiedEnd,
        CancellationToken cancellationToken = default);

    Task<MetrcApiResponse<object>> CreatePlantingsAsync(
        string licenseNumber,
        IEnumerable<CreatePlantBatchRequest> requests,
        CancellationToken cancellationToken = default);

    Task<MetrcApiResponse<object>> ChangeGrowthPhaseAsync(
        string licenseNumber,
        IEnumerable<ChangeGrowthPhaseRequest> requests,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Adapter for METRC Plants API
/// </summary>
public interface IMetrcPlantsAdapter : IMetrcAdapter
{
    Task<MetrcApiResponse<IEnumerable<MetrcPlantDto>>> GetVegetativeAsync(
        string licenseNumber,
        CancellationToken cancellationToken = default);

    Task<MetrcApiResponse<IEnumerable<MetrcPlantDto>>> GetFloweringAsync(
        string licenseNumber,
        CancellationToken cancellationToken = default);

    Task<MetrcApiResponse<object>> MovePlantsAsync(
        string licenseNumber,
        IEnumerable<MovePlantRequest> requests,
        CancellationToken cancellationToken = default);

    Task<MetrcApiResponse<object>> DestroyPlantsAsync(
        string licenseNumber,
        IEnumerable<DestroyPlantRequest> requests,
        CancellationToken cancellationToken = default);

    Task<MetrcApiResponse<object>> HarvestPlantsAsync(
        string licenseNumber,
        IEnumerable<HarvestPlantRequest> requests,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Adapter for METRC Harvests API
/// </summary>
public interface IMetrcHarvestsAdapter : IMetrcAdapter
{
    Task<MetrcApiResponse<IEnumerable<MetrcHarvestDto>>> GetActiveAsync(
        string licenseNumber,
        CancellationToken cancellationToken = default);

    Task<MetrcApiResponse<object>> CreatePackagesFromHarvestAsync(
        string licenseNumber,
        IEnumerable<CreatePackageFromHarvestRequest> requests,
        CancellationToken cancellationToken = default);

    Task<MetrcApiResponse<object>> RecordWasteAsync(
        string licenseNumber,
        IEnumerable<RecordHarvestWasteRequest> requests,
        CancellationToken cancellationToken = default);

    Task<MetrcApiResponse<object>> FinishAsync(
        string licenseNumber,
        IEnumerable<FinishHarvestRequest> requests,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Adapter for METRC Packages API
/// </summary>
public interface IMetrcPackagesAdapter : IMetrcAdapter
{
    Task<MetrcApiResponse<IEnumerable<MetrcPackageDto>>> GetActiveAsync(
        string licenseNumber,
        CancellationToken cancellationToken = default);

    Task<MetrcApiResponse<object>> CreateAsync(
        string licenseNumber,
        IEnumerable<CreatePackageRequest> requests,
        CancellationToken cancellationToken = default);

    Task<MetrcApiResponse<object>> AdjustAsync(
        string licenseNumber,
        IEnumerable<AdjustPackageRequest> requests,
        CancellationToken cancellationToken = default);

    Task<MetrcApiResponse<object>> RemediateAsync(
        string licenseNumber,
        IEnumerable<RemediatePackageRequest> requests,
        CancellationToken cancellationToken = default);

    Task<MetrcApiResponse<object>> FinishAsync(
        string licenseNumber,
        IEnumerable<FinishPackageRequest> requests,
        CancellationToken cancellationToken = default);

    Task<MetrcApiResponse<object>> ChangeLocationAsync(
        string licenseNumber,
        IEnumerable<ChangePackageLocationRequest> requests,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Adapter for METRC Items API
/// </summary>
public interface IMetrcItemsAdapter : IMetrcAdapter
{
    Task<MetrcApiResponse<IEnumerable<MetrcItemDto>>> GetActiveAsync(
        string licenseNumber,
        CancellationToken cancellationToken = default);

    Task<MetrcApiResponse<object>> CreateAsync(
        string licenseNumber,
        IEnumerable<CreateItemRequest> requests,
        CancellationToken cancellationToken = default);

    Task<MetrcApiResponse<object>> UpdateAsync(
        string licenseNumber,
        IEnumerable<UpdateItemRequest> requests,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Adapter for METRC Strains API
/// </summary>
public interface IMetrcStrainsAdapter : IMetrcAdapter
{
    Task<MetrcApiResponse<IEnumerable<MetrcStrainDto>>> GetActiveAsync(
        string licenseNumber,
        CancellationToken cancellationToken = default);

    Task<MetrcApiResponse<object>> CreateAsync(
        string licenseNumber,
        IEnumerable<CreateStrainRequest> requests,
        CancellationToken cancellationToken = default);

    Task<MetrcApiResponse<object>> UpdateAsync(
        string licenseNumber,
        IEnumerable<UpdateStrainRequest> requests,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Adapter for METRC Locations API
/// </summary>
public interface IMetrcLocationsAdapter : IMetrcAdapter
{
    Task<MetrcApiResponse<IEnumerable<MetrcLocationDto>>> GetActiveAsync(
        string licenseNumber,
        CancellationToken cancellationToken = default);

    Task<MetrcApiResponse<object>> CreateAsync(
        string licenseNumber,
        IEnumerable<CreateLocationRequest> requests,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Adapter for METRC Lab Tests API
/// </summary>
public interface IMetrcLabTestsAdapter : IMetrcAdapter
{
    Task<MetrcApiResponse<IEnumerable<MetrcLabTestDto>>> GetByPackageLabelAsync(
        string licenseNumber,
        string packageLabel,
        CancellationToken cancellationToken = default);

    Task<MetrcApiResponse<IEnumerable<MetrcLabTestTypeDto>>> GetTestTypesAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Adapter for METRC Processing Jobs API
/// </summary>
public interface IMetrcProcessingAdapter : IMetrcAdapter
{
    Task<MetrcApiResponse<IEnumerable<MetrcProcessingJobDto>>> GetActiveAsync(
        string licenseNumber,
        CancellationToken cancellationToken = default);

    Task<MetrcApiResponse<object>> CreateAsync(
        string licenseNumber,
        IEnumerable<CreateProcessingJobRequest> requests,
        CancellationToken cancellationToken = default);

    Task<MetrcApiResponse<object>> FinishAsync(
        string licenseNumber,
        IEnumerable<FinishProcessingJobRequest> requests,
        CancellationToken cancellationToken = default);
}

// Placeholder DTO classes - will be detailed in separate files
public record MetrcFacilityDto;
public record MetrcPlantBatchDto;
public record MetrcPlantDto;
public record MetrcHarvestDto;
public record MetrcPackageDto;
public record MetrcItemDto;
public record MetrcStrainDto;
public record MetrcLocationDto;
public record MetrcLabTestDto;
public record MetrcLabTestTypeDto;
public record MetrcProcessingJobDto;

// Placeholder request classes - will be detailed in separate files
public record CreatePlantBatchRequest;
public record ChangeGrowthPhaseRequest;
public record MovePlantRequest;
public record DestroyPlantRequest;
public record HarvestPlantRequest;
public record CreatePackageFromHarvestRequest;
public record RecordHarvestWasteRequest;
public record FinishHarvestRequest;
public record CreatePackageRequest;
public record AdjustPackageRequest;
public record RemediatePackageRequest;
public record FinishPackageRequest;
public record ChangePackageLocationRequest;
public record CreateItemRequest;
public record UpdateItemRequest;
public record CreateStrainRequest;
public record UpdateStrainRequest;
public record CreateLocationRequest;
public record CreateProcessingJobRequest;
public record FinishProcessingJobRequest;








