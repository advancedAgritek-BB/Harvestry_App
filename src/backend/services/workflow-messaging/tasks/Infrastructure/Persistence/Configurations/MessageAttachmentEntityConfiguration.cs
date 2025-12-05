using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harvestry.Tasks.Infrastructure.Persistence.Configurations;

public sealed class MessageAttachmentEntityConfiguration : IEntityTypeConfiguration<MessageAttachmentRecord>
{
    public void Configure(EntityTypeBuilder<MessageAttachmentRecord> builder)
    {
        builder.ToTable("message_attachments");

        builder.HasKey(x => x.MessageAttachmentId);
        builder.Property(x => x.MessageAttachmentId).HasColumnName("message_attachment_id");
        builder.Property(x => x.MessageId).HasColumnName("message_id");
        builder.Property(x => x.AttachmentType).HasColumnName("attachment_type");
        builder.Property(x => x.FileName).HasColumnName("file_name").HasMaxLength(500);
        builder.Property(x => x.FileUrl).HasColumnName("file_url");
        builder.Property(x => x.FileSizeBytes).HasColumnName("file_size_bytes");
        builder.Property(x => x.MimeType).HasColumnName("mime_type").HasMaxLength(150);
        builder.Property(x => x.MetadataJson).HasColumnName("metadata");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasOne(x => x.Message)
            .WithMany(x => x.Attachments)
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
