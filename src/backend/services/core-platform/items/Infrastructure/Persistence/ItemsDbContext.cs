using Harvestry.Items.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Items.Infrastructure.Persistence;

/// <summary>
/// DbContext for Items domain
/// </summary>
public class ItemsDbContext : DbContext
{
    public ItemsDbContext(DbContextOptions<ItemsDbContext> options) : base(options)
    {
    }

    public DbSet<Item> Items => Set<Item>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Item>(entity =>
        {
            entity.ToTable("items");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200);
            entity.Property(e => e.Category).HasColumnName("category").HasConversion<string>();
            entity.Property(e => e.UnitOfMeasure).HasColumnName("unit_of_measure").HasConversion<string>();
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();

            // Strain
            entity.Property(e => e.StrainId).HasColumnName("strain_id");
            entity.Property(e => e.StrainName).HasColumnName("strain_name").HasMaxLength(200);

            // Unit weight
            entity.Property(e => e.UnitWeight).HasColumnName("unit_weight").HasPrecision(12, 4);
            entity.Property(e => e.UnitWeightUnitOfMeasure).HasColumnName("unit_weight_unit_of_measure").HasMaxLength(20);

            // Potency
            entity.Property(e => e.DefaultThcPercent).HasColumnName("default_thc_percent").HasPrecision(6, 3);
            entity.Property(e => e.DefaultThcContent).HasColumnName("default_thc_content").HasPrecision(12, 4);
            entity.Property(e => e.DefaultThcContentUnitOfMeasure).HasColumnName("default_thc_content_uom").HasMaxLength(20);
            entity.Property(e => e.DefaultCbdPercent).HasColumnName("default_cbd_percent").HasPrecision(6, 3);
            entity.Property(e => e.DefaultCbdContent).HasColumnName("default_cbd_content").HasPrecision(12, 4);

            // Lab testing
            entity.Property(e => e.RequiresLabTesting).HasColumnName("requires_lab_testing");
            entity.Property(e => e.DefaultLabTestingState).HasColumnName("default_lab_testing_state").HasMaxLength(30);

            // Details
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(1000);
            entity.Property(e => e.Sku).HasColumnName("sku").HasMaxLength(100);
            entity.Property(e => e.Barcode).HasColumnName("barcode").HasMaxLength(100);

            // METRC
            entity.Property(e => e.MetrcItemId).HasColumnName("metrc_item_id");
            entity.Property(e => e.MetrcLastSyncAt).HasColumnName("metrc_last_sync_at");
            entity.Property(e => e.MetrcSyncStatus).HasColumnName("metrc_sync_status").HasMaxLength(30);

            // WMS - Classification
            entity.Property(e => e.InventoryCategory).HasColumnName("inventory_category").HasMaxLength(30);
            entity.Property(e => e.IsLotTracked).HasColumnName("is_lot_tracked");
            entity.Property(e => e.IsSerialTracked).HasColumnName("is_serial_tracked");

            // WMS - Reorder
            entity.Property(e => e.ReorderPoint).HasColumnName("reorder_point").HasPrecision(12, 4);
            entity.Property(e => e.ReorderQuantity).HasColumnName("reorder_quantity").HasPrecision(12, 4);
            entity.Property(e => e.SafetyStock).HasColumnName("safety_stock").HasPrecision(12, 4);
            entity.Property(e => e.LeadTimeDays).HasColumnName("lead_time_days");
            entity.Property(e => e.MinOrderQuantity).HasColumnName("min_order_quantity").HasPrecision(12, 4);
            entity.Property(e => e.MaxOrderQuantity).HasColumnName("max_order_quantity").HasPrecision(12, 4);

            // WMS - Pricing
            entity.Property(e => e.ListPrice).HasColumnName("list_price").HasPrecision(12, 4);
            entity.Property(e => e.WholesalePrice).HasColumnName("wholesale_price").HasPrecision(12, 4);
            entity.Property(e => e.CostPrice).HasColumnName("cost_price").HasPrecision(12, 4);
            entity.Property(e => e.MarginPercent).HasColumnName("margin_percent").HasPrecision(5, 2);

            // WMS - Flags
            entity.Property(e => e.IsSellable).HasColumnName("is_sellable");
            entity.Property(e => e.IsPurchasable).HasColumnName("is_purchasable");
            entity.Property(e => e.IsProducible).HasColumnName("is_producible");
            entity.Property(e => e.IsActiveForSale).HasColumnName("is_active_for_sale");

            // WMS - Default locations
            entity.Property(e => e.DefaultReceivingLocationId).HasColumnName("default_receiving_location_id");
            entity.Property(e => e.DefaultStorageLocationId).HasColumnName("default_storage_location_id");
            entity.Property(e => e.DefaultProductionLocationId).HasColumnName("default_production_location_id");

            // WMS - Shelf life
            entity.Property(e => e.ShelfLifeDays).HasColumnName("shelf_life_days");
            entity.Property(e => e.RequiresExpirationDate).HasColumnName("requires_expiration_date");

            // WMS - Weight
            entity.Property(e => e.StandardWeight).HasColumnName("standard_weight").HasPrecision(12, 4);
            entity.Property(e => e.StandardWeightUom).HasColumnName("standard_weight_uom").HasMaxLength(20);
            entity.Property(e => e.WeightTolerancePercent).HasColumnName("weight_tolerance_percent").HasPrecision(5, 2);

            // Metadata
            entity.Property(e => e.Metadata).HasColumnName("metadata").HasColumnType("jsonb");

            // Audit
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedByUserId).HasColumnName("updated_by_user_id");

            // Indexes
            entity.HasIndex(e => new { e.SiteId, e.Status });
            entity.HasIndex(e => new { e.SiteId, e.Sku }).IsUnique().HasFilter("sku IS NOT NULL");
            entity.HasIndex(e => new { e.SiteId, e.Barcode }).IsUnique().HasFilter("barcode IS NOT NULL");
            entity.HasIndex(e => new { e.SiteId, e.Category });
            entity.HasIndex(e => new { e.SiteId, e.MetrcItemId }).HasFilter("metrc_item_id IS NOT NULL");
        });
    }
}



