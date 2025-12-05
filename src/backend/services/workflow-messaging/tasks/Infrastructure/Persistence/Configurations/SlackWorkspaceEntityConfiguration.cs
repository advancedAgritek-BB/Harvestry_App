using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harvestry.Tasks.Infrastructure.Persistence.Configurations;

public sealed class SlackWorkspaceEntityConfiguration : IEntityTypeConfiguration<SlackWorkspaceRecord>
{
    public void Configure(EntityTypeBuilder<SlackWorkspaceRecord> builder)
    {
        builder.ToTable("slack_workspaces", schema: "tasks");

        builder.HasKey(x => x.SlackWorkspaceId);

        builder.Property(x => x.SlackWorkspaceId)
            .HasColumnName("slack_workspace_id");

        builder.Property(x => x.SiteId)
            .HasColumnName("site_id")
            .IsRequired();

        builder.Property(x => x.WorkspaceId)
            .HasColumnName("workspace_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.WorkspaceName)
            .HasColumnName("workspace_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.BotTokenEncrypted)
            .HasColumnName("bot_token_encrypted")
            .HasMaxLength(4000);

        builder.Property(x => x.RefreshTokenEncrypted)
            .HasColumnName("refresh_token_encrypted")
            .HasMaxLength(4000);

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(x => x.InstalledByUserId)
            .HasColumnName("installed_by_user_id")
            .IsRequired();

        builder.Property(x => x.InstalledAt)
            .HasColumnName("installed_at")
            .IsRequired();

        builder.Property(x => x.LastVerifiedAt)
            .HasColumnName("last_verified_at");

        builder.HasMany(x => x.ChannelMappings)
            .WithOne(x => x.SlackWorkspace)
            .HasForeignKey(x => x.SlackWorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.SiteId, x.WorkspaceId })
            .IsUnique();
    }
}
