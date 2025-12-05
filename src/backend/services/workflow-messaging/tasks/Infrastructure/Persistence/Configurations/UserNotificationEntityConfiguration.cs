using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harvestry.Tasks.Infrastructure.Persistence.Configurations;

public sealed class UserNotificationEntityConfiguration : IEntityTypeConfiguration<UserNotificationRecord>
{
    public void Configure(EntityTypeBuilder<UserNotificationRecord> builder)
    {
        builder.ToTable("user_notifications");

        builder.HasKey(x => x.NotificationId);

        builder.Property(x => x.NotificationId)
            .HasColumnName("notification_id");

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.SiteId)
            .HasColumnName("site_id")
            .IsRequired();

        builder.Property(x => x.NotificationType)
            .HasColumnName("notification_type")
            .IsRequired();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Message)
            .HasColumnName("message")
            .HasMaxLength(2000);

        builder.Property(x => x.RelatedEntityType)
            .HasColumnName("related_entity_type")
            .HasMaxLength(100);

        builder.Property(x => x.RelatedEntityId)
            .HasColumnName("related_entity_id");

        builder.Property(x => x.IsRead)
            .HasColumnName("is_read")
            .IsRequired();

        builder.Property(x => x.ReadAt)
            .HasColumnName("read_at");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("ix_user_notifications_user_id");

        builder.HasIndex(x => new { x.UserId, x.IsRead })
            .HasDatabaseName("ix_user_notifications_user_unread");

        builder.HasIndex(x => new { x.UserId, x.CreatedAt })
            .HasDatabaseName("ix_user_notifications_user_created");
    }
}

