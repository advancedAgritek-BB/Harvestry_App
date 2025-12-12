using Harvestry.Transfers.Application.DTOs;
using Harvestry.Transfers.Application.Interfaces;
using Harvestry.Transfers.Application.Mappers;
using Harvestry.Transfers.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Harvestry.Transfers.Application.Services;

public sealed class TransportManifestService : ITransportManifestService
{
    private readonly ITransportManifestRepository _manifestRepository;
    private readonly IOutboundTransferRepository _transferRepository;
    private readonly ILogger<TransportManifestService> _logger;

    public TransportManifestService(
        ITransportManifestRepository manifestRepository,
        IOutboundTransferRepository transferRepository,
        ILogger<TransportManifestService> logger)
    {
        _manifestRepository = manifestRepository;
        _transferRepository = transferRepository;
        _logger = logger;
    }

    public async Task<TransportManifestDto?> GetByTransferIdAsync(Guid siteId, Guid outboundTransferId, CancellationToken cancellationToken = default)
    {
        var manifest = await _manifestRepository.GetByTransferIdAsync(siteId, outboundTransferId, cancellationToken);
        return manifest?.ToDto();
    }

    public async Task<TransportManifestDto> CreateOrUpdateAsync(Guid siteId, Guid outboundTransferId, UpsertTransportManifestRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var transfer = await _transferRepository.GetByIdAsync(siteId, outboundTransferId, cancellationToken);
        if (transfer == null) throw new InvalidOperationException("Outbound transfer not found.");

        var manifest = await _manifestRepository.GetByTransferIdAsync(siteId, outboundTransferId, cancellationToken);
        if (manifest == null)
        {
            manifest = TransportManifest.CreateDraft(siteId, outboundTransferId, userId);
            await _manifestRepository.AddAsync(manifest, cancellationToken);
        }

        manifest.SetTransporter(request.TransporterName, request.TransporterLicenseNumber, userId);
        manifest.SetDriver(request.DriverName, request.DriverLicenseNumber, request.DriverPhone, userId);
        manifest.SetVehicle(request.VehicleMake, request.VehicleModel, request.VehiclePlate, userId);
        manifest.SetTimes(request.DepartureAt, request.ArrivalAt, userId);

        await _manifestRepository.UpdateAsync(manifest, cancellationToken);
        await _manifestRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Upserted transport manifest {ManifestId} for transfer {TransferId}", manifest.Id, outboundTransferId);
        return manifest.ToDto();
    }
}

