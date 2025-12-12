using Harvestry.Compliance.Metrc.Domain.Entities;
using Harvestry.Compliance.Metrc.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Compliance.Metrc.Infrastructure.Persistence;

/// <summary>
/// DbContext for METRC compliance data
/// </summary>
public sealed class MetrcDbContext : DbContext
{
    public MetrcDbContext(DbContextOptions<MetrcDbContext> options) : base(options)
    {
    }

    public DbSet<MetrcLicense> Licenses => Set<MetrcLicense>();
    public DbSet<MetrcSyncJob> SyncJobs => Set<MetrcSyncJob>();
    public DbSet<MetrcQueueItem> QueueItems => Set<MetrcQueueItem>();
    public DbSet<MetrcSyncCheckpoint> SyncCheckpoints => Set<MetrcSyncCheckpoint>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureLicense(modelBuilder);
        ConfigureSyncJob(modelBuilder);
        ConfigureQueueItem(modelBuilder);
        ConfigureSyncCheckpoint(modelBuilder);
    }

    private static void ConfigureLicense(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MetrcLicense>(entity =>
        {
            entity.ToTable("metrc_licenses");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SiteId).HasColumnName("site_id").IsRequired();
            entity.Property(e => e.LicenseNumber).HasColumnName("license_number").HasMaxLength(50).IsRequired();
            entity.Property(e => e.StateCode).HasColumnName("state_code").HasMaxLength(2).IsRequired();
            entity.Property(e => e.FacilityName).HasColumnName("facility_name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.VendorApiKeyEncrypted).HasColumnName("vendor_api_key_encrypted").HasMaxLength(500);
            entity.Property(e => e.UserApiKeyEncrypted).HasColumnName("user_api_key_encrypted").HasMaxLength(500);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.UseSandbox).HasColumnName("use_sandbox").HasDefaultValue(false);
            entity.Property(e => e.AutoSyncEnabled).HasColumnName("auto_sync_enabled").HasDefaultValue(true);
            entity.Property(e => e.SyncIntervalMinutes).HasColumnName("sync_interval_minutes").HasDefaultValue(15);
            entity.Property(e => e.LastSyncAt).HasColumnName("last_sync_at");
            entity.Property(e => e.LastSuccessfulSyncAt).HasColumnName("last_successful_sync_at");
            entity.Property(e => e.LastSyncError).HasColumnName("last_sync_error").HasMaxLength(2000);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired();
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.UpdatedByUserId).HasColumnName("updated_by_user_id");

            entity.HasIndex(e => e.LicenseNumber).IsUnique();
            entity.HasIndex(e => e.SiteId);
            entity.HasIndex(e => new { e.IsActive, e.AutoSyncEnabled });
        });
    }

    private static void ConfigureSyncJob(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MetrcSyncJob>(entity =>
        {
            entity.ToTable("metrc_sync_jobs");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SiteId).HasColumnName("site_id").IsRequired();
            entity.Property(e => e.LicenseNumber).HasColumnName("license_number").HasMaxLength(50).IsRequired();
            entity.Property(e => e.StateCode).HasColumnName("state_code").HasMaxLength(2).IsRequired();
            entity.Property(e => e.Direction).HasColumnName("direction").HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.StartedAt).HasColumnName("started_at");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.LastHeartbeatAt).HasColumnName("last_heartbeat_at");
            entity.Property(e => e.TotalItems).HasColumnName("total_items").HasDefaultValue(0);
            entity.Property(e => e.ProcessedItems).HasColumnName("processed_items").HasDefaultValue(0);
            entity.Property(e => e.SuccessfulItems).HasColumnName("successful_items").HasDefaultValue(0);
            entity.Property(e => e.FailedItems).HasColumnName("failed_items").HasDefaultValue(0);
            entity.Property(e => e.RetryCount).HasColumnName("retry_count").HasDefaultValue(0);
            entity.Property(e => e.MaxRetries).HasColumnName("max_retries").HasDefaultValue(3);
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message").HasMaxLength(2000);
            entity.Property(e => e.ErrorDetails).HasColumnName("error_details");
            entity.Property(e => e.InitiatedBy).HasColumnName("initiated_by").HasMaxLength(50);
            entity.Property(e => e.InitiatedByUserId).HasColumnName("initiated_by_user_id");

            entity.HasIndex(e => e.LicenseNumber);
            entity.HasIndex(e => e.SiteId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    private static void ConfigureQueueItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MetrcQueueItem>(entity =>
        {
            entity.ToTable("metrc_queue_items");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SyncJobId).HasColumnName("sync_job_id").IsRequired();
            entity.Property(e => e.SiteId).HasColumnName("site_id").IsRequired();
            entity.Property(e => e.LicenseNumber).HasColumnName("license_number").HasMaxLength(50).IsRequired();
            entity.Property(e => e.EntityType).HasColumnName("entity_type").HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.OperationType).HasColumnName("operation_type").HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.HarvestryEntityId).HasColumnName("harvestry_entity_id").IsRequired();
            entity.Property(e => e.MetrcId).HasColumnName("metrc_id");
            entity.Property(e => e.MetrcLabel).HasColumnName("metrc_label").HasMaxLength(50);
            entity.Property(e => e.PayloadJson).HasColumnName("payload_json").IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.Priority).HasColumnName("priority").HasDefaultValue(100);
            entity.Property(e => e.RetryCount).HasColumnName("retry_count").HasDefaultValue(0);
            entity.Property(e => e.MaxRetries).HasColumnName("max_retries").HasDefaultValue(3);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.ScheduledAt).HasColumnName("scheduled_at");
            entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message").HasMaxLength(2000);
            entity.Property(e => e.ErrorCode).HasColumnName("error_code").HasMaxLength(50);
            entity.Property(e => e.ResponseJson).HasColumnName("response_json");
            entity.Property(e => e.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(200);
            entity.Property(e => e.DependsOnItemId).HasColumnName("depends_on_item_id");

            entity.HasIndex(e => e.SyncJobId);
            entity.HasIndex(e => e.LicenseNumber);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IdempotencyKey).IsUnique();
            entity.HasIndex(e => new { e.LicenseNumber, e.Status, e.Priority, e.ScheduledAt });
        });
    }

    private static void ConfigureSyncCheckpoint(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MetrcSyncCheckpoint>(entity =>
        {
            entity.ToTable("metrc_sync_checkpoints");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LicenseId).HasColumnName("license_id").IsRequired();
            entity.Property(e => e.EntityType).HasColumnName("entity_type").HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.Direction).HasColumnName("direction").HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.LastSyncTimestamp).HasColumnName("last_sync_timestamp");
            entity.Property(e => e.LastSyncedMetrcId).HasColumnName("last_synced_metrc_id");
            entity.Property(e => e.LastSyncItemCount).HasColumnName("last_sync_item_count").HasDefaultValue(0);
            entity.Property(e => e.LastSuccessfulSyncAt).HasColumnName("last_successful_sync_at");
            entity.Property(e => e.LastFailedSyncAt).HasColumnName("last_failed_sync_at");
            entity.Property(e => e.LastError).HasColumnName("last_error").HasMaxLength(2000);
            entity.Property(e => e.ConsecutiveFailures).HasColumnName("consecutive_failures").HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired();

            entity.HasIndex(e => new { e.LicenseId, e.EntityType, e.Direction }).IsUnique();
        });
    }
}
