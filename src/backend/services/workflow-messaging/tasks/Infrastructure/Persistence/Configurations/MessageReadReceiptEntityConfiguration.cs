using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harvestry.Tasks.Infrastructure.Persistence.Configurations;

public sealed class MessageReadReceiptEntityConfiguration : IEntityTypeConfiguration<MessageReadReceiptRecord>
{
    public void Configure(EntityTypeBuilder<MessageReadReceiptRecord> builder)
    {
        builder.ToTable("message_read_receipts");

        builder.HasKey(x => x.MessageReadReceiptId);
        builder.Property(x => x.MessageReadReceiptId).HasColumnName("message_read_receipt_id");
        builder.Property(x => x.MessageId).HasColumnName("message_id");
        builder.Property(x => x.SiteId).HasColumnName("site_id");
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.ReadAt).HasColumnName("read_at");

        builder.HasIndex(x => new { x.MessageId, x.UserId })
            .IsUnique()
            .HasDatabaseName("ux_message_read_receipts_message_id_user_id");

        builder.HasIndex(x => x.MessageId)
            .HasDatabaseName("ix_message_read_receipts_message_id");

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("ix_message_read_receipts_user_id");

        builder.HasOne(x => x.Message)
            .WithMany(x => x.ReadReceipts)
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
