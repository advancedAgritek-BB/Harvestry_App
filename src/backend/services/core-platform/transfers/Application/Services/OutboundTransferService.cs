using System.Transactions;
using Harvestry.Compliance.Metrc.Application.Interfaces;
using Harvestry.Compliance.Metrc.Domain.Enums;
using Harvestry.Transfers.Application.DTOs;
using Harvestry.Transfers.Application.Interfaces;
using Harvestry.Transfers.Application.Mappers;
using Harvestry.Transfers.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Harvestry.Transfers.Application.Services;

public sealed class OutboundTransferService : IOutboundTransferService
{
    private readonly IShipmentTransferSourceReader _shipmentSourceReader;
    private readonly IOutboundTransferRepository _transferRepository;
    private readonly IOutboundTransferPackageRepository _packageRepository;
    private readonly ITransferEventRepository _eventRepository;
    private readonly ITransportManifestRepository _manifestRepository;
    private readonly IMetrcQueueService _metrcQueueService;
    private readonly ILogger<OutboundTransferService> _logger;

    public OutboundTransferService(
        IShipmentTransferSourceReader shipmentSourceReader,
        IOutboundTransferRepository transferRepository,
        IOutboundTransferPackageRepository packageRepository,
        ITransferEventRepository eventRepository,
        ITransportManifestRepository manifestRepository,
        IMetrcQueueService metrcQueueService,
        ILogger<OutboundTransferService> logger)
    {
        _shipmentSourceReader = shipmentSourceReader;
        _transferRepository = transferRepository;
        _packageRepository = packageRepository;
        _eventRepository = eventRepository;
        _manifestRepository = manifestRepository;
        _metrcQueueService = metrcQueueService;
        _logger = logger;
    }

    public async Task<OutboundTransferDto?> GetByIdAsync(Guid siteId, Guid transferId, CancellationToken cancellationToken = default)
    {
        var transfer = await _transferRepository.GetByIdAsync(siteId, transferId, cancellationToken);
        if (transfer == null) return null;

        var packages = await _packageRepository.GetByTransferIdAsync(siteId, transferId, cancellationToken);
        return transfer.ToDto(packages);
    }

