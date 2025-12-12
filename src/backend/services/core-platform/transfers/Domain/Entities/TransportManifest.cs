using Harvestry.Shared.Kernel.Domain;
using Harvestry.Transfers.Domain.Enums;

namespace Harvestry.Transfers.Domain.Entities;

public sealed class TransportManifest : AggregateRoot<Guid>
{
    private TransportManifest(Guid id) : base(id) { }

    private TransportManifest(
        Guid id,
        Guid siteId,
        Guid outboundTransferId,
        Guid createdByUserId) : base(id)
    {
        if (siteId == Guid.Empty) throw new ArgumentException("SiteId cannot be empty", nameof(siteId));
        if (outboundTransferId == Guid.Empty) throw new ArgumentException("OutboundTransferId cannot be empty", nameof(outboundTransferId));
        if (createdByUserId == Guid.Empty) throw new ArgumentException("CreatedByUserId cannot be empty", nameof(createdByUserId));

        SiteId = siteId;
        OutboundTransferId = outboundTransferId;
        Status = ManifestStatus.Draft;

        CreatedAt = DateTime.UtcNow;
        CreatedByUserId = createdByUserId;
        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = createdByUserId;
    }

    public Guid SiteId { get; private set; }
    public Guid OutboundTransferId { get; private set; }

    public ManifestStatus Status { get; private set; }

    public string? TransporterName { get; private set; }
    public string? TransporterLicenseNumber { get; private set; }
    public string? DriverName { get; private set; }
    public string? DriverLicenseNumber { get; private set; }
    public string? DriverPhone { get; private set; }

    public string? VehicleMake { get; private set; }
    public string? VehicleModel { get; private set; }
    public string? VehiclePlate { get; private set; }

    public DateTime? DepartureAt { get; private set; }
    public DateTime? ArrivalAt { get; private set; }

    public string? MetrcManifestNumber { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    public static TransportManifest CreateDraft(Guid siteId, Guid outboundTransferId, Guid createdByUserId)
    {
        return new TransportManifest(Guid.NewGuid(), siteId, outboundTransferId, createdByUserId);
    }

    public void SetTransporter(string? transporterName, string? transporterLicenseNumber, Guid userId)
    {
        EnsureMutable();
        TransporterName = transporterName?.Trim();
        TransporterLicenseNumber = transporterLicenseNumber?.Trim();
        Touch(userId);
    }

    public void SetDriver(string? driverName, string? driverLicenseNumber, string? driverPhone, Guid userId)
    {
        EnsureMutable();
        DriverName = driverName?.Trim();
        DriverLicenseNumber = driverLicenseNumber?.Trim();
        DriverPhone = driverPhone?.Trim();
        Touch(userId);
    }

    public void SetVehicle(string? make, string? model, string? plate, Guid userId)
    {
        EnsureMutable();
        VehicleMake = make?.Trim();
        VehicleModel = model?.Trim();
        VehiclePlate = plate?.Trim();
        Touch(userId);
    }

    public void MarkReady(Guid userId)
    {
        EnsureMutable();
        Status = ManifestStatus.Ready;
        Touch(userId);
    }

    public void MarkSubmittedToMetrc(string? metrcManifestNumber, Guid userId)
    {
        if (Status == ManifestStatus.Voided) throw new InvalidOperationException("Manifest is voided.");
        Status = ManifestStatus.SubmittedToMetrc;
        MetrcManifestNumber = metrcManifestNumber?.Trim();
        Touch(userId);
    }

    public void Void(string reason, Guid userId)
    {
        if (Status == ManifestStatus.Voided) return;
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Void reason is required", nameof(reason));
        Status = ManifestStatus.Voided;
        Touch(userId);
    }

    public void SetTimes(DateTime? departureAt, DateTime? arrivalAt, Guid userId)
    {
        EnsureMutable();
        DepartureAt = departureAt;
        ArrivalAt = arrivalAt;
        Touch(userId);
    }

    private void EnsureMutable()
    {
        if (Status == ManifestStatus.Voided) throw new InvalidOperationException("Manifest is voided.");
        if (Status == ManifestStatus.SubmittedToMetrc) throw new InvalidOperationException("Manifest has been submitted.");
    }

    private void Touch(Guid userId)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId cannot be empty", nameof(userId));
        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = userId;
    }
}

