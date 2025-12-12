using Harvestry.ProcessingJobs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.ProcessingJobs.Infrastructure.Persistence;

/// <summary>
/// DbContext for ProcessingJobs domain
/// </summary>
public class ProcessingJobsDbContext : DbContext
{
    public ProcessingJobsDbContext(DbContextOptions<ProcessingJobsDbContext> options) : base(options) { }

    public DbSet<ProcessingJob> ProcessingJobs => Set<ProcessingJob>();
    public DbSet<ProcessingJobType> ProcessingJobTypes => Set<ProcessingJobType>();
    public DbSet<ProcessingJobInput> ProcessingJobInputs => Set<ProcessingJobInput>();
    public DbSet<ProcessingJobOutput> ProcessingJobOutputs => Set<ProcessingJobOutput>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProcessingJob>(entity =>
        {
            entity.ToTable("processing_jobs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.JobNumber).HasColumnName("job_number").HasMaxLength(50);
            entity.Property(e => e.ProcessingJobTypeId).HasColumnName("processing_job_type_id");
            entity.Property(e => e.ProcessingJobTypeName).HasColumnName("processing_job_type_name").HasMaxLength(100);
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.ExpectedEndDate).HasColumnName("expected_end_date");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.MetrcJobId).HasColumnName("metrc_job_id");
            entity.Property(e => e.MetrcSyncStatus).HasColumnName("metrc_sync_status").HasMaxLength(30);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedByUserId).HasColumnName("updated_by_user_id");

            entity.HasMany(e => e.Inputs).WithOne().HasForeignKey(i => i.ProcessingJobId);
            entity.HasMany(e => e.Outputs).WithOne().HasForeignKey(o => o.ProcessingJobId);

            entity.HasIndex(e => new { e.SiteId, e.JobNumber }).IsUnique();
            entity.HasIndex(e => new { e.SiteId, e.Status });
        });

        modelBuilder.Entity<ProcessingJobType>(entity =>
        {
            entity.ToTable("processing_job_types");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SiteId).HasColumnName("site_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.DefaultOutputCategory).HasColumnName("default_output_category").HasMaxLength(50);
            entity.Property(e => e.ExpectedYieldPercent).HasColumnName("expected_yield_percent").HasPrecision(5, 2);
            entity.Property(e => e.EstimatedDurationHours).HasColumnName("estimated_duration_hours");
        });

        modelBuilder.Entity<ProcessingJobInput>(entity =>
        {
            entity.ToTable("processing_job_inputs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProcessingJobId).HasColumnName("processing_job_id");
            entity.Property(e => e.PackageId).HasColumnName("package_id");
            entity.Property(e => e.PackageLabel).HasColumnName("package_label").HasMaxLength(30);
            entity.Property(e => e.ItemName).HasColumnName("item_name").HasMaxLength(200);
            entity.Property(e => e.Quantity).HasColumnName("quantity").HasPrecision(12, 4);
            entity.Property(e => e.UnitOfMeasure).HasColumnName("unit_of_measure").HasMaxLength(20);
            entity.Property(e => e.UnitCost).HasColumnName("unit_cost").HasPrecision(12, 4);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<ProcessingJobOutput>(entity =>
        {
            entity.ToTable("processing_job_outputs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProcessingJobId).HasColumnName("processing_job_id");
            entity.Property(e => e.PackageId).HasColumnName("package_id");
            entity.Property(e => e.PackageLabel).HasColumnName("package_label").HasMaxLength(30);
            entity.Property(e => e.ItemName).HasColumnName("item_name").HasMaxLength(200);
            entity.Property(e => e.Quantity).HasColumnName("quantity").HasPrecision(12, 4);
            entity.Property(e => e.UnitOfMeasure).HasColumnName("unit_of_measure").HasMaxLength(20);
            entity.Property(e => e.IsWaste).HasColumnName("is_waste");
            entity.Property(e => e.WasteType).HasColumnName("waste_type").HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });
    }
}