    public async Task<OutboundTransferListResponse> GetBySiteAsync(Guid siteId, int page = 1, int pageSize = 50, string? status = null, CancellationToken cancellationToken = default)
    {
        var (transfers, total) = await _transferRepository.GetBySiteAsync(siteId, page, pageSize, status, cancellationToken);
        return new OutboundTransferListResponse
        {
            Transfers = transfers.Select(t => t.ToDto(packages: Array.Empty<OutboundTransferPackage>())).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<OutboundTransferDto> CreateFromShipmentAsync(Guid siteId, CreateOutboundTransferFromShipmentRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        if (request.ShipmentId == Guid.Empty) throw new ArgumentException("ShipmentId is required", nameof(request.ShipmentId));

        var source = await _shipmentSourceReader.GetAsync(siteId, request.ShipmentId, cancellationToken);
        if (source == null) throw new InvalidOperationException("Shipment not found or missing destination details.");
        if (string.IsNullOrWhiteSpace(source.DestinationLicenseNumber)) throw new InvalidOperationException("Destination license number is required to create a transfer.");
        if (source.Packages.Count == 0) throw new InvalidOperationException("Shipment has no packages.");

        var transfer = OutboundTransfer.CreateDraft(
            siteId,
            shipmentId: source.ShipmentId,
            salesOrderId: source.SalesOrderId,
            destinationLicenseNumber: source.DestinationLicenseNumber,
            destinationFacilityName: source.DestinationFacilityName,
            createdByUserId: userId);

        transfer.SetPlannedWindow(request.PlannedDepartureAt, request.PlannedArrivalAt, userId);

        var transferPackages = source.Packages.Select(p =>
            OutboundTransferPackage.Create(siteId, transfer.Id, p.PackageId, p.PackageLabel, p.Quantity, p.UnitOfMeasure)
        ).ToList();

        var evt = TransferEvent.Create(
            siteId,
            transfer.Id,
            eventType: "created_from_shipment",
            eventReason: null,
            metadata: new Dictionary<string, object>
            {
                ["shipmentId"] = source.ShipmentId,
                ["salesOrderId"] = source.SalesOrderId ?? Guid.Empty
            },
            createdByUserId: userId);

        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        await _transferRepository.AddAsync(transfer, cancellationToken);
        await _packageRepository.AddRangeAsync(transferPackages, cancellationToken);
        await _eventRepository.AddAsync(evt, cancellationToken);

        // Ensure there's a manifest record (draft) for data capture
        var existingManifest = await _manifestRepository.GetByTransferIdAsync(siteId, transfer.Id, cancellationToken);
        if (existingManifest == null)
        {
            var manifest = TransportManifest.CreateDraft(siteId, transfer.Id, userId);
            await _manifestRepository.AddAsync(manifest, cancellationToken);
        }

        await _transferRepository.SaveChangesAsync(cancellationToken);
        await _packageRepository.SaveChangesAsync(cancellationToken);
        await _eventRepository.SaveChangesAsync(cancellationToken);
        await _manifestRepository.SaveChangesAsync(cancellationToken);

        tx.Complete();

        _logger.LogInformation("Created outbound transfer {TransferId} from shipment {ShipmentId}", transfer.Id, source.ShipmentId);
        return transfer.ToDto(transferPackages);
    }

    public async Task<OutboundTransferDto?> MarkReadyAsync(Guid siteId, Guid transferId, Guid userId, CancellationToken cancellationToken = default)
    {
        var transfer = await _transferRepository.GetByIdAsync(siteId, transferId, cancellationToken);
        if (transfer == null) return null;

        transfer.MarkReady(userId);
        await _transferRepository.UpdateAsync(transfer, cancellationToken);
        await _transferRepository.SaveChangesAsync(cancellationToken);

        var packages = await _packageRepository.GetByTransferIdAsync(siteId, transferId, cancellationToken);
        return transfer.ToDto(packages);
    }

    public async Task<OutboundTransferDto?> SubmitToMetrcAsync(Guid siteId, Guid transferId, SubmitOutboundTransferToMetrcRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        if (request.MetrcSyncJobId == Guid.Empty) throw new ArgumentException("MetrcSyncJobId is required", nameof(request.MetrcSyncJobId));
        if (string.IsNullOrWhiteSpace(request.LicenseNumber)) throw new ArgumentException("LicenseNumber is required", nameof(request.LicenseNumber));

        var transfer = await _transferRepository.GetByIdAsync(siteId, transferId, cancellationToken);
        if (transfer == null) return null;

        var packages = await _packageRepository.GetByTransferIdAsync(siteId, transferId, cancellationToken);
        if (packages.Count == 0) throw new InvalidOperationException("Transfer has no packages.");

        var manifest = await _manifestRepository.GetByTransferIdAsync(siteId, transferId, cancellationToken);

        var payload = BuildMetrcTransferTemplatePayload(transfer, packages, manifest);

        // If we already have a template id, update; otherwise create.
        var operation = transfer.MetrcTransferTemplateId.HasValue ? MetrcOperationType.Update : MetrcOperationType.Create;

        await _metrcQueueService.EnqueueAsync(
            syncJobId: request.MetrcSyncJobId,
            siteId: siteId,
            licenseNumber: request.LicenseNumber.Trim(),
            entityType: MetrcEntityType.Transfer,
            operationType: operation,
            harvestryEntityId: transfer.Id,
            payload: payload,
            priority: request.Priority,
            metrcId: transfer.MetrcTransferTemplateId,
            metrcLabel: transfer.MetrcTransferNumber,
            dependsOnItemId: null,
            cancellationToken: cancellationToken);

        transfer.UpdateMetrcSync("queued", null, userId);
        await _transferRepository.UpdateAsync(transfer, cancellationToken);
        await _transferRepository.SaveChangesAsync(cancellationToken);

        return transfer.ToDto(packages);
    }

    public async Task<OutboundTransferDto?> VoidAsync(Guid siteId, Guid transferId, VoidOutboundTransferRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Reason)) throw new ArgumentException("Reason is required", nameof(request.Reason));

        var transfer = await _transferRepository.GetByIdAsync(siteId, transferId, cancellationToken);
        if (transfer == null) return null;

        var packages = await _packageRepository.GetByTransferIdAsync(siteId, transferId, cancellationToken);

        if (transfer.MetrcTransferTemplateId.HasValue)
        {
            if (request.MetrcSyncJobId == Guid.Empty) throw new ArgumentException("MetrcSyncJobId is required to void a submitted transfer.", nameof(request.MetrcSyncJobId));
            if (string.IsNullOrWhiteSpace(request.LicenseNumber)) throw new ArgumentException("LicenseNumber is required to void a submitted transfer.", nameof(request.LicenseNumber));

            await _metrcQueueService.EnqueueAsync(
                syncJobId: request.MetrcSyncJobId,
                siteId: siteId,
                licenseNumber: request.LicenseNumber.Trim(),
                entityType: MetrcEntityType.Transfer,
                operationType: MetrcOperationType.Delete,
                harvestryEntityId: transfer.Id,
                payload: new { },
                priority: 50,
                metrcId: transfer.MetrcTransferTemplateId,
                metrcLabel: transfer.MetrcTransferNumber,
                dependsOnItemId: null,
                cancellationToken: cancellationToken);
        }

        transfer.MarkVoided(request.Reason, userId);
        await _transferRepository.UpdateAsync(transfer, cancellationToken);
        await _transferRepository.SaveChangesAsync(cancellationToken);

        return transfer.ToDto(packages);
    }

    private static object BuildMetrcTransferTemplatePayload(
        OutboundTransfer transfer,
        IReadOnlyList<OutboundTransferPackage> packages,
        TransportManifest? manifest)
    {
        var destination = new
        {
            RecipientLicenseNumber = transfer.DestinationLicenseNumber,
            TransferTypeName = "Wholesale",
            PlannedRoute = (string?)null,
            EstimatedDepartureDateTime = transfer.PlannedDepartureAt,
            EstimatedArrivalDateTime = transfer.PlannedArrivalAt,
            Packages = packages.Select(p => new
            {
                PackageLabel = p.PackageLabel,
                WholesalePrice = (decimal?)null
            }).ToList()
        };

        return new
        {
            Name = $"Harvestry-{transfer.Id}",
            TransporterFacilityLicenseNumber = manifest?.TransporterLicenseNumber,
            DriverOccupationalLicenseNumber = (string?)null,
            DriverName = manifest?.DriverName,
            DriverLicenseNumber = manifest?.DriverLicenseNumber,
            PhoneNumberForQuestions = manifest?.DriverPhone,
            VehicleMake = manifest?.VehicleMake,
            VehicleModel = manifest?.VehicleModel,
            VehicleLicensePlateNumber = manifest?.VehiclePlate,
            Destinations = new[] { destination }
        };
    }
}

