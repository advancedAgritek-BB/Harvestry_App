using Harvestry.Packages.Domain.Entities;
using Harvestry.Packages.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Packages.Infrastructure.Persistence;

/// <summary>
/// DbContext for Packages domain
/// </summary>
public class PackagesDbContext : DbContext
{
    public PackagesDbContext(DbContextOptions<PackagesDbContext> options) : base(options)
    {
    }

    public DbSet<Package> Packages => Set<Package>();
    public DbSet<InventoryMovement> Movements => Set<InventoryMovement>();
    public DbSet<PackageAdjustment> Adjustments => Set<PackageAdjustment>();
    public DbSet<PackageRemediation> Remediations => Set<PackageRemediation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigurePackage(modelBuilder);
        ConfigureMovement(modelBuilder);
        ConfigureAdjustment(modelBuilder);
        ConfigureRemediation(modelBuilder);
    }

    private static void ConfigurePackage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Package>(entity =>
        {
            entity.ToTable("packages");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            
            entity.Property(e => e.PackageLabel)
                .HasColumnName("package_label")
                .HasMaxLength(30)
                .HasConversion(v => v.Value, v => PackageLabel.Create(v));

            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.ItemName).HasColumnName("item_name").HasMaxLength(200);
            entity.Property(e => e.ItemCategory).HasColumnName("item_category").HasMaxLength(50);

            entity.Property(e => e.Quantity).HasColumnName("quantity").HasPrecision(12, 4);
            entity.Property(e => e.InitialQuantity).HasColumnName("initial_quantity").HasPrecision(12, 4);
            entity.Property(e => e.UnitOfMeasure).HasColumnName("unit_of_measure").HasMaxLength(20);

            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.LocationName).HasColumnName("location_name").HasMaxLength(200);
            entity.Property(e => e.SublocationName).HasColumnName("sublocation_name").HasMaxLength(100);

            entity.Property(e => e.SourceHarvestId).HasColumnName("source_harvest_id");
            entity.Property(e => e.SourceHarvestName).HasColumnName("source_harvest_name").HasMaxLength(200);
            entity.Property(e => e.ProductionBatchNumber).HasColumnName("production_batch_number").HasMaxLength(50);
            entity.Property(e => e.IsProductionBatch).HasColumnName("is_production_batch");

            entity.Property(e => e.IsTradeSample).HasColumnName("is_trade_sample");
            entity.Property(e => e.IsDonation).HasColumnName("is_donation");
            entity.Property(e => e.ProductRequiresRemediation).HasColumnName("product_requires_remediation");
            entity.Property(e => e.PatientLicenseNumber).HasColumnName("patient_license_number").HasMaxLength(50);

            entity.Property(e => e.PackagedDate).HasColumnName("packaged_date");
            entity.Property(e => e.ExpirationDate).HasColumnName("expiration_date");
            entity.Property(e => e.UseByDate).HasColumnName("use_by_date");
            entity.Property(e => e.FinishedDate).HasColumnName("finished_date");

            entity.Property(e => e.LabTestingState).HasColumnName("lab_testing_state").HasConversion<string>();
            entity.Property(e => e.LabTestingStateRequired).HasColumnName("lab_testing_state_required");
            entity.Property(e => e.ThcPercent).HasColumnName("thc_percent").HasPrecision(6, 3);
            entity.Property(e => e.CbdPercent).HasColumnName("cbd_percent").HasPrecision(6, 3);

            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.PackageType).HasColumnName("package_type").HasConversion<string>();
            entity.Property(e => e.Notes).HasColumnName("notes");

            entity.Property(e => e.MetrcPackageId).HasColumnName("metrc_package_id");
            entity.Property(e => e.MetrcLastSyncAt).HasColumnName("metrc_last_sync_at");
            entity.Property(e => e.MetrcSyncStatus).HasColumnName("metrc_sync_status").HasMaxLength(30);

            // WMS fields
            entity.Property(e => e.UnitCost).HasColumnName("unit_cost").HasPrecision(12, 4);
            entity.Property(e => e.MaterialCost).HasColumnName("material_cost").HasPrecision(12, 4);
            entity.Property(e => e.LaborCost).HasColumnName("labor_cost").HasPrecision(12, 4);
            entity.Property(e => e.OverheadCost).HasColumnName("overhead_cost").HasPrecision(12, 4);
            entity.Property(e => e.ReservedQuantity).HasColumnName("reserved_quantity").HasPrecision(12, 4);
            entity.Property(e => e.InventoryCategory).HasColumnName("inventory_category").HasConversion<string>();

            entity.Property(e => e.HoldReasonCode).HasColumnName("hold_reason_code").HasConversion<string>();
            entity.Property(e => e.HoldPlacedAt).HasColumnName("hold_placed_at");
            entity.Property(e => e.HoldPlacedByUserId).HasColumnName("hold_placed_by_user_id");
            entity.Property(e => e.HoldReleasedAt).HasColumnName("hold_released_at");
            entity.Property(e => e.HoldReleasedByUserId).HasColumnName("hold_released_by_user_id");
            entity.Property(e => e.RequiresTwoPersonRelease).HasColumnName("requires_two_person_release");
            entity.Property(e => e.HoldFirstApproverId).HasColumnName("hold_first_approver_id");
            entity.Property(e => e.HoldFirstApprovedAt).HasColumnName("hold_first_approved_at");

            entity.Property(e => e.VendorId).HasColumnName("vendor_id");
            entity.Property(e => e.VendorName).HasColumnName("vendor_name").HasMaxLength(200);
            entity.Property(e => e.VendorLotNumber).HasColumnName("vendor_lot_number").HasMaxLength(100);
            entity.Property(e => e.PurchaseOrderId).HasColumnName("purchase_order_id");
            entity.Property(e => e.PurchaseOrderNumber).HasColumnName("purchase_order_number").HasMaxLength(50);
            entity.Property(e => e.ReceivedDate).HasColumnName("received_date");

            entity.Property(e => e.Grade).HasColumnName("grade").HasConversion<string>();
            entity.Property(e => e.QualityScore).HasColumnName("quality_score").HasPrecision(5, 2);
            entity.Property(e => e.QualityNotes).HasColumnName("quality_notes");

            entity.Property(e => e.GenerationDepth).HasColumnName("generation_depth");
            entity.Property(e => e.RootAncestorId).HasColumnName("root_ancestor_id");
            entity.Property(e => e.AncestryPath).HasColumnName("ancestry_path");

            entity.Property(e => e.Metadata).HasColumnName("metadata").HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedByUserId).HasColumnName("updated_by_user_id");

            entity.HasIndex(e => new { e.SiteId, e.Status });
            entity.HasIndex(e => e.PackageLabel).IsUnique();
            entity.HasIndex(e => new { e.SiteId, e.ItemId });
            entity.HasIndex(e => new { e.SiteId, e.LocationId });
            entity.HasIndex(e => new { e.SiteId, e.ExpirationDate });

            entity.Ignore(e => e.Adjustments);
            entity.Ignore(e => e.Remediations);
            entity.Ignore(e => e.SourcePackageLabels);
        });
    }

    private static void ConfigureMovement(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InventoryMovement>(entity =>
        {
            entity.ToTable("inventory_movements");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.MovementType).HasColumnName("movement_type").HasConversion<string>();
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();

            entity.Property(e => e.PackageId).HasColumnName("package_id");
            entity.Property(e => e.PackageLabel).HasColumnName("package_label").HasMaxLength(30);
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.ItemName).HasColumnName("item_name").HasMaxLength(200);

            entity.Property(e => e.FromLocationId).HasColumnName("from_location_id");
            entity.Property(e => e.FromLocationPath).HasColumnName("from_location_path").HasMaxLength(500);
            entity.Property(e => e.ToLocationId).HasColumnName("to_location_id");
            entity.Property(e => e.ToLocationPath).HasColumnName("to_location_path").HasMaxLength(500);

            entity.Property(e => e.Quantity).HasColumnName("quantity").HasPrecision(12, 4);
            entity.Property(e => e.UnitOfMeasure).HasColumnName("unit_of_measure").HasMaxLength(20);
            entity.Property(e => e.QuantityBefore).HasColumnName("quantity_before").HasPrecision(12, 4);
            entity.Property(e => e.QuantityAfter).HasColumnName("quantity_after").HasPrecision(12, 4);

            entity.Property(e => e.UnitCost).HasColumnName("unit_cost").HasPrecision(12, 4);
            entity.Property(e => e.TotalCost).HasColumnName("total_cost").HasPrecision(12, 4);

            entity.Property(e => e.ReasonCode).HasColumnName("reason_code").HasMaxLength(50);
            entity.Property(e => e.ReasonNotes).HasColumnName("reason_notes");

            entity.Property(e => e.ProcessingJobId).HasColumnName("processing_job_id");
            entity.Property(e => e.ProcessingJobNumber).HasColumnName("processing_job_number").HasMaxLength(50);

            entity.Property(e => e.SalesOrderId).HasColumnName("sales_order_id");
            entity.Property(e => e.SalesOrderNumber).HasColumnName("sales_order_number").HasMaxLength(50);
            entity.Property(e => e.TransferId).HasColumnName("transfer_id");

            entity.Property(e => e.MetrcManifestId).HasColumnName("metrc_manifest_id").HasMaxLength(50);
            entity.Property(e => e.BiotrackTransferId).HasColumnName("biotrack_transfer_id").HasMaxLength(50);
            entity.Property(e => e.SyncStatus).HasColumnName("sync_status").HasMaxLength(20);
            entity.Property(e => e.SyncError).HasColumnName("sync_error");
            entity.Property(e => e.SyncedAt).HasColumnName("synced_at");

            entity.Property(e => e.VerifiedByUserId).HasColumnName("verified_by_user_id");
            entity.Property(e => e.VerifiedAt).HasColumnName("verified_at");
            entity.Property(e => e.ScanData).HasColumnName("scan_data");
            entity.Property(e => e.BarcodeScanned).HasColumnName("barcode_scanned").HasMaxLength(100);

            entity.Property(e => e.Notes).HasColumnName("notes");

            entity.Property(e => e.RequiresApproval).HasColumnName("requires_approval");
            entity.Property(e => e.FirstApproverId).HasColumnName("first_approver_id");
            entity.Property(e => e.FirstApprovedAt).HasColumnName("first_approved_at");
            entity.Property(e => e.SecondApproverId).HasColumnName("second_approver_id");
            entity.Property(e => e.SecondApprovedAt).HasColumnName("second_approved_at");
            entity.Property(e => e.RejectionReason).HasColumnName("rejection_reason");

            entity.Property(e => e.BatchMovementId).HasColumnName("batch_movement_id");
            entity.Property(e => e.BatchSequence).HasColumnName("batch_sequence");

            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CompletedByUserId).HasColumnName("completed_by_user_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.Property(e => e.Metadata).HasColumnName("metadata_json").HasColumnType("jsonb");

            entity.HasIndex(e => new { e.SiteId, e.CreatedAt });
            entity.HasIndex(e => e.PackageId);
            entity.HasIndex(e => e.MovementType);
            entity.HasIndex(e => e.BatchMovementId);

            entity.Ignore(e => e.EvidenceUrls);
            entity.Ignore(e => e.PhotoUrls);
            entity.Ignore(e => e.SourcePackageIds);
            entity.Ignore(e => e.TargetPackageIds);
        });
    }

    private static void ConfigureAdjustment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PackageAdjustment>(entity =>
        {
            entity.ToTable("package_adjustments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PackageId).HasColumnName("package_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity").HasPrecision(12, 4);
            entity.Property(e => e.UnitOfMeasure).HasColumnName("unit_of_measure").HasMaxLength(20);
            entity.Property(e => e.Reason).HasColumnName("reason").HasConversion<string>();
            entity.Property(e => e.ReasonNote).HasColumnName("reason_note");
            entity.Property(e => e.AdjustmentDate).HasColumnName("adjustment_date");
            entity.Property(e => e.PerformedByUserId).HasColumnName("performed_by_user_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });
    }

    private static void ConfigureRemediation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PackageRemediation>(entity =>
        {
            entity.ToTable("package_remediations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PackageId).HasColumnName("package_id");
            entity.Property(e => e.RemediationMethodName).HasColumnName("remediation_method_name").HasMaxLength(200);
            entity.Property(e => e.RemediationSteps).HasColumnName("remediation_steps");
            entity.Property(e => e.RemediationDate).HasColumnName("remediation_date");
            entity.Property(e => e.PerformedByUserId).HasColumnName("performed_by_user_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });
    }
}




