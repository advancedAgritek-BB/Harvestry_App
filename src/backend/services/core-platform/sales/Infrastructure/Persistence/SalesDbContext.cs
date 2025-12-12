using Harvestry.Sales.Domain.Entities;
using Harvestry.Sales.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Sales.Infrastructure.Persistence;

public sealed class SalesDbContext : DbContext
{
    public SalesDbContext(DbContextOptions<SalesDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();
    public DbSet<SalesAllocation> SalesAllocations => Set<SalesAllocation>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentPackage> ShipmentPackages => Set<ShipmentPackage>();
    public DbSet<ComplianceEvent> ComplianceEvents => Set<ComplianceEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureCustomer(modelBuilder);
        ConfigureSalesOrder(modelBuilder);
        ConfigureSalesOrderLine(modelBuilder);
        ConfigureSalesAllocation(modelBuilder);
        ConfigureShipment(modelBuilder);
        ConfigureShipmentPackage(modelBuilder);
        ConfigureComplianceEvent(modelBuilder);
    }

    private static void ConfigureCustomer(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SiteId).HasColumnName("site_id");

            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200);
            entity.Property(e => e.LicenseNumber).HasColumnName("license_number").HasMaxLength(100);
            entity.Property(e => e.FacilityName).HasColumnName("facility_name").HasMaxLength(200);
            entity.Property(e => e.FacilityType).HasColumnName("facility_type").HasMaxLength(50);

            entity.Property(e => e.Address).HasColumnName("address").HasMaxLength(500);
            entity.Property(e => e.City).HasColumnName("city").HasMaxLength(100);
            entity.Property(e => e.State).HasColumnName("state").HasMaxLength(50);
            entity.Property(e => e.Zip).HasColumnName("zip").HasMaxLength(20);

            entity.Property(e => e.PrimaryContactName).HasColumnName("primary_contact_name").HasMaxLength(200);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(200);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(50);

            entity.Property(e => e.LicenseVerifiedStatus).HasColumnName("license_verified_status").HasConversion<string>();
            entity.Property(e => e.LicenseVerifiedAt).HasColumnName("license_verified_at");
            entity.Property(e => e.LicenseVerificationSource).HasColumnName("license_verification_source").HasMaxLength(100);
            entity.Property(e => e.LicenseVerificationNotes).HasColumnName("license_verification_notes");
            entity.Property(e => e.MetrcRecipientId).HasColumnName("metrc_recipient_id").HasMaxLength(100);

            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.Tags).HasColumnName("tags").HasMaxLength(500);

            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedByUserId).HasColumnName("updated_by_user_id");

            entity.HasIndex(e => new { e.SiteId, e.LicenseNumber }).IsUnique();
            entity.HasIndex(e => new { e.SiteId, e.Name });

            entity.Ignore(e => e.DomainEvents);
        });
    }

    private static void ConfigureSalesOrder(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SalesOrder>(entity =>
        {
            entity.ToTable("sales_orders");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.OrderNumber).HasColumnName("order_number").HasMaxLength(40);

            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.CustomerName).HasColumnName("customer_name").HasMaxLength(200);
            entity.Property(e => e.DestinationLicenseNumber).HasColumnName("destination_license_number").HasMaxLength(100);
            entity.Property(e => e.DestinationFacilityName).HasColumnName("destination_facility_name").HasMaxLength(200);

            entity.Property(e => e.RequestedShipDate).HasColumnName("requested_ship_date");
            entity.Property(e => e.SubmittedAt).HasColumnName("submitted_at");
            entity.Property(e => e.CancelledAt).HasColumnName("cancelled_at");

            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.Notes).HasColumnName("notes");

            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedByUserId).HasColumnName("updated_by_user_id");

            entity.HasIndex(e => new { e.SiteId, e.OrderNumber }).IsUnique();

            entity.Ignore(e => e.DomainEvents);
            entity.Ignore(e => e.Lines);
        });
    }

    private static void ConfigureSalesOrderLine(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SalesOrderLine>(entity =>
        {
            entity.ToTable("sales_order_lines");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.SalesOrderId).HasColumnName("sales_order_id");
            entity.Property(e => e.LineNumber).HasColumnName("line_number");

            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.ItemName).HasColumnName("item_name").HasMaxLength(200);
            entity.Property(e => e.UnitOfMeasure).HasColumnName("unit_of_measure").HasMaxLength(20);

            entity.Property(e => e.RequestedQuantity).HasColumnName("requested_quantity").HasPrecision(12, 4);
            entity.Property(e => e.AllocatedQuantity).HasColumnName("allocated_quantity").HasPrecision(12, 4);
            entity.Property(e => e.ShippedQuantity).HasColumnName("shipped_quantity").HasPrecision(12, 4);

            entity.Property(e => e.UnitPrice).HasColumnName("unit_price").HasPrecision(12, 4);
            entity.Property(e => e.CurrencyCode).HasColumnName("currency_code").HasMaxLength(3);

            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");

            entity.HasIndex(e => new { e.SalesOrderId, e.LineNumber }).IsUnique();
            entity.Ignore(e => e.DomainEvents);
        });
    }

    private static void ConfigureSalesAllocation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SalesAllocation>(entity =>
        {
            entity.ToTable("sales_allocations");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.SalesOrderId).HasColumnName("sales_order_id");
            entity.Property(e => e.SalesOrderLineId).HasColumnName("sales_order_line_id");

            entity.Property(e => e.PackageId).HasColumnName("package_id");
            entity.Property(e => e.PackageLabel).HasColumnName("package_label").HasMaxLength(30);
            entity.Property(e => e.AllocatedQuantity).HasColumnName("allocated_quantity").HasPrecision(12, 4);
            entity.Property(e => e.UnitOfMeasure).HasColumnName("unit_of_measure").HasMaxLength(20);

            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CancelledAt).HasColumnName("cancelled_at");
            entity.Property(e => e.CancelledByUserId).HasColumnName("cancelled_by_user_id");
            entity.Property(e => e.CancelReason).HasColumnName("cancel_reason");

            entity.Ignore(e => e.DomainEvents);
            entity.Ignore(e => e.IsCancelled);
        });
    }

    private static void ConfigureShipment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.ToTable("shipments");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.ShipmentNumber).HasColumnName("shipment_number").HasMaxLength(40);
            entity.Property(e => e.SalesOrderId).HasColumnName("sales_order_id");

            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.PickingStartedAt).HasColumnName("picking_started_at");
            entity.Property(e => e.PackedAt).HasColumnName("packed_at");
            entity.Property(e => e.ShippedAt).HasColumnName("shipped_at");
            entity.Property(e => e.CancelledAt).HasColumnName("cancelled_at");

            entity.Property(e => e.CarrierName).HasColumnName("carrier_name").HasMaxLength(200);
            entity.Property(e => e.TrackingNumber).HasColumnName("tracking_number").HasMaxLength(100);
            entity.Property(e => e.Notes).HasColumnName("notes");

            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedByUserId).HasColumnName("updated_by_user_id");

            entity.HasIndex(e => new { e.SiteId, e.ShipmentNumber }).IsUnique();

            entity.Ignore(e => e.DomainEvents);
            entity.Ignore(e => e.Packages);
        });
    }

    private static void ConfigureShipmentPackage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShipmentPackage>(entity =>
        {
            entity.ToTable("shipment_packages");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.ShipmentId).HasColumnName("shipment_id");
            entity.Property(e => e.SalesAllocationId).HasColumnName("sales_allocation_id");

            entity.Property(e => e.PackageId).HasColumnName("package_id");
            entity.Property(e => e.PackageLabel).HasColumnName("package_label").HasMaxLength(30);
            entity.Property(e => e.Quantity).HasColumnName("quantity").HasPrecision(12, 4);
            entity.Property(e => e.UnitOfMeasure).HasColumnName("unit_of_measure").HasMaxLength(20);

            entity.Property(e => e.PackedAt).HasColumnName("packed_at");
            entity.Property(e => e.PackedByUserId).HasColumnName("packed_by_user_id");

            entity.Ignore(e => e.DomainEvents);
        });
    }

    private static void ConfigureComplianceEvent(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ComplianceEvent>(entity =>
        {
            entity.ToTable("compliance_events");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.EntityType).HasColumnName("entity_type").HasMaxLength(50);
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.EventType).HasColumnName("event_type").HasMaxLength(100);
            entity.Property(e => e.PayloadJson).HasColumnName("payload_json");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");

            entity.HasIndex(e => new { e.SiteId, e.EntityType, e.EntityId });
            entity.HasIndex(e => new { e.SiteId, e.CreatedAt });

            entity.Ignore(e => e.DomainEvents);
        });
    }
}

