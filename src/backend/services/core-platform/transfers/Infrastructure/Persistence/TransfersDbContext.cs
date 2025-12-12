using Harvestry.Transfers.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Transfers.Infrastructure.Persistence;

public sealed class TransfersDbContext : DbContext
{
    public TransfersDbContext(DbContextOptions<TransfersDbContext> options) : base(options) { }

    public DbSet<OutboundTransfer> OutboundTransfers => Set<OutboundTransfer>();
    public DbSet<OutboundTransferPackage> OutboundTransferPackages => Set<OutboundTransferPackage>();
    public DbSet<TransportManifest> TransportManifests => Set<TransportManifest>();
    public DbSet<InboundTransferReceipt> InboundReceipts => Set<InboundTransferReceipt>();
    public DbSet<InboundTransferReceiptLine> InboundReceiptLines => Set<InboundTransferReceiptLine>();
    public DbSet<TransferEvent> TransferEvents => Set<TransferEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureOutboundTransfer(modelBuilder);
        ConfigureOutboundTransferPackage(modelBuilder);
        ConfigureTransportManifest(modelBuilder);
        ConfigureInboundReceipt(modelBuilder);
        ConfigureInboundReceiptLine(modelBuilder);
        ConfigureTransferEvent(modelBuilder);
    }

    private static void ConfigureOutboundTransfer(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboundTransfer>(entity =>
        {
            entity.ToTable("outbound_transfers");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.ShipmentId).HasColumnName("shipment_id");
            entity.Property(e => e.SalesOrderId).HasColumnName("sales_order_id");

            entity.Property(e => e.DestinationLicenseNumber).HasColumnName("destination_license_number").HasMaxLength(100);
            entity.Property(e => e.DestinationFacilityName).HasColumnName("destination_facility_name").HasMaxLength(200);

            entity.Property(e => e.PlannedDepartureAt).HasColumnName("planned_departure_at");
            entity.Property(e => e.PlannedArrivalAt).HasColumnName("planned_arrival_at");

            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.StatusReason).HasColumnName("status_reason");

            entity.Property(e => e.MetrcTransferTemplateId).HasColumnName("metrc_transfer_template_id");
            entity.Property(e => e.MetrcTransferNumber).HasColumnName("metrc_transfer_number").HasMaxLength(50);
            entity.Property(e => e.MetrcLastSubmittedAt).HasColumnName("metrc_last_submitted_at");
            entity.Property(e => e.MetrcSyncStatus).HasColumnName("metrc_sync_status");
            entity.Property(e => e.MetrcSyncError).HasColumnName("metrc_sync_error");

            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedByUserId).HasColumnName("updated_by_user_id");

            entity.Ignore(e => e.Packages);
            entity.Ignore(e => e.DomainEvents);
        });
    }

    private static void ConfigureOutboundTransferPackage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboundTransferPackage>(entity =>
        {
            entity.ToTable("outbound_transfer_packages");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.OutboundTransferId).HasColumnName("outbound_transfer_id");
            entity.Property(e => e.PackageId).HasColumnName("package_id");
            entity.Property(e => e.PackageLabel).HasColumnName("package_label").HasMaxLength(30);
            entity.Property(e => e.Quantity).HasColumnName("quantity").HasPrecision(12, 4);
            entity.Property(e => e.UnitOfMeasure).HasColumnName("unit_of_measure").HasMaxLength(20);

            entity.Ignore(e => e.DomainEvents);
        });
    }

    private static void ConfigureTransportManifest(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TransportManifest>(entity =>
        {
            entity.ToTable("transport_manifests");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.OutboundTransferId).HasColumnName("outbound_transfer_id");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();

            entity.Property(e => e.TransporterName).HasColumnName("transporter_name").HasMaxLength(200);
            entity.Property(e => e.TransporterLicenseNumber).HasColumnName("transporter_license_number").HasMaxLength(100);

            entity.Property(e => e.DriverName).HasColumnName("driver_name").HasMaxLength(200);
            entity.Property(e => e.DriverLicenseNumber).HasColumnName("driver_license_number").HasMaxLength(100);
            entity.Property(e => e.DriverPhone).HasColumnName("driver_phone").HasMaxLength(30);

            entity.Property(e => e.VehicleMake).HasColumnName("vehicle_make").HasMaxLength(100);
            entity.Property(e => e.VehicleModel).HasColumnName("vehicle_model").HasMaxLength(100);
            entity.Property(e => e.VehiclePlate).HasColumnName("vehicle_plate").HasMaxLength(30);

            entity.Property(e => e.DepartureAt).HasColumnName("departure_at");
            entity.Property(e => e.ArrivalAt).HasColumnName("arrival_at");
            entity.Property(e => e.MetrcManifestNumber).HasColumnName("metrc_manifest_number").HasMaxLength(50);

            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedByUserId).HasColumnName("updated_by_user_id");

            entity.Ignore(e => e.DomainEvents);
        });
    }

    private static void ConfigureInboundReceipt(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InboundTransferReceipt>(entity =>
        {
            entity.ToTable("inbound_transfer_receipts");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.OutboundTransferId).HasColumnName("outbound_transfer_id");
            entity.Property(e => e.MetrcTransferId).HasColumnName("metrc_transfer_id");
            entity.Property(e => e.MetrcTransferNumber).HasColumnName("metrc_transfer_number").HasMaxLength(50);

            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.ReceivedAt).HasColumnName("received_at");
            entity.Property(e => e.ReceivedByUserId).HasColumnName("received_by_user_id");
            entity.Property(e => e.Notes).HasColumnName("notes");

            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedByUserId).HasColumnName("updated_by_user_id");

            entity.Ignore(e => e.Lines);
            entity.Ignore(e => e.DomainEvents);
        });
    }

    private static void ConfigureInboundReceiptLine(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InboundTransferReceiptLine>(entity =>
        {
            entity.ToTable("inbound_transfer_receipt_lines");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.InboundReceiptId).HasColumnName("inbound_receipt_id");
            entity.Property(e => e.PackageLabel).HasColumnName("package_label").HasMaxLength(30);
            entity.Property(e => e.ReceivedQuantity).HasColumnName("received_quantity").HasPrecision(12, 4);
            entity.Property(e => e.UnitOfMeasure).HasColumnName("unit_of_measure").HasMaxLength(20);
            entity.Property(e => e.Accepted).HasColumnName("accepted");
            entity.Property(e => e.RejectionReason).HasColumnName("rejection_reason");

            entity.Ignore(e => e.DomainEvents);
        });
    }

    private static void ConfigureTransferEvent(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TransferEvent>(entity =>
        {
            entity.ToTable("transfer_events");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.OutboundTransferId).HasColumnName("outbound_transfer_id");
            entity.Property(e => e.EventType).HasColumnName("event_type").HasMaxLength(50);
            entity.Property(e => e.EventReason).HasColumnName("event_reason");
            entity.Property(e => e.Metadata).HasColumnName("metadata").HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");

            entity.Ignore(e => e.DomainEvents);
        });
    }
}

