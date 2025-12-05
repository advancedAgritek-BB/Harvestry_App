using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harvestry.Tasks.Infrastructure.Persistence.Configurations;

public sealed class StandardOperatingProcedureEntityConfiguration : IEntityTypeConfiguration<StandardOperatingProcedureRecord>
{
    public void Configure(EntityTypeBuilder<StandardOperatingProcedureRecord> builder)
    {
        builder.ToTable("standard_operating_procedures");

        builder.HasKey(x => x.SopId);

        builder.Property(x => x.SopId)
            .HasColumnName("sop_id");

        builder.Property(x => x.OrgId)
            .HasColumnName("org_id")
            .IsRequired();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Content)
            .HasColumnName("content");

        builder.Property(x => x.Category)
            .HasColumnName("category")
            .HasMaxLength(100);

        builder.Property(x => x.Version)
            .HasColumnName("version")
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(x => x.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.OrgId)
            .HasDatabaseName("ix_sops_org_id");

        builder.HasIndex(x => new { x.OrgId, x.IsActive })
            .HasDatabaseName("ix_sops_org_active");

        builder.HasIndex(x => new { x.OrgId, x.Category })
            .HasDatabaseName("ix_sops_org_category");
    }
}

