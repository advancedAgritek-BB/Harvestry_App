using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harvestry.Tasks.Infrastructure.Persistence.Configurations;

public sealed class TaskDependencyEntityConfiguration : IEntityTypeConfiguration<TaskDependencyRecord>
{
    public void Configure(EntityTypeBuilder<TaskDependencyRecord> builder)
    {
        builder.ToTable("task_dependencies");

        builder.HasKey(x => x.TaskDependencyId);

        builder.Property(x => x.TaskDependencyId)
            .HasColumnName("task_dependency_id");

        builder.Property(x => x.TaskId)
            .HasColumnName("task_id")
            .IsRequired();

        builder.Property(x => x.DependsOnTaskId)
            .HasColumnName("depends_on_task_id")
            .IsRequired();

        builder.Property(x => x.DependencyType)
            .HasColumnName("dependency_type")
            .IsRequired();

        builder.Property(x => x.IsBlocking)
            .HasColumnName("is_blocking")
            .IsRequired();

        builder.Property(x => x.MinimumLag)
            .HasColumnName("minimum_lag");

        builder.HasOne(x => x.Task)
            .WithMany(t => t.Dependencies)
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_task_dependencies_task");

        builder.HasOne<TaskRecord>()
            .WithMany()
            .HasForeignKey(x => x.DependsOnTaskId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_task_dependencies_depends_on");

        builder.HasIndex(x => new { x.TaskId, x.DependsOnTaskId })
            .IsUnique()
            .HasDatabaseName("ux_task_dependencies_unique_link");
    }
}
