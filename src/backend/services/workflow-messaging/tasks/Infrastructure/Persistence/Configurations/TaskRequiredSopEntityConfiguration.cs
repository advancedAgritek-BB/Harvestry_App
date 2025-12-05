using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harvestry.Tasks.Infrastructure.Persistence.Configurations;

public sealed class TaskRequiredSopEntityConfiguration : IEntityTypeConfiguration<TaskRequiredSopRecord>
{
    public void Configure(EntityTypeBuilder<TaskRequiredSopRecord> builder)
    {
        builder.ToTable("task_required_sops");

        builder.HasKey(x => x.TaskRequiredSopId);

        builder.Property(x => x.TaskRequiredSopId)
            .HasColumnName("task_required_sop_id");

        builder.Property(x => x.TaskId)
            .HasColumnName("task_id")
            .IsRequired();

        builder.Property(x => x.SopId)
            .HasColumnName("sop_id")
            .IsRequired();

        builder.HasOne(x => x.Task)
            .WithMany(t => t.RequiredSops)
            .HasForeignKey(x => x.TaskId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.TaskId, x.SopId })
            .IsUnique()
            .HasDatabaseName("ux_task_required_sops_unique");
    }
}
