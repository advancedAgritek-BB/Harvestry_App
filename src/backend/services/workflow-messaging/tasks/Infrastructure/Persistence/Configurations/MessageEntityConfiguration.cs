using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harvestry.Tasks.Infrastructure.Persistence.Configurations;

public sealed class MessageEntityConfiguration : IEntityTypeConfiguration<MessageRecord>
{
    public void Configure(EntityTypeBuilder<MessageRecord> builder)
    {
        builder.ToTable("messages");

        builder.HasKey(x => x.MessageId);
        builder.Property(x => x.MessageId).HasColumnName("message_id");
        builder.Property(x => x.SiteId).HasColumnName("site_id");
        builder.Property(x => x.ConversationId).HasColumnName("conversation_id");
        builder.Property(x => x.ParentMessageId).HasColumnName("parent_message_id");
        builder.Property(x => x.SenderUserId).HasColumnName("sender_user_id");
        builder.Property(x => x.Content).HasColumnName("content");
        builder.Property(x => x.IsEdited).HasColumnName("is_edited");
        builder.Property(x => x.EditedAt).HasColumnName("edited_at");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.MetadataJson).HasColumnName("metadata");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(x => x.Conversation)
            .WithMany(x => x.Messages)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ParentMessage)
            .WithMany()
            .HasForeignKey(x => x.ParentMessageId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Attachments)
            .WithOne(x => x.Message)
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ReadReceipts)
            .WithOne(x => x.Message)
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
