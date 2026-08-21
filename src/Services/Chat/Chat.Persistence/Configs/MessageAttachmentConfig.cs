using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class MessageAttachmentConfig : IEntityTypeConfiguration<MessageAttachment>
{
    public void Configure(EntityTypeBuilder<MessageAttachment> builder)
    {
        // Table
        builder.ToTable("message_attachments");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type).HasConversion<byte>().IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Size).IsRequired();
        builder.Property(x => x.StorageKey).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Width);
        builder.Property(x => x.Height);
        builder.Property(x => x.Duration);

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x == null ? null : x.ToJson(),
                x => x == null ? null : MetadataBase.FromJson<ChatMetadata>(x))
            .HasColumnType("jsonb");

        // Relationships
        builder.HasOne(x => x.Message)
            .WithMany(x => x.Attachments)
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.MessageId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
